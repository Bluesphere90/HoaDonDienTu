// --- START OF FILE InvoiceModels.cs ---
using System;
using System.Collections.Generic;
using System.Globalization; // Thêm cho CultureInfo
using Newtonsoft.Json;

namespace HoaDonDienTu.Models
{
    // =========================================================================
    // 1. MODELS FOR INVOICE LIST (SUMMARY) - Giữ nguyên từ code của bạn
    // =========================================================================
    public class InvoiceQueryResult
    {
        [JsonProperty("datas")]
        public List<InvoiceSummaryData> Datas { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        public InvoiceQueryResult() // Constructor
        {
            Datas = new List<InvoiceSummaryData>();
        }
    }

    public class InvoiceSummaryData // Dữ liệu thô từ API danh sách hóa đơn
    {
        [JsonProperty("nbmst")]
        public string Nbmst { get; set; }

        [JsonProperty("nbten")]
        public string Nban { get; set; }

        [JsonProperty("khhdon")]
        public string Khhdon { get; set; }

        [JsonProperty("shdon")]
        public string Shdon { get; set; }

        [JsonProperty("khmshdon")]
        public string Khmshdon { get; set; }

        [JsonProperty("tdlap")]
        public string Tdlap { get; set; } // Ngày lập dạng ISO string

        [JsonProperty("tgtttbso")]
        public string Tgtttbso { get; set; } // Tổng tiền dạng string, cần parse

        [JsonProperty("tthai")]
        public string Tthai { get; set; } // Mã trạng thái hóa đơn
    }

    public class InvoiceSummary // Model cho DataGrid dgTongHop
    {
        public int STT { get; set; }
        public string NgayHD { get; set; } // Đã định dạng dd/MM/yyyy
        public string MST { get; set; }
        public string TenDV { get; set; }
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public string TrangThai { get; set; } // Đã diễn giải
        public string TongTien { get; set; } // Đã định dạng tiền tệ
    }

    // =========================================================================
    // 2. MODELS FOR INVOICE DETAIL PROCESSING
    // =========================================================================

