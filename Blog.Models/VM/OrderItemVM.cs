using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.VM
{
    public class OrderItemVM
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public bool IsPaymentDone { get; set; }
        public string ProductImage { get; set; }
        public string ModeOfLecture { get; set; }
        public string Attempt { get; set; }
        public string ValidityInMonths { get; set; }
        public string Views { get; set; }
        public int ProductId { get; set; }
    }
}