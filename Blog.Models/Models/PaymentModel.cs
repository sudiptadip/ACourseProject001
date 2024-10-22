using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Models.Models
{
    public class PaymentModel
    {
        public string Key { get; set; }
        public string TxnId { get; set; }
        public string Amount { get; set; }
        public string ProductInfo { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string SuccessUrl { get; set; }
        public string FailureUrl { get; set; } // Failure URL
        public string Hash { get; set; }
    }
}
