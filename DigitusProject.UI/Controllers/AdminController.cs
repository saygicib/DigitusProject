using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using DigitusProject.WebUI.Models;
using DigitusProject.WebUI.Identity;

namespace DigitusProject.WebUI.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationIdentityDbContext _context;

        public AdminController( ApplicationIdentityDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        public IActionResult Index()
        {
            AboutToUserModel aboutToUserModel = new()
            {
                NumberOfOnlineUsers = NumberOfOnlineUsers(),
                NewRegisteredUser = NewRegisteredUser(),
                SentVerificationCodeButNotConfirmed = SentVerificationCodeButNotConfirmed(),
                AvarageRecordingTime = AvarageRecordingTime()
            };
            return View(aboutToUserModel);
        }
        public int NumberOfOnlineUsers()
        {
            int count = 0;
            count = ClientSource.Clients.Count();
            return count;
        }
        public int NewRegisteredUser()
        {
            int count =0;
            return count = _context.AboutTheUsers.Count(x => x.EmailSentDate.AddDays(1) > DateTime.Now);
        }
        public int SentVerificationCodeButNotConfirmed()
        {
            int count=0;
            return count = _context.AboutTheUsers.Count(x => (x.AccountConfirmationDate > x.EmailSentDate.AddDays(1)) || (x.AccountConfirmationDate==null && x.EmailSentDate.AddDays(1) > DateTime.Now));
        }
        public int AvarageRecordingTime()
        {
            int avarageRecordingTime = 0;
            var registeredUsers = _context.AboutTheUsers.Where(x=>x.AccountConfirmationDate!=null).ToList();
            int emailSentDateSecondSum = registeredUsers.Where(x=>x.AccountConfirmationDate != null).Sum(x => x.EmailSentDate.Second);
            int accountConfirmationSecondSum = (int)registeredUsers.Where(x=>x.AccountConfirmationDate != null).Sum(x=>x.AccountConfirmationDate?.Second);
            if (registeredUsers.Count() == 0)
                return avarageRecordingTime;
            avarageRecordingTime = (accountConfirmationSecondSum - emailSentDateSecondSum) / registeredUsers.Count();
            return avarageRecordingTime;
        }
    }
}
