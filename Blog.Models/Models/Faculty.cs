using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class Faculty
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [Display (Name = "Faculty Name")]
        public string FacultyName { get; set; }
        public string Description { get; set; }
        [Required]
        [Range(1, 10000, ErrorMessage = "Sorted Order must be grater than 0")]
        public int SortedOrder { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }
        [Required]
        public bool IsShowMentorPage { get; set; }

        [Required]
        public bool IsShowMenuPage { get; set; }

        [ValidateNever]
        public DateTime CreatedOn { get; set; }
        [ValidateNever]
        public DateTime ModifiedOn { get; set; }


        [ValidateNever]
        public ICollection<Product> Products { get; set; }
    }
}