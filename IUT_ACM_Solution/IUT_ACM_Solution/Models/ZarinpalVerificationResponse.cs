using System.Text.Json;

namespace IUT_ACM_Solution.Models
{
    public class ZarinpalVerificationResponse
    {
        public VerificationData? data { get; set; }
        public JsonElement? errors { get; set; }

        public class VerificationData
        {
            public int code { get; set; }
            public string message { get; set; } = "";
            public long ref_id { get; set; }
            public string? card_pan { get; set; }
            public string? card_hash { get; set; }
            public JsonElement? fee_type { get; set; }
            public JsonElement? fee { get; set; }
        }
    }
}
