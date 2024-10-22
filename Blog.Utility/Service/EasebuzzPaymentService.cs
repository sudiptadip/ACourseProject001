using Blog.Models.Models;
using Blog.Utility.Service.IService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Blog.Utility.Service
{
    public class EasebuzzPaymentService 
    {
        private readonly HttpClient _httpClient;

        public EasebuzzPaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> InitiatePayment(PaymentModel payment)
        {
            var requestUrl = "https://pay.easebuzz.in/payment/initiate";
            var response = await _httpClient.PostAsJsonAsync(requestUrl, payment);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsStringAsync(); // Process the response
            }
            return null;
        }
    }
}
