using System.Text.Json;

namespace IUT_ACM_Solution.Models
{
    public class ZarinpalPaymentResponse
    {
        public PaymentData? data { get; set; }
        public JsonElement? errors { get; set; }  

        public class PaymentData
        {
            public int code { get; set; }
            public string message { get; set; } = "";
            public string authority { get; set; } = "";
            public JsonElement? fee_type { get; set; } 
            public JsonElement? fee { get; set; }       
        }
    }
}
