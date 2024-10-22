using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string ProductImageUrl { get; set; }
        public string ProductDescription { get; set; }
        public bool IsActive { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DefaultPrice { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public bool IsPayment { get; set; } = false;
        public decimal DefaultDiscountPrice { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        [ForeignKey("Category")]
        public int CategoryId { get; set; }
        public Category Category { get; set; }


        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }


        [ForeignKey("Faculty")]
        public int FacultyId { get; set; }
        public Faculty Faculty { get; set; }


        public ICollection<ProductAttribute> ProductAttributes { get; set; }
        public ICollection<ProductOption> Options { get; set; }
        public ICollection<ProductCombination> Combinations { get; set; }

    }
}
