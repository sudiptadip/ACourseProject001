using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class ProductPrice
    {
        public int Id { get; set; }
        public string ModeOfLecture { get; set; }   
        public int ValidityInMonths { get; set; }   
        public decimal Views { get; set; }             
        public decimal Price { get; set; }       
        public decimal? DiscountPrice { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }          
        public Product Product { get; set; }
    }
}
