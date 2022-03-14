using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DigitusProject.WebUI.Models
{
    public class AboutTheUser
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime EmailSentDate { get; set; }
        public DateTime? AccountConfirmationDate { get; set; }
    }
}
