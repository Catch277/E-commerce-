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
        public async Task<IActionResult> GetFee(int districtId, string wardCode, string provinceName)
        {
            if (ShippingPolicy.IsFreeShipProvince(provinceName))
            {
                return Json(new { success = true, fee = 0, isFreeShip = true });
            }

            var fee = await _ghnService.CalculateFeeAsync(districtId, wardCode);
            return Json(new { success = true, fee = fee, isFreeShip = false });
        }

        [HttpGet]
        public async Task<IActionResult> GetProvinces()
        {
            var data = await _ghnService.GetProvincesAsync();
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistricts(int provinceId)
        {
            var data = await _ghnService.GetDistrictsAsync(provinceId);
            return Json(data);
        }

        [HttpGet]
        public async Task<IActionResult> GetWards(int districtId)
        {
            var data = await _ghnService.GetWardsAsync(districtId);
            return Json(data);
        }
    }
}