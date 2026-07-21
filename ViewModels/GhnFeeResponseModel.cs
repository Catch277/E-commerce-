namespace ECommerceWeb.ViewModels
{
    public class GhnFeeResponseModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public GhnFeeData data { get; set; }
    }
    public class GhnServiceListResponse
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<GhnServiceOption> data { get; set; }
    }

    public class GhnServiceOption
    {
        public int service_id { get; set; }
        public string short_name { get; set; }
    }

    public class GhnFeeData
    {
        public int total { get; set; } // Tổng phí vận chuyển thực tế
        public int service_fee { get; set; }
    }
}