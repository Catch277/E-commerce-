using System.Globalization;
using System.Text;

namespace ECommerceWeb.Services
{
    public static class ShippingPolicy
    {
        private static readonly string[] FreeShipKeywords = { "ha noi", "ho chi minh" };

        public static bool IsFreeShipProvince(string provinceName)
        {
            if (string.IsNullOrWhiteSpace(provinceName)) return false;

            var normalized = RemoveDiacritics(provinceName).ToLowerInvariant();
            return FreeShipKeywords.Any(k => normalized.Contains(k));
        }

        private static string RemoveDiacritics(string text)
        {
            var formD = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in formD)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }
}