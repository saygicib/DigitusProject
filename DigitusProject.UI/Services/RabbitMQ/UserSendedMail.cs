using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Services.RabbitMQ
{
    public class UserSendedMail
    {
        public string Email { get; set; }
        public string MessageTitle { get; set; }
        public string MessageBody { get; set; }
    }
}
