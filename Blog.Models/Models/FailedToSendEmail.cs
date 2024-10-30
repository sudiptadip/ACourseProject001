using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class FailedToSendEmail
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Email { get; set; }
        public string Category { get; set; }
    }
}