    // Model để deserialize JSON response đầy đủ từ API chi tiết hóa đơn
    public class InvoiceDetailApiResponse
    {
        [JsonProperty("nbmst")]
        public string ApiMaSoThueNguoiBan { get; set; }
        [JsonProperty("khmshdon")]
        public string ApiMauSoHoaDon { get; set; }
        [JsonProperty("khhdon")]
        public string ApiKyHieuHoaDon { get; set; }
        [JsonProperty("shdon")]
        public string ApiSoHoaDon { get; set; }
        [JsonProperty("cqt")]
        public string Apicqt { get; set; }
        [JsonProperty("cttkhac")]
        public string ApiThongTinKhac { get; set; }
        [JsonProperty("dvtte")]
        public string ApiDonViTienTe { get; set; }
        [JsonProperty("hdon")]
        public string Apihdon { get; set; }
        [JsonProperty("hsgcma")]
        public string Apihsgcma { get; set; }
        [JsonProperty("hsgoc")]
        public string Apihsgoc { get; set; }
        [JsonProperty("hthdon")]
        public string Apihthdon { get; set; }
        [JsonProperty("htttoan")]
        public string ApiHinhThucThanhToanCode { get; set; }
        [JsonProperty("id")]
        public string Apiid { get; set; }
        [JsonProperty("idtbao")]
        public string Apiidtbao { get; set; }
        [JsonProperty("khdon")]
        public string Apikhdon { get; set; }
        [JsonProperty("khhdgoc")]
        public string Apikhhdgoc { get; set; }
        [JsonProperty("khmshdgoc")]
        public string Apikhmshdgoc { get; set; }
        [JsonProperty("lhdgoc")]
        public string Apilhdgoc { get; set; }
        [JsonProperty("mhdon")]
        public string ApiMaCQT { get; set; }
        [JsonProperty("mtdiep")]
        public string Apimtdiep { get; set; }
        [JsonProperty("mtdtchieu")]
        public string Apimtdtchieu { get; set; }
        [JsonProperty("nbdchi")]
        public string ApiDiaChiNguoiBan { get; set; }
        [JsonProperty("chma")]
        public string Apichma { get; set; }
        [JsonProperty("chten")]
        public string Apichten { get; set; }
        [JsonProperty("nbhdktngay")]
        public string Apinbhdktngay { get; set; }
        [JsonProperty("nbhdktso")]
        public string Apinbhdktso { get; set; }
        [JsonProperty("nbhdso")]
        public string Apinbhdso { get; set; }
        [JsonProperty("nblddnbo")]
        public string Apinblddnbo { get; set; }
        [JsonProperty("nbptvchuyen")]
        public string Apinbptvchuyen { get; set; }
        [JsonProperty("nbstkhoan")]
        public string Apinbstkhoan { get; set; }
        [JsonProperty("nbten")]
        public string ApiTenNguoiBan { get; set; }
        [JsonProperty("nbtnhang")]
        public string Apinbtnhang { get; set; }
        [JsonProperty("nbtnvchuyen")]
        public string Apinbtnvchuyen { get; set; }
        [JsonProperty("nbttkhac")]
        public string Apinbttkhac { get; set; }
        [JsonProperty("ncma")]
        public string Apincma { get; set; }
        [JsonProperty("ncnhat")]
        public string Apincnhat { get; set; }
        [JsonProperty("ngcnhat")]
        public string Apingcnhat { get; set; }
        [JsonProperty("nky")]
        public string ApiNgayKyISO { get; set; }
        [JsonProperty("nmdchi")]
        public string ApiDiaChiNguoiMua { get; set; }
        [JsonProperty("nmmst")]
        public string ApiMaSoThueNguoiMua { get; set; }
        [JsonProperty("nmstkhoan")]
        public string Apinmstkhoan { get; set; }
        [JsonProperty("nmten")]
        public string ApiTenNguoiMua { get; set; }
        [JsonProperty("nmtnhang")]
        public string Apinmtnhang { get; set; }
        [JsonProperty("nmtnmua")]
        public string Apinmtnmua { get; set; }
        [JsonProperty("nmttkhac")]
        public string Apinmttkhac { get; set; }
        [JsonProperty("ntao")]
        public string Apintao { get; set; }
        [JsonProperty("ntnhan")]
        public string Apintnhan { get; set; }
        [JsonProperty("pban")]
        public string Apipban { get; set; }
        [JsonProperty("ptgui")]
        public string Apiptgui { get; set; }
        [JsonProperty("shdgoc")]
        public string Apishdgoc { get; set; }
        [JsonProperty("tchat")]
        public string Apitchat { get; set; }
        [JsonProperty("tdlap")]
        public string ApiNgayLapHoaDonISO { get; set; }
        [JsonProperty("tgia")]
        public decimal? ApiTyGia { get; set; }
        [JsonProperty("tgtcthue")]
        public decimal? ApiTongTienChuaThue { get; set; }
        [JsonProperty("tgtthue")]
        public decimal? ApiTongTienThue { get; set; }
        [JsonProperty("tgtttbchu")]
        public string ApiTongTienThanhToanBangChu { get; set; }
        [JsonProperty("tgtttbso")]
        public decimal? ApiTongTienThanhToan { get; set; }
        [JsonProperty("thdon")]
        public string Apithdon { get; set; }
        [JsonProperty("thlap")]
        public string Apithlap { get; set; }
        [JsonProperty("thttlphi")]
        public string Apithttlphi { get; set; }
        [JsonProperty("thttltsuat")]
        public string Apithttltsuat { get; set; }
        [JsonProperty("tlhdon")]
        public string ApiLoaiHoaDon { get; set; }
        [JsonProperty("ttcktmai")]
        public decimal? ApiTongTienChietKhauTM { get; set; }
        [JsonProperty("tthai")]
        public string ApiTrangThaiHD_Code { get; set; }
        [JsonProperty("ttkhac")]
        public string Apittkhac { get; set; }
        [JsonProperty("tttbao")]
        public string Apitttbao { get; set; }
        [JsonProperty("ttttkhac")]
        public string Apittttkhac { get; set; }
        [JsonProperty("ttxly")]
        public string ApiTinhTrangXuLy_Code { get; set; }
        [JsonProperty("tvandnkntt")]
        public string Apitvandnkntt { get; set; }
        [JsonProperty("mhso")]
        public string Apimhso { get; set; }
        [JsonProperty("ladhddt")]
        public string Apiladhddt { get; set; }
        [JsonProperty("mkhang")]
        public string Apimkhang { get; set; }
        [JsonProperty("nbsdthoai")]
        public string Apinbsdthoai { get; set; }
        [JsonProperty("nbdctdtu")]
        public string Apinbdctdtu { get; set; }
        [JsonProperty("nbfax")]
        public string Apinbfax { get; set; }
        [JsonProperty("nbwebsite")]
        public string Apinbwebsite { get; set; }
        [JsonProperty("nbcks")]
        public string Apinbcks { get; set; }
        [JsonProperty("nmsdthoai")]
        public string Apinmsdthoai { get; set; }
        [JsonProperty("nmdctdtu")]
        public string Apinmdctdtu { get; set; }
        [JsonProperty("nmcmnd")]
        public string Apinmcmnd { get; set; }
        [JsonProperty("nmcks")]
        public string Apinmcks { get; set; }
        [JsonProperty("bhphap")]
        public string Apibhphap { get; set; }
        [JsonProperty("hddunlap")]
        public string Apihddunlap { get; set; }
        [JsonProperty("gchdgoc")]
        public string Apigchdgoc { get; set; }
        [JsonProperty("tbhgtngay")]
        public string Apitbhgtngay { get; set; }
        [JsonProperty("bhpldo")]
        public string Apibhpldo { get; set; }
        [JsonProperty("bhpcbo")]
        public string Apibhpcbo { get; set; }
        [JsonProperty("bhpngay")]
        public string Apibhpngay { get; set; }
        [JsonProperty("tdlhdgoc")]
        public string Apitdlhdgoc { get; set; }
        [JsonProperty("tgtphi")]
        public string Apitgtphi { get; set; }
        [JsonProperty("unhiem")]
        public string Apiunhiem { get; set; }
        [JsonProperty("mstdvnunlhdon")]
        public string Apimstdvnunlhdon { get; set; }
        [JsonProperty("tdvnunlhdon")]
        public string Apitdvnunlhdon { get; set; }
        [JsonProperty("nbmdvqhnsach")]
        public string Apinbmdvqhnsach { get; set; }
        [JsonProperty("nbsqdinh")]
        public string Apinbsqdinh { get; set; }
        [JsonProperty("nbncqdinh")]
        public string Apinbncqdinh { get; set; }
        [JsonProperty("nbcqcqdinh")]
        public string Apinbcqcqdinh { get; set; }
        [JsonProperty("nbhtban")]
        public string Apinbhtban { get; set; }
        [JsonProperty("nmmdvqhnsach")]
        public string Apinmmdvqhnsach { get; set; }
        [JsonProperty("nmddvchden")]
        public string Apinmddvchden { get; set; }
        [JsonProperty("nmtgvchdtu")]
        public string Apinmtgvchdtu { get; set; }
        [JsonProperty("nmtgvchdden")]
        public string Apinmtgvchdden { get; set; }
        [JsonProperty("nbtnban")]
        public string Apinbtnban { get; set; }
        [JsonProperty("dcdvnunlhdon")]
        public string Apidcdvnunlhdon { get; set; }
        [JsonProperty("dksbke")]
        public string Apidksbke { get; set; }
        [JsonProperty("dknlbke")]
        public string Apidknlbke { get; set; }
        [JsonProperty("thtttoan")]
        public string ApiHinhThucThanhToanText { get; set; }
        [JsonProperty("msttcgp")]
        public string Apimsttcgp { get; set; }
        [JsonProperty("cqtcks")]
        public string Apicqtcks { get; set; }
        [JsonProperty("gchu")]
        public string Apigchu { get; set; }
        [JsonProperty("kqcht")]
        public string Apikqcht { get; set; }
        [JsonProperty("hdntgia")]
        public string Apihdntgia { get; set; }
        [JsonProperty("tgtkcthue")]
        public string Apitgtkcthue { get; set; }
        [JsonProperty("tgtkhac")]
        public string Apitgtkhac { get; set; }
        [JsonProperty("nmshchieu")]
        public string Apinmshchieu { get; set; }
        [JsonProperty("nmnchchieu")]
        public string Apinmnchchieu { get; set; }
        [JsonProperty("nmnhhhchieu")]
        public string Apinmnhhhchieu { get; set; }
        [JsonProperty("nmqtich")]
        public string Apinmqtich { get; set; }
        [JsonProperty("ktkhthue")]
        public string Apiktkhthue { get; set; }
        [JsonProperty("qrcode")]
        public string Apiqrcode { get; set; }
        [JsonProperty("ttmstten")]
        public string Apittmstten { get; set; }
        [JsonProperty("ladhddtten")]
        public string Apiladhddtten { get; set; }
        [JsonProperty("hdxkhau")]
        public string Apihdxkhau { get; set; }
        [JsonProperty("hdxkptquan")]
        public string Apihdxkptquan { get; set; }
        [JsonProperty("hdgktkhthue")]
        public string Apihdgktkhthue { get; set; }
        [JsonProperty("hdonLquans")]
        public string ApihdonLquans { get; set; }
        [JsonProperty("tthdclquan")]
        public string Apitthdclquan { get; set; }
        [JsonProperty("pdndungs")]
        public string Apipdndungs { get; set; }
        [JsonProperty("hdtbssrses")]
        public string Apihdtbssrses { get; set; }
        [JsonProperty("hdTrung")]
        public string ApihdTrung { get; set; }
        [JsonProperty("isHDTrung")]
        public string ApiisHDTrung { get; set; }


