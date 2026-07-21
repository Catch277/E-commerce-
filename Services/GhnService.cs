using ECommerceWeb.ViewModels;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using RestSharp;

namespace ECommerceWeb.Services
{
    public class GhnService : IGhnService
    {
        private readonly IConfiguration _config;
        private readonly IMemoryCache _cache;
        private readonly ILogger<GhnService> _logger;

        public GhnService(IConfiguration config, IMemoryCache cache, ILogger<GhnService> logger)
        {
            _config = config;
            _cache = cache;
            _logger = logger;
        }

        private RestClient CreateClient(string baseUrl)
        {
            var client = new RestClient(baseUrl);
            return client;
        }

        private RestRequest CreateRequest(string resource, Method method = Method.Get)
        {
            var token = _config["GhnAPI:Token"];
            var shopId = _config["GhnAPI:ShopId"];

            var request = new RestRequest(resource, method);
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Token", token);
            if (!string.IsNullOrWhiteSpace(shopId))
            {
                request.AddHeader("ShopId", shopId);
            }
            return request;
        }

        public async Task<int> CalculateFeeAsync(int toDistrictId, string toWardCode)
        {
            var apiUrl = _config["GhnAPI:ApiUrl"];
            var token = _config["GhnAPI:Token"];
            var shopId = _config["GhnAPI:ShopId"];
            var fromDistrictIdStr = _config["GhnAPI:FromDistrictId"];

            if (string.IsNullOrWhiteSpace(apiUrl) ||
                string.IsNullOrWhiteSpace(token) ||
                string.IsNullOrWhiteSpace(shopId) ||
                string.IsNullOrWhiteSpace(fromDistrictIdStr))
            {
                _logger.LogWarning("Thiếu cấu hình GhnAPI (ApiUrl/Token/ShopId/FromDistrictId).");
                return 0;
            }

            var fromDistrictId = int.Parse(fromDistrictIdStr);

            var serviceId = await GetAvailableServiceIdAsync(fromDistrictId, toDistrictId);
            if (serviceId == 0)
            {
                _logger.LogWarning("Không lấy được service_id cho tuyến {From} -> {To}", fromDistrictId, toDistrictId);
                return 0;
            }

            var client = new RestClient(apiUrl);
            var request = new RestRequest() { Method = Method.Post };

            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Token", token);
            request.AddHeader("ShopId", shopId);

            var requestData = new GhnFeeRequestModel
            {
                from_district_id = fromDistrictId,
                to_district_id = toDistrictId,
                to_ward_code = toWardCode,
                service_id = serviceId,
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
                    return result.data.total;
                }
                _logger.LogWarning("GHN fee trả code khác 200: {Content}", response.Content);
            }
            else
            {
                _logger.LogWarning("GHN fee lỗi HTTP: {StatusCode} - {Content}", response.StatusCode, response.Content);
            }

            return 0;
        }

        // ---- Province/District/Ward ----

        public async Task<List<GhnProvince>> GetProvincesAsync()
        {
            const string cacheKey = "ghn:provinces";
            if (_cache.TryGetValue(cacheKey, out List<GhnProvince> cached))
            {
                return cached;
            }

            var baseUrl = _config["GhnAPI:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                _logger.LogWarning("GhnAPI:BaseUrl chưa được cấu hình.");
                return new List<GhnProvince>();
            }

            var client = CreateClient(baseUrl);
            var request = CreateRequest("/shiip/public-api/master-data/province");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                _logger.LogWarning("GHN GetProvinces lỗi: {StatusCode} - {Content}", response.StatusCode, response.Content);
                return new List<GhnProvince>();
            }

            var result = JsonConvert.DeserializeObject<GhnApiListResponse<GhnProvince>>(response.Content);
            var data = result?.data ?? new List<GhnProvince>();

            _cache.Set(cacheKey, data, TimeSpan.FromHours(24));
            return data;
        }

        public async Task<List<GhnDistrict>> GetDistrictsAsync(int provinceId)
        {
            var cacheKey = $"ghn:districts:{provinceId}";
            if (_cache.TryGetValue(cacheKey, out List<GhnDistrict> cached))
            {
                return cached;
            }

            var baseUrl = _config["GhnAPI:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return new List<GhnDistrict>();

            var client = CreateClient(baseUrl);
            var request = CreateRequest($"/shiip/public-api/master-data/district?province_id={provinceId}");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return new List<GhnDistrict>();

            var result = JsonConvert.DeserializeObject<GhnApiListResponse<GhnDistrict>>(response.Content);
            var data = result?.data ?? new List<GhnDistrict>();

            _cache.Set(cacheKey, data, TimeSpan.FromHours(24));
            return data;
        }

        public async Task<List<GhnWard>> GetWardsAsync(int districtId)
        {
            var cacheKey = $"ghn:wards:{districtId}";
            if (_cache.TryGetValue(cacheKey, out List<GhnWard> cached))
            {
                return cached;
            }

            var baseUrl = _config["GhnAPI:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl)) return new List<GhnWard>();

            var client = CreateClient(baseUrl);
            var request = CreateRequest($"/shiip/public-api/master-data/ward?district_id={districtId}");

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful) return new List<GhnWard>();

            var result = JsonConvert.DeserializeObject<GhnApiListResponse<GhnWard>>(response.Content);
            var data = result?.data ?? new List<GhnWard>();

            // Phường/xã ít khi đổi trong ngắn hạn -> cache vài giờ
            _cache.Set(cacheKey, data, TimeSpan.FromHours(6));
            return data;
        }

        public async Task<int> GetAvailableServiceIdAsync(int fromDistrictId, int toDistrictId)
        {
            var cacheKey = $"ghn:service:{fromDistrictId}:{toDistrictId}";
            if (_cache.TryGetValue(cacheKey, out int cachedServiceId))
            {
                return cachedServiceId;
            }

            var baseUrl = _config["GhnAPI:BaseUrl"];
            var shopId = _config["GhnAPI:ShopId"];

            if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(shopId))
            {
                _logger.LogWarning("Thiếu GhnAPI:BaseUrl hoặc ShopId khi lấy available services.");
                return 0;
            }

            var client = CreateClient(baseUrl);
            var request = CreateRequest("/shiip/public-api/v2/shipping-order/available-services", Method.Post);

            var body = new
            {
                shop_id = int.Parse(shopId),
                from_district = fromDistrictId,
                to_district = toDistrictId
            };
            request.AddParameter("application/json", JsonConvert.SerializeObject(body), ParameterType.RequestBody);

            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                _logger.LogWarning("GHN available-services lỗi: {StatusCode} - {Content}", response.StatusCode, response.Content);
                return 0;
            }

            var result = JsonConvert.DeserializeObject<GhnServiceListResponse>(response.Content);
            var services = result?.data ?? new List<GhnServiceOption>();

            if (!services.Any())
            {
                _logger.LogWarning("GHN không trả về dịch vụ khả dụng nào cho {From} -> {To}", fromDistrictId, toDistrictId);
                return 0;
            }

            // Lấy dịch vụ đầu tiên khả dụng (thường GHN chỉ trả 1-2 lựa chọn: chuẩn/nhanh)
            var serviceId = services.First().service_id;

            // Cache ngắn hạn vì phụ thuộc tuyến từ-đến cụ thể
            _cache.Set(cacheKey, serviceId, TimeSpan.FromHours(6));
            return serviceId;
        }
    }
}