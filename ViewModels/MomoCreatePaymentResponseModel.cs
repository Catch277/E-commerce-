using Newtonsoft.Json;

namespace ECommerceWeb.ViewModels
{
    public class MomoCreatePaymentResponseModel
    {
        [JsonProperty("partnerCode")]
        public string? PartnerCode { get; set; }

        [JsonProperty("requestId")]
        public string? RequestId { get; set; }

        [JsonProperty("orderId")]
        public string? OrderId { get; set; }

        [JsonProperty("amount")]
        public long Amount { get; set; }

        [JsonProperty("responseTime")]
        public long ResponseTime { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }

        [JsonProperty("resultCode")]
        public int ResultCode { get; set; }

        [JsonProperty("payUrl")]
        public string? PayUrl { get; set; }

        [JsonProperty("shortLink")]
        public string? ShortLink { get; set; }

        [JsonProperty("deeplink")]
        public string? Deeplink { get; set; }

        [JsonProperty("qrCodeUrl")]
        public string? QrCodeUrl { get; set; }
    }
}
