using ECommerceWeb.ViewModels;

namespace ECommerceWeb.Services
{
    public interface IGhnService
    {
        Task<int> CalculateFeeAsync(int toDistrictId, string toWardCode);
        Task<List<GhnProvince>> GetProvincesAsync();
        Task<List<GhnDistrict>> GetDistrictsAsync(int provinceId);
        Task<List<GhnWard>> GetWardsAsync(int districtId);
        Task<int> GetAvailableServiceIdAsync(int fromDistrictId, int toDistrictId);
    }
}