using DigitusProject.WebUI.Identity;
using DigitusProject.WebUI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using DigitusProject.WebUI.Extensions;
using DigitusProject.WebUI.Services.RabbitMQ;
using Microsoft.AspNetCore.SignalR;
using DigitusProject.WebUI.Services.Hubs;
using System.Linq;

namespace DigitusProject.WebUI.Controllers
{
    [AutoValidateAntiforgeryToken]
    public class AccountController : Controller
    {
        private readonly ApplicationIdentityDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RabbitMQPublisher _rabbitMQPublisher;
        private readonly IHubContext<MyHub> _hubContext;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, RabbitMQPublisher rabbitMQPublisher, IHubContext<MyHub> hubContext, ApplicationIdentityDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _rabbitMQPublisher = rabbitMQPublisher;
            _hubContext = hubContext;
            _context = context;
        }
        public IActionResult Register()
        {
            return View(new RegisterModel());
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,
                UserName = model.Email,
                Email = model.Email
            };            
            var result = await _userManager.CreateAsync(user, model.Password);            
            if (result.Succeeded)
            {
                var aboutToUser = new AboutTheUser
                {
                    UserId = user.Id,
                    EmailSentDate = System.DateTime.Now
                };
                await _context.AboutTheUsers.AddAsync(aboutToUser);
                _context.SaveChanges();
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callBackUrl = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = token });

                _rabbitMQPublisher.Publish(new UserSendedMail() { Email = model.Email, MessageTitle = "Confirm your email", MessageBody = $"<a href='http://localhost:2822{callBackUrl}'>Click</a> the link to confirm your email account." });

                TempData.Put("message", new ResultMessage() { Title = "Confirm your email", Message = "Click the link to confirm your email account.", Css = "warning" });

                return RedirectToAction("login", "account");
            }

            TempData.Put("message", new ResultMessage() { Title = "Exists", Message = "This email or username exists", Css = "warning" });
            return View(model);
        }
        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            return View(new LoginModel()
            {
                ReturnUrl = returnUrl
            });
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData.Put("message", new ResultMessage() { Title = "Not Found", Message = "User Not Found", Css = "warning" });
                return View(model);
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData.Put("message", new ResultMessage() { Title = "Confirm your account", Message = "Account confirmation required.", Css = "warning" });
                return View(model);
            }


            var result = await _signInManager.PasswordSignInAsync(user, model.Password, true, false);
            if (result.Succeeded)
            {
                await _hubContext.Clients.User(user.Id).SendAsync("IsOnline");
                return Redirect(model.ReturnUrl ?? "~/");
            }
            TempData.Put("message", new ResultMessage() { Title = "Error", Message = "Email or password error.", Css = "danger" });

            return View(model);
        }
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData.Put("message", new ResultMessage() { Title = "Session Closed.", Message = "The session was securely closed.", Css = "warning" });

            return Redirect("~/");
        }
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                TempData.Put("message", new ResultMessage() { Title = "Account Confirmation.", Message = "Your information is incorrect for account confirmation.", Css = "danger" });
                return Redirect("~/");
            }
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    var aboutToUser = _context.AboutTheUsers.FirstOrDefault(x => x.UserId == userId);
                    aboutToUser.AccountConfirmationDate = System.DateTime.Now;
                    _context.AboutTheUsers.Update(aboutToUser);
                    _context.SaveChanges();
                    TempData.Put("message", new ResultMessage() { Title = "Account Confirmation.", Message = "Your account has been successfully approved", Css = "success" });
                    return RedirectToAction("Login");
                }
            }
            TempData.Put("message", new ResultMessage() { Title = "Account Confirmation.", Message = "Your account could not be confirmed", Css = "danger" });
            return View();
        }
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                TempData.Put("message", new ResultMessage() { Title = "Forgot Password", Message = "Your information is incorrect", Css = "danger" });
                return View();
            }
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
            {
                TempData.Put("message", new ResultMessage() { Title = "Forgot Password", Message = "Not user found for this email address.", Css = "danger" });
                return View();
            }
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callBackUrl = Url.Action("ResetPassword", "Account", new
            {
                token = token
            });

            _rabbitMQPublisher.Publish(new UserSendedMail() { Email = email, MessageTitle = "Reset Password", MessageBody = $"<a href='http://localhost:2822{callBackUrl}'>Click</a> the link for reset password." });

            TempData.Put("message", new ResultMessage() { Title = "Forgot Password", Message = "Sended mail for reset password", Css = "danger" });

            return RedirectToAction("Login", "Account");
        }
        [HttpGet]
        public IActionResult ResetPassword(string token)
        {
            if (token == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var model = new ResetPasswordModel { Token = token };
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("Login", "Account");
            }

            return View(dto);
        }
        public async Task<IActionResult> Accessdenied()
        {
            return View();
        }
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.FindByIdAsync(_userManager.GetUserId(User));
            return View(new RegisterModel()
            {
                Name = user.Name,
                Surname = user.Surname,
                Email = user.Email
            });
        }
    }
}
