using ECommerceWeb.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceWeb.Services
{
    public class MomoService : IMomoService
    {
        private readonly MomoOptionModel _options;

        public MomoService(IOptions<MomoOptionModel> options)
        {
            _options = options.Value;
        }

        public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(
            PaymentInformationModel model)
        {
            if (string.IsNullOrWhiteSpace(model.OrderId))
            {
                throw new ArgumentException("OrderId không hợp lệ.");
            }

            long amount = Convert.ToInt64(
                Math.Round(model.Amount, MidpointRounding.AwayFromZero));

            if (amount <= 0)
            {
                throw new InvalidOperationException(
                    "Số tiền thanh toán MoMo không hợp lệ.");
            }

            string requestId = Guid.NewGuid().ToString("N");

            // MoMo yêu cầu orderId duy nhất cho mỗi lần tạo giao dịch.
            // Phần trước dấu '-' vẫn là OrderID thật trong database.
            string momoOrderId =
                $"{model.OrderId}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";

            string orderInfo =
                $"Thanh toan MoMo don hang: {model.OrderId}";

            string extraData = string.Empty;

            string rawData =
                $"accessKey={_options.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&ipnUrl={_options.NotifyUrl}" +
                $"&orderId={momoOrderId}" +
                $"&orderInfo={orderInfo}" +
                $"&partnerCode={_options.PartnerCode}" +
                $"&redirectUrl={_options.ReturnUrl}" +
                $"&requestId={requestId}" +
                $"&requestType={_options.RequestType}";

            string signature = ComputeHmacSha256(
                rawData,
                _options.SecretKey);

            var requestData = new
            {
                partnerCode = _options.PartnerCode,
                partnerName = "TechStore",
                storeId = "TechStore",
                requestId,
                amount,
                orderId = momoOrderId,
                orderInfo,
                redirectUrl = _options.ReturnUrl,
                ipnUrl = _options.NotifyUrl,
                lang = "vi",
                extraData,
                requestType = _options.RequestType,
                signature
            };

            using var client = new RestClient(_options.MomoApiUrl);
            var request = new RestRequest("", Method.Post);

            request.AddHeader(
                "Content-Type",
                "application/json; charset=UTF-8");

            request.AddJsonBody(requestData);

            Console.WriteLine("========== MOMO REQUEST ==========");
            Console.WriteLine($"API URL: {_options.MomoApiUrl}");
            Console.WriteLine($"PartnerCode: {_options.PartnerCode}");
            Console.WriteLine($"RequestType: {_options.RequestType}");
            Console.WriteLine($"ReturnUrl: {_options.ReturnUrl}");
            Console.WriteLine($"NotifyUrl: {_options.NotifyUrl}");
            Console.WriteLine($"Internal OrderId: {model.OrderId}");
            Console.WriteLine($"MoMo OrderId: {momoOrderId}");
            Console.WriteLine($"Amount: {amount}");
            Console.WriteLine($"RequestId: {requestId}");
            Console.WriteLine($"RawData: {rawData}");

            var response = await client.ExecuteAsync(request);

            Console.WriteLine("========== MOMO RESPONSE ==========");
            Console.WriteLine($"HTTP: {(int)response.StatusCode}");
            Console.WriteLine($"Content: {response.Content}");
            Console.WriteLine($"Error: {response.ErrorMessage}");

            if (string.IsNullOrWhiteSpace(response.Content))
            {
                throw new InvalidOperationException(
                    $"MoMo không trả về nội dung. " +
                    $"HTTP: {(int)response.StatusCode}. " +
                    $"Lỗi kết nối: {response.ErrorMessage}");
            }

            MomoCreatePaymentResponseModel? momoResponse;

            try
            {
                momoResponse =
                    JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(
                        response.Content);
            }
            catch (JsonException)
            {
                throw new InvalidOperationException(
                    $"Phản hồi MoMo không phải JSON hợp lệ: {response.Content}");
            }

            if (momoResponse == null)
            {
                throw new InvalidOperationException(
                    $"Không đọc được phản hồi MoMo: {response.Content}");
            }

            if (!response.IsSuccessful ||
                momoResponse.ResultCode != 0 ||
                string.IsNullOrWhiteSpace(momoResponse.PayUrl))
            {
                throw new InvalidOperationException(
                    momoResponse.Message ??
                    $"MoMo từ chối giao dịch. HTTP: {(int)response.StatusCode}");
            }

            return momoResponse;
        }

        public PaymentResponseModel PaymentExecuteAsync(
            IQueryCollection collection)
        {
            string partnerCode = collection["partnerCode"].ToString();
            string orderId = collection["orderId"].ToString();
            string requestId = collection["requestId"].ToString();
            string amount = collection["amount"].ToString();
            string orderInfo = collection["orderInfo"].ToString();
            string orderType = collection["orderType"].ToString();
            string transId = collection["transId"].ToString();
            string resultCode = collection["resultCode"].ToString();
            string message = collection["message"].ToString();
            string payType = collection["payType"].ToString();
            string responseTime = collection["responseTime"].ToString();
            string extraData = collection["extraData"].ToString();
            string receivedSignature = collection["signature"].ToString();

            string rawData =
                $"accessKey={_options.AccessKey}" +
                $"&amount={amount}" +
                $"&extraData={extraData}" +
                $"&message={message}" +
                $"&orderId={orderId}" +
                $"&orderInfo={orderInfo}" +
                $"&orderType={orderType}" +
                $"&partnerCode={partnerCode}" +
                $"&payType={payType}" +
                $"&requestId={requestId}" +
                $"&responseTime={responseTime}" +
                $"&resultCode={resultCode}" +
                $"&transId={transId}";

            string calculatedSignature =
                ComputeHmacSha256(rawData, _options.SecretKey);

            bool validSignature =
                !string.IsNullOrWhiteSpace(receivedSignature) &&
                CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(calculatedSignature),
                    Encoding.UTF8.GetBytes(receivedSignature));

            long.TryParse(
                amount,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long parsedAmount);

            return new PaymentResponseModel
            {
                Success = validSignature && resultCode == "0",
                PaymentMethod = "MoMo",
                OrderDescription = orderInfo,
                OrderId = orderId,
                TransactionId = transId,
                ResponseCode = resultCode,
                Amount = parsedAmount
            };
        }

        private static string ComputeHmacSha256(
            string message,
            string secretKey)
        {
            byte[] keyBytes = Encoding.UTF8.GetBytes(secretKey);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            using var hmac = new HMACSHA256(keyBytes);
            byte[] hashBytes = hmac.ComputeHash(messageBytes);

            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }
    }
}