        // Danh sách các dòng chi tiết hàng hóa/dịch vụ
        [JsonProperty("hdhhdvu")]
        public List<InvoiceItemRawData> Hdhhdvu { get; set; }

        public InvoiceDetailApiResponse() // Constructor
        {
            Hdhhdvu = new List<InvoiceItemRawData>();
        }
    }

    // Model cho một dòng chi tiết THÔ từ mảng "hdhhdvu" trong JSON
    public class InvoiceItemRawData
    {
        [JsonProperty("sxep")]
        public int? SoThuTuDongRaw { get; set; }
        [JsonProperty("ten")]
        public string TenHHDVRaw { get; set; }
        [JsonProperty("dvtinh")]
        public string DonViTinhRaw { get; set; }
        [JsonProperty("sluong")]
        public decimal? SoLuongRaw { get; set; }
        [JsonProperty("dgia")]
        public decimal? DonGiaRaw { get; set; }
        [JsonProperty("stckhau")]
        public decimal? SoTienChietKhauRaw { get; set; }
        [JsonProperty("ltsuat")]
        public string LoaiThueSuatRaw { get; set; }
        [JsonProperty("tsuat")]
        public decimal? ThueSuatValueRaw { get; set; } // Thuế suất dạng số (0.1, 0.08)
        [JsonProperty("thtien")]
        public decimal? ThanhTienChuaThueRaw { get; set; }
        [JsonProperty("tthue")]
        public decimal? TienThueRaw { get; set; }

