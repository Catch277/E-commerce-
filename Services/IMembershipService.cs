namespace ECommerceWeb.Services
{
    public interface IMembershipService
    {
        Task<bool> AddPointsAsync(int userId, int orderId, decimal totalAmount);
    }
}