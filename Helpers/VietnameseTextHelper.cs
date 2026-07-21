using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ECommerceWeb.Helpers
{
    public static class VietnameseTextHelper
    {
        // Các viết tắt phổ biến người dùng hay gõ, map sang dạng đầy đủ (đã bỏ dấu)
        // Áp dụng SAU khi bỏ dấu, nên value cũng phải viết không dấu
        private static readonly Dictionary<string, string> Abbreviations = new()
        {
            { "hn", "ha noi" },
            { "hcm", "ho chi minh" },
            { "tphcm", "ho chi minh" },
            { "sg", "ho chi minh" }, // Sài Gòn - tên gọi cũ vẫn phổ biến
            { "tp", "thanh pho" },
            { "q1", "quan 1" },
            { "q2", "quan 2" },
            { "q3", "quan 3" },
            { "q7", "quan 7" },
            { "q10", "quan 10" },
        };

        public static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            var normalized = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var result = sb.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace('đ', 'd').Replace('Đ', 'D');

            return result.ToLowerInvariant().Trim();
        }

        /// <summary>
        /// Tách chuỗi tìm kiếm thành từng từ khóa, đã bỏ dấu + expand viết tắt phổ biến.
        /// "Q1 TPHCM" -> ["quan", "1", "ho", "chi", "minh"]
        /// </summary>
        public static string[] SplitKeywords(string input)
        {
            var normalized = RemoveDiacritics(input);
            var rawWords = normalized.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

            var expanded = new List<string>();
            foreach (var word in rawWords)
            {
                if (Abbreviations.TryGetValue(word, out var fullForm))
                {
                    expanded.AddRange(fullForm.Split(' '));
                }
                else
                {
                    expanded.Add(word);
                }
            }

            return expanded.ToArray();
        }
    }
}