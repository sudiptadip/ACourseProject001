using Blog.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Utility.Service.IService
{
    public interface IEasebuzzPaymentService
    {
        public Task<string> InitiatePayment(PaymentModel payment);
    }
}
