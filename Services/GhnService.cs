using ECommerceWeb.ViewModels;
using Newtonsoft.Json;
using RestSharp;

namespace ECommerceWeb.Services
{
    public class GhnService : IGhnService
    {
        private readonly IConfiguration _config;

        public GhnService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<int> CalculateFeeAsync(int toDistrictId, string toWardCode)
        {
            var apiUrl = _config["GhnAPI:ApiUrl"];
            var token = _config["GhnAPI:Token"];
            var shopId = _config["GhnAPI:ShopId"];

            var client = new RestClient(apiUrl);
            var request = new RestRequest() { Method = Method.Post };

            // Thêm các Headers bắt buộc của GHN
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Token", token);
            request.AddHeader("ShopId", shopId);

            // Khởi tạo thông tin gói hàng (Mặc định gói 500g, 10x10x10cm)
            var requestData = new GhnFeeRequestModel
            {
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                weight = 500,
                length = 15,
                width = 15,
                height = 10
            };

            request.AddParameter("application/json", JsonConvert.SerializeObject(requestData), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);

            if (response.IsSuccessful)
            {
                var result = JsonConvert.DeserializeObject<GhnFeeResponseModel>(response.Content);
                if (result != null && result.code == 200)
                {
                    return result.data.total; // Trả về phí vận chuyển
                }
            }

            return 0; // Trả về 0 nếu có lỗi
        }
    }
}