using System.Security.Cryptography;


namespace Blog.Utility.Service
{
    public class Easebuzz
    {
        private readonly string _salt;
        private readonly string _key;
        private readonly string _env;
        private readonly bool _enableIframe;

        // Constructor to initialize Easebuzz with key, salt, and environment
        public Easebuzz(string salt, string key, string env, bool enableIframe)
        {
            _salt = salt;
            _key = key;
            _env = env;
            _enableIframe = enableIframe;
        }

        // Method to initiate payment
        public string InitiatePaymentAPI(Dictionary<string, string> parameters)
        {
            string apiUrl = _env == "test" ? "https://testpay.easebuzz.in/payment/initiateLink" : "https://pay.easebuzz.in/payment/initiateLink";
            return MakeApiCall(apiUrl, parameters);
        }

        public string TransactionPaymentAPI(Dictionary<string, string> parameters)
        {
            string apiUrl = "https://pay.easebuzz.in/transaction/v2.1/retrieve";
            return MakeApiCall(apiUrl, parameters);
        }

        // General method for making HTTP requests to the Easebuzz API
        private string MakeApiCall(string apiUrl, Dictionary<string, string> parameters)
        {
            using (var client = new HttpClient())
            {
                var content = new FormUrlEncodedContent(parameters);
                HttpResponseMessage response = client.PostAsync(apiUrl, content).Result;

                if (response.IsSuccessStatusCode)
                {
                    return response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    throw new Exception("Error during Easebuzz API call: " + response.StatusCode.ToString());
                }
            }
        }

        // Encryption method to handle sensitive data (AES encryption example)
        public string Encrypt(string plainText, byte[] key, byte[] iv)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;
                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }
    }
}