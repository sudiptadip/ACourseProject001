using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class Category
    {
        [Key] 
        public int Id { get; set; }
        [Required]
        public string CategoryName { get; set; }
        [Required]
        public int SortedOrder { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}