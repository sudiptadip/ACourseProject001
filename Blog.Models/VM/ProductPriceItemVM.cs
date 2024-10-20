using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class ProductPriceItemVM
    {
        public string ModeOfLecture { get; set; }
        public string ValidityInMonths { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public string Views { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }          
        public decimal? DiscountPrice { get; set; }
    }
}
