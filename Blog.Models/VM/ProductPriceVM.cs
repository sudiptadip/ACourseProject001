using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class ProductPriceVM
    {
        public int ProductId { get; set; }
        public List<ProductPriceItemVM> ProductPrices { get; set; }
    }
}
