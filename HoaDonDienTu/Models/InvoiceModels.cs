﻿// --- START OF FILE InvoiceModels.cs ---
using System;
using System.Collections.Generic;
using System.Globalization; // Thêm cho CultureInfo
using System.Text.Json.Serialization;
using HoaDonDienTu.Helper;

namespace HoaDonDienTu.Models
{
    // =========================================================================
    // 1. MODELS FOR INVOICE LIST (SUMMARY) - Giữ nguyên từ code của bạn
    // =========================================================================
    public class InvoiceQueryResult
    {
        [JsonPropertyName("datas")]
        public List<InvoiceSummaryData> Datas { get; set; }

        [JsonPropertyName("state")]
        public string State { get; set; }

        public InvoiceQueryResult() // Constructor
        {
            Datas = new List<InvoiceSummaryData>();
        }
    }

    public class InvoiceSummaryData // Dữ liệu thô từ API danh sách hóa đơn
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("nbmst")]
        public string nbmst { get; set; }

        [JsonPropertyName("khmshdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string khmshdon { get; set; }

        [JsonPropertyName("khhdon")]
        public string khhdon { get; set; }

        [JsonPropertyName("shdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string shdon { get; set; }

        [JsonPropertyName("cqt")]
        public string cqt { get; set; }

        [JsonPropertyName("cttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string cttkhac { get; set; }

        [JsonPropertyName("dvtte")]
        public string dvtte { get; set; }

        [JsonPropertyName("hdon")]
        public string hdon { get; set; }

        [JsonPropertyName("hsgcma")]
        public string hsgcma { get; set; }

        [JsonPropertyName("hsgoc")]
        public string hsgoc { get; set; }

        [JsonPropertyName("hthdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string hthdon { get; set; }

        [JsonPropertyName("htttoan")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string htttoan { get; set; }

        [JsonPropertyName("idtbao")]
        public string idtbao { get; set; }

        [JsonPropertyName("khdon")]
        public string khdon { get; set; }

        [JsonPropertyName("khhdgoc")]
        public string khhdgoc { get; set; }

        [JsonPropertyName("khmshdgoc")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string khmshdgoc { get; set; }

        [JsonPropertyName("lhdgoc")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string lhdgoc { get; set; }

        [JsonPropertyName("mhdon")]
        public string mhdon { get; set; }

        [JsonPropertyName("mtdiep")]
        public string mtdiep { get; set; }

        [JsonPropertyName("mtdtchieu")]
        public string mtdtchieu { get; set; }

        [JsonPropertyName("nbdchi")]
        public string nbdchi { get; set; }

        [JsonPropertyName("chma")]
        public string chma { get; set; }

        [JsonPropertyName("chten")]
        public string chten { get; set; }

        [JsonPropertyName("nbhdktngay")]
        public string nbhdktngay { get; set; }

        [JsonPropertyName("nbhdktso")]
        public string nbhdktso { get; set; }

        [JsonPropertyName("nbhdso")]
        public string nbhdso { get; set; }

        [JsonPropertyName("nblddnbo")]
        public string nblddnbo { get; set; }

        [JsonPropertyName("nbptvchuyen")]
        public string nbptvchuyen { get; set; }

        [JsonPropertyName("nbstkhoan")]
        public string nbstkhoan { get; set; }

        [JsonPropertyName("nbten")]
        public string nbten { get; set; }

        [JsonPropertyName("nbtnhang")]
        public string nbtnhang { get; set; }

        [JsonPropertyName("nbtnvchuyen")]
        public string nbtnvchuyen { get; set; }

        [JsonPropertyName("nbttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nbttkhac { get; set; }

        [JsonPropertyName("ncma")]
        public string ncma { get; set; }

        [JsonPropertyName("ncnhat")]
        public string ncnhat { get; set; }

        [JsonPropertyName("ngcnhat")]
        public string ngcnhat { get; set; }

        [JsonPropertyName("nky")]
        public string nky { get; set; }

        [JsonPropertyName("nmdchi")]
        public string nmdchi { get; set; }

        [JsonPropertyName("nmmst")]
        public string nmmst { get; set; }

        [JsonPropertyName("nmstkhoan")]
        public string nmstkhoan { get; set; }

        [JsonPropertyName("nmten")]
        public string nmten { get; set; }

        [JsonPropertyName("nmtnhang")]
        public string nmtnhang { get; set; }

        [JsonPropertyName("nmtnmua")]
        public string nmtnmua { get; set; }

        [JsonPropertyName("nmttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nmttkhac { get; set; }

        [JsonPropertyName("ntao")]
        public string ntao { get; set; }

        [JsonPropertyName("ntnhan")]
        public string ntnhan { get; set; }

        [JsonPropertyName("pban")]
        public string pban { get; set; }

        [JsonPropertyName("ptgui")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string ptgui { get; set; }

        [JsonPropertyName("shdgoc")]
        public string shdgoc { get; set; }

        [JsonPropertyName("tchat")]
        public object tchat { get; set; }

        [JsonPropertyName("tdlap")]
        public string tdlap { get; set; }

        [JsonPropertyName("tgia")]
        public decimal? tgia { get; set; }

        [JsonPropertyName("tgtcthue")]
        public decimal? tgtcthue { get; set; }

        [JsonPropertyName("tgtthue")]
        public decimal? tgtthue { get; set; }

        [JsonPropertyName("tgtttbchu")]
        public string tgtttbchu { get; set; }

        [JsonPropertyName("tgtttbso")]
        public decimal? tgtttbso { get; set; }

        [JsonPropertyName("thdon")]
        public string thdon { get; set; }

        [JsonPropertyName("thlap")]
        public object thlap { get; set; }

        [JsonPropertyName("thttlphi")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttlphi { get; set; }

        [JsonPropertyName("thttltsuat")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttltsuat { get; set; }

        [JsonPropertyName("tlhdon")]
        public string tlhdon { get; set; }

        [JsonPropertyName("ttcktmai")]
        public decimal? ttcktmai { get; set; }

        [JsonPropertyName("tthai")]
        public object tthai { get; set; }

        [JsonPropertyName("ttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttkhac { get; set; }

        [JsonPropertyName("tttbao")]
        public object tttbao { get; set; }

        [JsonPropertyName("ttttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttttkhac { get; set; }

        [JsonPropertyName("ttxly")]
        public object ttxly { get; set; }

        [JsonPropertyName("tvandnkntt")]
        public string tvandnkntt { get; set; }

        [JsonPropertyName("mhso")]
        public string mhso { get; set; }

        [JsonPropertyName("ladhddt")]
        public object ladhddt { get; set; }

        [JsonPropertyName("mkhang")]
        public string mkhang { get; set; }

        [JsonPropertyName("nbsdthoai")]
        public string nbsdthoai { get; set; }

        [JsonPropertyName("nbdctdtu")]
        public string nbdctdtu { get; set; }

        [JsonPropertyName("nbfax")]
        public string nbfax { get; set; }

        [JsonPropertyName("nbwebsite")]
        public string nbwebsite { get; set; }

        [JsonPropertyName("nbcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nbcks { get; set; }

        [JsonPropertyName("nmsdthoai")]
        public string nmsdthoai { get; set; }

        [JsonPropertyName("nmdctdtu")]
        public string nmdctdtu { get; set; }

        [JsonPropertyName("nmcmnd")]
        public string nmcmnd { get; set; }

        [JsonPropertyName("nmcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nmcks { get; set; }

        [JsonPropertyName("bhphap")]
        public object bhphap { get; set; }

        [JsonPropertyName("hddunlap")]
        public string hddunlap { get; set; }

        [JsonPropertyName("gchdgoc")]
        public string gchdgoc { get; set; }

        [JsonPropertyName("tbhgtngay")]
        public string tbhgtngay { get; set; }

        [JsonPropertyName("bhpldo")]
        public string bhpldo { get; set; }

        [JsonPropertyName("bhpcbo")]
        public string bhpcbo { get; set; }

        [JsonPropertyName("bhpngay")]
        public string bhpngay { get; set; }

        [JsonPropertyName("tdlhdgoc")]
        public string tdlhdgoc { get; set; }

        [JsonPropertyName("tgtphi")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtphi { get; set; }

        [JsonPropertyName("unhiem")]
        public string unhiem { get; set; }

        [JsonPropertyName("mstdvnunlhdon")]
        public string mstdvnunlhdon { get; set; }

        [JsonPropertyName("tdvnunlhdon")]
        public string tdvnunlhdon { get; set; }

        [JsonPropertyName("nbmdvqhnsach")]
        public string nbmdvqhnsach { get; set; }

        [JsonPropertyName("nbsqdinh")]
        public string nbsqdinh { get; set; }

        [JsonPropertyName("nbncqdinh")]
        public string nbncqdinh { get; set; }

        [JsonPropertyName("nbcqcqdinh")]
        public string nbcqcqdinh { get; set; }

        [JsonPropertyName("nbhtban")]
        public string nbhtban { get; set; }

        [JsonPropertyName("nmmdvqhnsach")]
        public string nmmdvqhnsach { get; set; }

        [JsonPropertyName("nmddvchden")]
        public string nmddvchden { get; set; }

        [JsonPropertyName("nmtgvchdtu")]
        public string nmtgvchdtu { get; set; }

        [JsonPropertyName("nmtgvchdden")]
        public string nmtgvchdden { get; set; }

        [JsonPropertyName("nbtnban")]
        public string nbtnban { get; set; }

        [JsonPropertyName("dcdvnunlhdon")]
        public string dcdvnunlhdon { get; set; }

        [JsonPropertyName("dksbke")]
        public string dksbke { get; set; }

        [JsonPropertyName("dknlbke")]
        public string dknlbke { get; set; }

        [JsonPropertyName("thtttoan")]
        public string thtttoan { get; set; }

        [JsonPropertyName("msttcgp")]
        public string msttcgp { get; set; }

        [JsonPropertyName("cqtcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string cqtcks { get; set; }

        [JsonPropertyName("gchu")]
        public string gchu { get; set; }

        [JsonPropertyName("kqcht")]
        public string kqcht { get; set; }

        [JsonPropertyName("hdntgia")]
        public string hdntgia { get; set; }

        [JsonPropertyName("tgtkcthue")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtkcthue { get; set; }

        [JsonPropertyName("tgtkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtkhac { get; set; }

        [JsonPropertyName("nmshchieu")]
        public string nmshchieu { get; set; }

        [JsonPropertyName("nmnchchieu")]
        public string nmnchchieu { get; set; }

        [JsonPropertyName("nmnhhhchieu")]
        public string nmnhhhchieu { get; set; }

        [JsonPropertyName("nmqtich")]
        public string nmqtich { get; set; }

        [JsonPropertyName("ktkhthue")]
        public string ktkhthue { get; set; }

        [JsonPropertyName("qrcode")]
        public string qrcode { get; set; }

        [JsonPropertyName("ttmstten")]
        public string ttmstten { get; set; }

        [JsonPropertyName("ladhddtten")]
        public string ladhddtten { get; set; }

        [JsonPropertyName("hdxkhau")]
        public string hdxkhau { get; set; }

        [JsonPropertyName("hdxkptquan")]
        public string hdxkptquan { get; set; }

        [JsonPropertyName("hdgktkhthue")]
        public string hdgktkhthue { get; set; }

        [JsonPropertyName("hdonLquans")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdonLquans { get; set; }

        [JsonPropertyName("tthdclquan")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BoolToStringConverter))]
        public string tthdclquan { get; set; }

        [JsonPropertyName("pdndungs")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string pdndungs { get; set; }

        [JsonPropertyName("hdtbssrses")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdtbssrses { get; set; }

        [JsonPropertyName("hdTrung")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdTrung { get; set; }

        [JsonPropertyName("isHDTrung")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string isHDTrung { get; set; }

        // === Phần phục vụ cho việc lưu vào DataBase ===

        public string LastDownloadedDetailDate { get; set; } // Ngày tải chi tiết hóa đơn lần cuối, định dạng "dd/MM/yyyy"

    }

    public class InvoiceSummary // Model cho DataGrid dgTongHop
    {
        public int STT { get; set; }
        public string id { get; set; } // ID của hóa đơn
        public string TenHoaDon { get; set; } // Tên hóa đơn, ví dụ: "Hóa đơn GTGT", "Hóa đơn bán hàng"
        public string MauHoaDon { get; set; } // Mã mẫu hóa đơn
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public string NgayHD { get; set; } // Đã định dạng dd/MM/yyyy
        public string DVTienTe { get; set; } // Đã định dạng tiền tệ
        public decimal? TyGia { get; set; } // Giá trị tỷ giá, có thể null nếu không có
        public string TenNguoiBan { get; set; }
        public string MSTNguoiBan { get; set; }
        public string DiaChiNguoiBan { get; set; } // Địa chỉ người bán
        public string TenNguoiMua { get; set; }
        public string MSTNguoiMua { get; set; } // Mã số thuế người mua
        public string DiaChiNguoiMua { get; set; } // Địa chỉ người mua
        public decimal? TongTienChuaThue { get; set; } // Tổng tiền chưa thuế, có thể null
        public decimal? TongTienThue { get; set; } // Tổng tiền thuế, có thể null
        public string TrangThai { get; set; } // Đã diễn giải
        public string KetQuaKiemTraHoaDon { get; set; } // Kết quả kiểm tra hóa đơn (đã diễn giải)

    }

    // =========================================================================
    // 2. MODELS FOR INVOICE DETAIL PROCESSING
    // =========================================================================

    // Model để deserialize JSON response đầy đủ từ API chi tiết hóa đơn

    public class InvoiceDetailData // Dữ liệu thô từ API danh sách hóa đơn
    {
        [JsonPropertyName("id")]
        public string id { get; set; }

        [JsonPropertyName("nbmst")]
        public string nbmst { get; set; }

        [JsonPropertyName("khmshdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string khmshdon { get; set; }

        [JsonPropertyName("khhdon")]
        public string khhdon { get; set; }

        [JsonPropertyName("shdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string shdon { get; set; }

        [JsonPropertyName("cqt")]
        public string cqt { get; set; }

        [JsonPropertyName("cttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string cttkhac { get; set; }

        [JsonPropertyName("dvtte")]
        public string dvtte { get; set; }

        [JsonPropertyName("hdon")]
        public string hdon { get; set; }

        [JsonPropertyName("hsgcma")]
        public string hsgcma { get; set; }

        [JsonPropertyName("hsgoc")]
        public string hsgoc { get; set; }

        [JsonPropertyName("hthdon")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string hthdon { get; set; }

        [JsonPropertyName("htttoan")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string htttoan { get; set; }

        [JsonPropertyName("idtbao")]
        public string idtbao { get; set; }

        [JsonPropertyName("khdon")]
        public string khdon { get; set; }

        [JsonPropertyName("khhdgoc")]
        public string khhdgoc { get; set; }

        [JsonPropertyName("khmshdgoc")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string khmshdgoc { get; set; }

        [JsonPropertyName("lhdgoc")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string lhdgoc { get; set; }

        [JsonPropertyName("mhdon")]
        public string mhdon { get; set; }

        [JsonPropertyName("mtdiep")]
        public string mtdiep { get; set; }

        [JsonPropertyName("mtdtchieu")]
        public string mtdtchieu { get; set; }

        [JsonPropertyName("nbdchi")]
        public string nbdchi { get; set; }

        [JsonPropertyName("chma")]
        public string chma { get; set; }

        [JsonPropertyName("chten")]
        public string chten { get; set; }

        [JsonPropertyName("nbhdktngay")]
        public string nbhdktngay { get; set; }

        [JsonPropertyName("nbhdktso")]
        public string nbhdktso { get; set; }

        [JsonPropertyName("nbhdso")]
        public string nbhdso { get; set; }

        [JsonPropertyName("nblddnbo")]
        public string nblddnbo { get; set; }

        [JsonPropertyName("nbptvchuyen")]
        public string nbptvchuyen { get; set; }

        [JsonPropertyName("nbstkhoan")]
        public string nbstkhoan { get; set; }

        [JsonPropertyName("nbten")]
        public string nbten { get; set; }

        [JsonPropertyName("nbtnhang")]
        public string nbtnhang { get; set; }

        [JsonPropertyName("nbtnvchuyen")]
        public string nbtnvchuyen { get; set; }

        [JsonPropertyName("nbttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nbttkhac { get; set; }

        [JsonPropertyName("ncma")]
        public string ncma { get; set; }

        [JsonPropertyName("ncnhat")]
        public string ncnhat { get; set; }

        [JsonPropertyName("ngcnhat")]
        public string ngcnhat { get; set; }

        [JsonPropertyName("nky")]
        public string nky { get; set; }

        [JsonPropertyName("nmdchi")]
        public string nmdchi { get; set; }

        [JsonPropertyName("nmmst")]
        public string nmmst { get; set; }

        [JsonPropertyName("nmstkhoan")]
        public string nmstkhoan { get; set; }

        [JsonPropertyName("nmten")]
        public string nmten { get; set; }

        [JsonPropertyName("nmtnhang")]
        public string nmtnhang { get; set; }

        [JsonPropertyName("nmtnmua")]
        public string nmtnmua { get; set; }

        [JsonPropertyName("nmttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nmttkhac { get; set; }

        [JsonPropertyName("ntao")]
        public string ntao { get; set; }

        [JsonPropertyName("ntnhan")]
        public string ntnhan { get; set; }

        [JsonPropertyName("pban")]
        public string pban { get; set; }

        [JsonPropertyName("ptgui")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string ptgui { get; set; }

        [JsonPropertyName("shdgoc")]
        public string shdgoc { get; set; }

        [JsonPropertyName("tchat")]
        public object tchat { get; set; }

        [JsonPropertyName("tdlap")]
        public string tdlap { get; set; }

        [JsonPropertyName("tgia")]
        public decimal? tgia { get; set; }

        [JsonPropertyName("tgtcthue")]
        public decimal? tgtcthue { get; set; }

        [JsonPropertyName("tgtthue")]
        public decimal? tgtthue { get; set; }

        [JsonPropertyName("tgtttbchu")]
        public string tgtttbchu { get; set; }

        [JsonPropertyName("tgtttbso")]
        public decimal? tgtttbso { get; set; }

        [JsonPropertyName("thdon")]
        public string thdon { get; set; }

        [JsonPropertyName("thlap")]
        public object thlap { get; set; }

        [JsonPropertyName("thttlphi")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttlphi { get; set; }

        [JsonPropertyName("thttltsuat")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttltsuat { get; set; }

        [JsonPropertyName("tlhdon")]
        public string tlhdon { get; set; }

        [JsonPropertyName("ttcktmai")]
        public decimal? ttcktmai { get; set; }

        [JsonPropertyName("tthai")]
        public object tthai { get; set; }

        [JsonPropertyName("ttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttkhac { get; set; }

        [JsonPropertyName("tttbao")]
        public object tttbao { get; set; }

        [JsonPropertyName("ttttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttttkhac { get; set; }

        [JsonPropertyName("ttxly")]
        public object ttxly { get; set; }

        [JsonPropertyName("tvandnkntt")]
        public string tvandnkntt { get; set; }

        [JsonPropertyName("mhso")]
        public string mhso { get; set; }

        [JsonPropertyName("ladhddt")]
        public object ladhddt { get; set; }

        [JsonPropertyName("mkhang")]
        public string mkhang { get; set; }

        [JsonPropertyName("nbsdthoai")]
        public string nbsdthoai { get; set; }

        [JsonPropertyName("nbdctdtu")]
        public string nbdctdtu { get; set; }

        [JsonPropertyName("nbfax")]
        public string nbfax { get; set; }

        [JsonPropertyName("nbwebsite")]
        public string nbwebsite { get; set; }

        [JsonPropertyName("nbcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nbcks { get; set; }

        [JsonPropertyName("nmsdthoai")]
        public string nmsdthoai { get; set; }

        [JsonPropertyName("nmdctdtu")]
        public string nmdctdtu { get; set; }

        [JsonPropertyName("nmcmnd")]
        public string nmcmnd { get; set; }

        [JsonPropertyName("nmcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string nmcks { get; set; }

        [JsonPropertyName("bhphap")]
        public object bhphap { get; set; }

        [JsonPropertyName("hddunlap")]
        public string hddunlap { get; set; }

        [JsonPropertyName("gchdgoc")]
        public string gchdgoc { get; set; }

        [JsonPropertyName("tbhgtngay")]
        public string tbhgtngay { get; set; }

        [JsonPropertyName("bhpldo")]
        public string bhpldo { get; set; }

        [JsonPropertyName("bhpcbo")]
        public string bhpcbo { get; set; }

        [JsonPropertyName("bhpngay")]
        public string bhpngay { get; set; }

        [JsonPropertyName("tdlhdgoc")]
        public string tdlhdgoc { get; set; }

        [JsonPropertyName("tgtphi")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtphi { get; set; }

        [JsonPropertyName("unhiem")]
        public string unhiem { get; set; }

        [JsonPropertyName("mstdvnunlhdon")]
        public string mstdvnunlhdon { get; set; }

        [JsonPropertyName("tdvnunlhdon")]
        public string tdvnunlhdon { get; set; }

        [JsonPropertyName("nbmdvqhnsach")]
        public string nbmdvqhnsach { get; set; }

        [JsonPropertyName("nbsqdinh")]
        public string nbsqdinh { get; set; }

        [JsonPropertyName("nbncqdinh")]
        public string nbncqdinh { get; set; }

        [JsonPropertyName("nbcqcqdinh")]
        public string nbcqcqdinh { get; set; }

        [JsonPropertyName("nbhtban")]
        public string nbhtban { get; set; }

        [JsonPropertyName("nmmdvqhnsach")]
        public string nmmdvqhnsach { get; set; }

        [JsonPropertyName("nmddvchden")]
        public string nmddvchden { get; set; }

        [JsonPropertyName("nmtgvchdtu")]
        public string nmtgvchdtu { get; set; }

        [JsonPropertyName("nmtgvchdden")]
        public string nmtgvchdden { get; set; }

        [JsonPropertyName("nbtnban")]
        public string nbtnban { get; set; }

        [JsonPropertyName("dcdvnunlhdon")]
        public string dcdvnunlhdon { get; set; }

        [JsonPropertyName("dksbke")]
        public string dksbke { get; set; }

        [JsonPropertyName("dknlbke")]
        public string dknlbke { get; set; }

        [JsonPropertyName("thtttoan")]
        public string thtttoan { get; set; }

        [JsonPropertyName("msttcgp")]
        public string msttcgp { get; set; }

        [JsonPropertyName("cqtcks")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string cqtcks { get; set; }

        [JsonPropertyName("gchu")]
        public string gchu { get; set; }

        [JsonPropertyName("kqcht")]
        public string kqcht { get; set; }

        [JsonPropertyName("hdntgia")]
        public string hdntgia { get; set; }

        [JsonPropertyName("tgtkcthue")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtkcthue { get; set; }

        [JsonPropertyName("tgtkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string tgtkhac { get; set; }

        [JsonPropertyName("nmshchieu")]
        public string nmshchieu { get; set; }

        [JsonPropertyName("nmnchchieu")]
        public string nmnchchieu { get; set; }

        [JsonPropertyName("nmnhhhchieu")]
        public string nmnhhhchieu { get; set; }

        [JsonPropertyName("nmqtich")]
        public string nmqtich { get; set; }

        [JsonPropertyName("ktkhthue")]
        public string ktkhthue { get; set; }

        [JsonPropertyName("qrcode")]
        public string qrcode { get; set; }

        [JsonPropertyName("ttmstten")]
        public string ttmstten { get; set; }

        [JsonPropertyName("ladhddtten")]
        public string ladhddtten { get; set; }

        [JsonPropertyName("hdxkhau")]
        public string hdxkhau { get; set; }

        [JsonPropertyName("hdxkptquan")]
        public string hdxkptquan { get; set; }

        [JsonPropertyName("hdgktkhthue")]
        public string hdgktkhthue { get; set; }

        [JsonPropertyName("hdonLquans")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdonLquans { get; set; }

        [JsonPropertyName("tthdclquan")]
        [System.Text.Json.Serialization.JsonConverter(typeof(BoolToStringConverter))]
        public string tthdclquan { get; set; }

        [JsonPropertyName("pdndungs")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string pdndungs { get; set; }

        [JsonPropertyName("hdtbssrses")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdtbssrses { get; set; }

        [JsonPropertyName("hdTrung")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string hdTrung { get; set; }

        [JsonPropertyName("isHDTrung")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleStringConverter))]
        public string isHDTrung { get; set; }

        // Danh sách các dòng chi tiết hàng hóa/dịch vụ
        [JsonPropertyName("hdhhdvu")]
        public List<HangHoaDichVuRawData> hdhhdvu { get; set; }

        public InvoiceDetailData() // Constructor
        {
            hdhhdvu = new List<HangHoaDichVuRawData>();
        }

        public string LastDownloadedDetailDate { get; set; } // Ngày tải chi tiết hóa đơn lần cuối, định dạng "dd/MM/yyyy"

    }

     // Model cho một dòng chi tiết THÔ từ mảng "hdhhdvu" trong JSON
    public class HangHoaDichVuRawData
    {
        [JsonPropertyName("idhdon")]
        public string idhdon { get; set; }
        [JsonPropertyName("id")]
        public string id { get; set; }
        [JsonPropertyName("dgia")]
        public decimal? dgia { get; set; }
        [JsonPropertyName("dvtinh")]
        public string dvtinh { get; set; }
        [JsonPropertyName("ltsuat")]
        public string ltsuat { get; set; }
        [JsonPropertyName("sluong")]
        public decimal? sluong { get; set; }
        [JsonPropertyName("stbchu")]
        public string stbchu { get; set; }
        [JsonPropertyName("stckhau")]
        public decimal? stckhau { get; set; }
        [JsonPropertyName("stt")]
        public int? stt { get; set; }
        [JsonPropertyName("sxep")]
        public int? sxep { get; set; }
        [JsonPropertyName("tchat")]
        public int? tchat { get; set; }
        [JsonPropertyName("ten")]
        public string ten { get; set; }
        [JsonPropertyName("thtcthue")]
        public decimal? thtcthue { get; set; }
        [JsonPropertyName("thtien")]
        public decimal? thtien { get; set; }
        [JsonPropertyName("tlckhau")]
        public decimal? tlckhau { get; set; }
        [JsonPropertyName("tsuat")]
        public decimal? tsuat { get; set; }
        [JsonPropertyName("tthue")]
        public decimal? tthue { get; set; }
        [JsonPropertyName("ttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttkhac { get; set; }
        [JsonPropertyName("dvtte")]
        public string dvtte { get; set; }
        [JsonPropertyName("tgia")]
        public decimal? tgia { get; set; }
        [JsonPropertyName("tthhdtrung")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string tthhdtrung { get; set; }


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
        public string DonGiaFormatted => Helper.FormatHelper.FormatCurrency(DonGia, 2); // Ví dụ 2 chữ số thập phân
        public string ThanhTienChuaThueFormatted => Helper.FormatHelper.FormatCurrency(ThanhTienChuaThue);
        public string TienThueFormatted => Helper.FormatHelper.FormatCurrency(TienThue);
        public string SoLuongFormatted => SoLuong.HasValue ? SoLuong.Value.ToString("N2", CultureInfo.GetCultureInfo("vi-VN")) : string.Empty; // Ví dụ định dạng số lượng

        public string TongTienChuaThue_HD_Formatted => Helper.FormatHelper.FormatCurrency(TongTienChuaThue_HD);
        public string TongTienThue_HD_Formatted => Helper.FormatHelper.FormatCurrency(TongTienThue_HD);
        public string TongTienThanhToan_HD_Formatted => Helper.FormatHelper.FormatCurrency(TongTienThanhToan_HD);

    }

    // =========================================================================
    // 3. INVOICE IDENTIFIER - Giữ nguyên
    // =========================================================================
    public class InvoiceIdentifier
    {
        public string ID { get; set; } // ID của hóa đơn
        public string Nbmst { get; set; }
        public string Khhdon { get; set; }
        public string Shdon { get; set; }
        public string Khmshdon { get; set; }
        public bool IsScoQuery { get; set; }
        public string Tdlap { get; set; }
    }
}

// --- END OF FILE InvoiceModels.cs ---