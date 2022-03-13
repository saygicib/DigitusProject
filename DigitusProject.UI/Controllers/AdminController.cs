using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace DigitusProject.WebUI.Controllers
{
    [Authorize(Roles ="admin")]
    public class AdminController : Controller
    {
    }
}
