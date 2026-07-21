namespace ECommerceWeb.ViewModels
{
    public class GhnApiListResponse<T>
    {
        public int code { get; set; }
        public string message { get; set; }
        public List<T> data { get; set; }
    }

    public class GhnProvince
    {
        public int ProvinceID { get; set; }
        public string ProvinceName { get; set; }
    }

    public class GhnDistrict
    {
        public int DistrictID { get; set; }
        public int ProvinceID { get; set; }
        public string DistrictName { get; set; }
    }

    public class GhnWard
    {
        public string WardCode { get; set; }
        public int DistrictID { get; set; }
        public string WardName { get; set; }
    }
}