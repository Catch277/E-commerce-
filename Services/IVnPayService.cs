using ECommerceWeb.ViewModels;

namespace ECommerceWeb.Services
{
    public interface IVnPayService
    {
        string CreatePaymentUrl(HttpContext context, PaymentInformationModel model);
        PaymentResponseModel PaymentExecute(IQueryCollection collections);
    }
}