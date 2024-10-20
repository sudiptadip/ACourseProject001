using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class ProductOption
    {
        public int Id { get; set; }
        [Required]
        public string ModeOfLecture { get; set; }
        [Required]
        public string Validity { get; set; }
        [Required]
        public string Views { get; set; }
        [Required]
        public string Attempt { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [ValidateNever]
        public Product Product { get; set; }
    }
}
