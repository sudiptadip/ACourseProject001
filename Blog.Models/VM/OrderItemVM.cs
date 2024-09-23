using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class OrderItemVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductImage { get; set; }
        public string ModeOfLecture { get; set; }
        public string Attempt { get; set; }
        public int ValidityInMonths { get; set; }
        public int Views { get; set; }
        public int ProductId { get; set; }
    }
}