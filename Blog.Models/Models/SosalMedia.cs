using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class SosalMedia
    {
        public int Id { get; set; }
        public string? Facebook { get; set; }
        public string? YouTube { get; set; }
        public string? WhatsApp { get; set; }
        public string? Instagram { get; set; }
        public string? LinkedIn { get; set; }
        public string? Telegram { get; set; }
        public string? Twitter { get; set; }
        [Required]
        public string Mobile { get; set; }
        [Required]
        public string Email { get; set; }
    }
}