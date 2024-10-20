using Blog.Models.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class ProductPriceVM
    {
        [ValidateNever]
        public int ProductId { get; set; }
        [ValidateNever]
        public string ProductName { get; set; }
        public ProductOption ProductOption { get; set; }
        [ValidateNever]
        public List<ProductCombination> Combinations { get; set; } = new List<ProductCombination>();
    }
}