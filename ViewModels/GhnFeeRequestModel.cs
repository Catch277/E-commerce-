namespace ECommerceWeb.ViewModels
{
    public class GhnFeeRequestModel
    {
        public int to_district_id { get; set; }
        public string to_ward_code { get; set; }
        public int height { get; set; } = 10;
        public int length { get; set; } = 10;
        public int weight { get; set; } = 500; // Gram
        public int width { get; set; } = 10;
    }
}