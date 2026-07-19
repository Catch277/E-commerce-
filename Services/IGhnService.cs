using ECommerceWeb.ViewModels;

namespace ECommerceWeb.Services
{
    public interface IGhnService
    {
        Task<int> CalculateFeeAsync(int toDistrictId, string toWardCode);
    }
}