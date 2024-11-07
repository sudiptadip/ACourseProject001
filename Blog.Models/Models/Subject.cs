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
    public class Subject 
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string SubjectName { get; set; }
        [Required]
        public int SortedOrder { get; set; }
        public int CategoryId { get; set; }
        [NotMapped]
        [ValidateNever]
        public string CategoryName { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}