using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Models
{
    public class AboutToUserModel
    {
        public int NumberOfOnlineUsers { get; set; }
        public int NewRegisteredUser { get; set; }
        public int SentVerificationCodeButNotConfirmed { get; set; }
        public int AvarageRecordingTime { get; set; }
    }
}
