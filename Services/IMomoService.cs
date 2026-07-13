using ECommerceWeb.ViewModels;

namespace ECommerceWeb.Services
{
    public interface IMomoService
    {
        Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(PaymentInformationModel model);
        PaymentResponseModel PaymentExecuteAsync(IQueryCollection collection);
    }
}