        // Nếu API có trả về mã hàng hóa trong item, thêm vào đây
        // [JsonProperty("maHHDV")] // Tên trường JSON thực tế nếu có
        // public string MaHHDVRaw { get; set; } 
    }

    // Model cho DataGrid dgChiTiet (Lớp phẳng để hiển thị, lặp lại thông tin chung)
    public class InvoiceDisplayItem
    {
        // Thông tin chung của hóa đơn (lặp lại cho mỗi dòng)
        public string LoaiHoaDon { get; set; }
        public string MauSoHoaDon { get; set; }
        public string KyHieuHoaDon { get; set; }
        public string SoHoaDon { get; set; }
        public string NgayLapHoaDon { get; set; } // Đã định dạng
        public string NgayKy { get; set; } // Đã định dạng
        public string MaCQT { get; set; }
        public string DonViTienTe { get; set; }
        public decimal? TyGia { get; set; }
        public string TenNguoiBan { get; set; }
        public string MaSoThueNguoiBan { get; set; }
        public string DiaChiNguoiBan { get; set; }
        public string TenNguoiMua { get; set; }
        public string MaSoThueNguoiMua { get; set; }
        public string DiaChiNguoiMua { get; set; }
        public string HinhThucThanhToan { get; set; }

        // Thông tin của dòng chi tiết
        public int? SoThuTuDong { get; set; }
        public string MaHHDV { get; set; } = ""; // Mặc định trống, vì API không trả về rõ ràng
        public string TenHHDV { get; set; }
        public string DonViTinh { get; set; }
        public decimal? SoLuong { get; set; }
        public decimal? DonGia { get; set; }
        public decimal? SoTienChietKhau { get; set; }
        public string LoaiThueSuat { get; set; }
        public decimal? ThanhTienChuaThue { get; set; }
        public decimal? TienThue { get; set; }

