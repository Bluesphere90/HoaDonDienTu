using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoaDonDienTu.Helper
{
    public static class FormatHelper
    {
        public static string FormatInvoiceDate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return string.Empty;
            if (DateTime.TryParse(isoDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AllowWhiteSpaces, out DateTime date))
            {
                return date.ToLocalTime().ToString("dd/MM/yyyy");
            }
            // Thử parse nếu chỉ có ngày tháng năm
            if (DateTime.TryParseExact(isoDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date.ToString("dd/MM/yyyy");
            }
            return isoDate; // Trả về gốc nếu không parse được
        }

        public static string FormatCurrency(decimal? amount, int decimalPlaces = 0)
        {
            if (!amount.HasValue) return string.Empty; // Hoặc "0" tùy yêu cầu
            return amount.Value.ToString($"N{decimalPlaces}", CultureInfo.GetCultureInfo("vi-VN"));
        }

        // Dùng cho InvoiceSummary.TongTien (là string)
        public static string FormatCurrencyFromString(string amountStr, int decimalPlaces = 0)
        {
            if (string.IsNullOrEmpty(amountStr)) return string.Empty; // Hoặc "0"
            if (decimal.TryParse(amountStr, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal value))
            {
                return value.ToString($"N{decimalPlaces}", CultureInfo.GetCultureInfo("vi-VN"));
            }
            return amountStr; // Trả về gốc nếu không parse được
        }


        public static string GetInvoiceStatusDescription(string statusCode)
        {
            if (string.IsNullOrEmpty(statusCode)) return "Không xác định";
            switch (statusCode)
            {
                case "1": return "Hóa đơn mới";
                case "2": return "Hóa đơn thay thế";
                case "3": return "Hóa đơn điều chỉnh";
                case "4": return "Hóa đơn bị thay thế";
                case "5": return "Hóa đơn đã bị điều chỉnh";
                case "6": return "Hóa đơn bị hủy";
                default: return $"Mã không xác định ({statusCode})";

                    // Danh sách tthai từ tài liệu API
            }
        }

        public static string GetInvoiceProcessingStatusDescription(string statusCode)
        {
            if (string.IsNullOrEmpty(statusCode)) return "Chưa có";
            // Đây là mã ttxly - cần tài liệu chính xác từ GDT hoặc dựa trên các mẫu
            switch (statusCode)
            {
                case "0": return "Tổng cục thuế đã nhận";
                case "1": return "Đang tiến hành kiểm tra điều kiện cấp mã";
                case "2": return "CQT từ chối theo từng lần phát sinh";
                case "3": return "Hóa đơn đủ điều kiện cấp mã";
                case "4": return "Hóa đơn không đủ điều kiện cấp mã";
                case "5": return "Đã cấp mã hóa đơn";
                case "6": return "Tổng cục thuế đã nhận không mã";
                case "7": return "Đã kiểm tra HĐĐT định kỳ không có mã";
                case "8": return "Tổng cục thuế đã nhận hóa đơn có mã khởi tạo từ máy tính tiền";
                // Thêm các case khác dựa trên tài liệu API
                default: return $"Mã không xác định ({statusCode})";

            }
        }
    }
}
