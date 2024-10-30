using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class EmailSettings
    {
        public string EmailHost { get; set; }
        public int EmailPort { get; set; }
        public string EmailUsername { get; set; }
        public string EmailPassword { get; set; }
        public bool EnableSSL { get; set; }
        public string SenderName { get; set; }
        public string SenderEmail { get; set; }
    }
}