        // Thông tin tổng của hóa đơn (lặp lại cho mỗi dòng)
        public decimal? TongTienChuaThue_HD { get; set; }
        public decimal? TongTienThue_HD { get; set; }
        public decimal? TongTienChietKhauTM_HD { get; set; }
        public decimal? TongTienThanhToan_HD { get; set; }
        public string TongTienThanhToanBangChu_HD { get; set; }
        public string TrangThai_HD { get; set; } // Đã diễn giải
        public string TinhTrangXuLy_HD { get; set; } // Đã diễn giải

        // Thuộc tính đã định dạng (có thể dùng Converter trong XAML thay thế)
        public string DonGiaFormatted => FormatHelper.FormatCurrency(DonGia, 2); // Ví dụ 2 chữ số thập phân
        public string ThanhTienChuaThueFormatted => FormatHelper.FormatCurrency(ThanhTienChuaThue);
        public string TienThueFormatted => FormatHelper.FormatCurrency(TienThue);
        public string SoLuongFormatted => SoLuong.HasValue ? SoLuong.Value.ToString("N2", CultureInfo.GetCultureInfo("vi-VN")) : string.Empty; // Ví dụ định dạng số lượng

        public string TongTienChuaThue_HD_Formatted => FormatHelper.FormatCurrency(TongTienChuaThue_HD);
        public string TongTienThue_HD_Formatted => FormatHelper.FormatCurrency(TongTienThue_HD);
        public string TongTienThanhToan_HD_Formatted => FormatHelper.FormatCurrency(TongTienThanhToan_HD);

    }

    // =========================================================================
    // 3. INVOICE IDENTIFIER - Giữ nguyên
    // =========================================================================
    public class InvoiceIdentifier
    {
        public string Nbmst { get; set; }
        public string Khhdon { get; set; }
        public string Shdon { get; set; }
        public string Khmshdon { get; set; }
        public bool IsScoQuery { get; set; }
        public string Tdlap { get; set; }
    }

    // =========================================================================
    // 4. FORMAT HELPER CLASS
    // =========================================================================
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
// --- END OF FILE InvoiceModels.cs ---