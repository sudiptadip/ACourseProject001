using Blog.Utility;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Blog.Utility.Service
{
    public class EasebuzzService
    {
        private readonly string _key;
        private readonly string _salt;
        private readonly string _env;
        private readonly bool _enableIframe;

       
        public string Key => _key;
        public string Salt => _salt;
        public bool EnableIframe => _enableIframe;

        public EasebuzzService(IConfiguration configuration)
        {
            _key = configuration["Easebuzz:Key"];
            _salt = configuration["Easebuzz:Salt"];
            _env = configuration["Easebuzz:Env"];
            _enableIframe = bool.Parse(configuration["Easebuzz:EnableIframe"]);
        }

        public string InitiatePayment(Dictionary<string, string> parameters)
        {
            Easebuzz easebuzz = new Easebuzz(_salt, _key, _env, _enableIframe);
            return easebuzz.InitiatePaymentAPI(parameters);
        }

        public string TransactionPayment(Dictionary<string, string> parameters)
        {
            Easebuzz easebuzz = new Easebuzz(_salt, _key, _env, _enableIframe);
            return easebuzz.TransactionPaymentAPI(parameters);
        }

    }
}