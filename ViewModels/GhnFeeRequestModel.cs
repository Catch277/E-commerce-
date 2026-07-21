namespace ECommerceWeb.ViewModels
{
    public class GhnFeeRequestModel
    {
        public int from_district_id { get; set; }
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; }
        public int height { get; set; } = 10;
        public int length { get; set; } = 10;
        public int weight { get; set; } = 500;
        public int width { get; set; } = 10;
        public int service_id { get; set; }
    }
}
