using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Identity
{
    public class User : IdentityUser
    {
        public string Name { get; set; }
        public string Surname { get; set; }
    }
}
