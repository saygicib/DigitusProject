using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using DigitusProject.WebUI.Models;

namespace DigitusProject.WebUI.Controllers
{
    [Authorize(Roles ="admin")]
    public class AdminController : Controller
    {
        public int NumberOfOnlineUsers()
        {
            int count = 0;
            count = ClientSource.Clients.Count();
            return count;
        }
    }
}
