using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class Achevements
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SortedOrder { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
    }
}