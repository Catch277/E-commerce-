using ECommerceWeb.Services;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceWeb.Controllers
{
    public class ShippingController : Controller
    {
        private readonly IGhnService _ghnService;

        public ShippingController(IGhnService ghnService)
        {
            _ghnService = ghnService;
        }

        [HttpGet]
        public async Task<IActionResult> GetFee(int districtId, string wardCode)
        {
            var fee = await _ghnService.CalculateFeeAsync(districtId, wardCode);
            return Json(new { success = true, fee = fee });
        }
    }
}