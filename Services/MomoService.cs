using ECommerceWeb.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace ECommerceWeb.Services
{
    public class MomoService : IMomoService
    {
        private readonly IOptions<MomoOptionModel> _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(PaymentInformationModel model)
        {
            model.OrderId = DateTime.UtcNow.Ticks.ToString(); // Random ID
            model.OrderDescription = "Thanh toan Momo cho don hang: " + model.OrderId;

            // Thứ tự nối chuỗi phải tuân thủ nghiêm ngặt bảng chữ cái Alphabet (a-z)
            var rawData =
                $"accessKey={_options.Value.AccessKey}" +
                $"&amount={model.Amount}" +
                $"&extraData=" +
                $"&ipnUrl={_options.Value.NotifyUrl}" +
                $"&orderId={model.OrderId}" +
                $"&orderInfo={model.OrderDescription}" +
                $"&partnerCode={_options.Value.PartnerCode}" +
                $"&redirectUrl={_options.Value.ReturnUrl}" +
                $"&requestId={model.OrderId}" +
                $"&requestType={_options.Value.RequestType}";

            var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

            var client = new RestClient(_options.Value.MomoApiUrl);
            var request = new RestRequest() { Method = Method.Post };
            request.AddHeader("Content-Type", "application/json; charset=UTF-8");

            var requestData = new
            {
                partnerCode = _options.Value.PartnerCode,
                partnerName = "TechStore",
                storeId = "MomoTestStore",
                requestId = model.OrderId,
                amount = model.Amount,
                orderId = model.OrderId,
                orderInfo = model.OrderDescription,
                redirectUrl = _options.Value.ReturnUrl,
                ipnUrl = _options.Value.NotifyUrl,
                lang = "vi",
                extraData = "",
                requestType = _options.Value.RequestType,
                signature = signature
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);
            var momoResponse = JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
            return momoResponse;
        }

        public PaymentResponseModel PaymentExecuteAsync(IQueryCollection collection)
        {
            var amount = collection.FirstOrDefault(s => s.Key == "amount").Value.ToString();
            var orderInfo = collection.FirstOrDefault(s => s.Key == "orderInfo").Value.ToString();
            var orderId = collection.FirstOrDefault(s => s.Key == "orderId").Value.ToString();
            var resultCode = collection.FirstOrDefault(s => s.Key == "resultCode").Value.ToString();

            return new PaymentResponseModel
            {
                Success = resultCode == "0", // resultCode = 0 là giao dịch thành công theo chuẩn MoMo
                PaymentMethod = "MoMo",
                OrderDescription = orderInfo,
                OrderId = orderId,
                VnPayResponseCode = resultCode
            };
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);
            byte[] hashBytes;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}