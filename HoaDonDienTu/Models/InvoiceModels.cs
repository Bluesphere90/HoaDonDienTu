// --- START OF FILE InvoiceModels.cs ---
using System;
using System.Collections.Generic;
using System.Globalization; // Thêm cho CultureInfo
using System.Text.Json.Serialization;
using HoaDonDienTu.Helper;
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
        public string ptgui { get; set; }

        [JsonPropertyName("shdgoc")]
        public string shdgoc { get; set; }

        [JsonPropertyName("tchat")]
        public string tchat { get; set; }

        [JsonPropertyName("tdlap")]
        public string tdlap { get; set; }

        [JsonPropertyName("tgia")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal? tgia { get; set; }

        [JsonPropertyName("tgtcthue")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal? tgtcthue { get; set; }

        [JsonPropertyName("tgtthue")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal? tgtthue { get; set; }

        [JsonPropertyName("tgtttbchu")]
        public string tgtttbchu { get; set; }

        [JsonPropertyName("tgtttbso")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal? tgtttbso { get; set; }

        [JsonPropertyName("thdon")]
        public string thdon { get; set; }

        [JsonPropertyName("thlap")]
        public string thlap { get; set; }

        [JsonPropertyName("thttlphi")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttlphi { get; set; }

        [JsonPropertyName("thttltsuat")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string thttltsuat { get; set; }

        [JsonPropertyName("tlhdon")]
        public string tlhdon { get; set; }

        [JsonPropertyName("ttcktmai")]
        [System.Text.Json.Serialization.JsonConverter(typeof(FlexibleDecimalConverter))]
        public decimal? ttcktmai { get; set; }

        [JsonPropertyName("tthai")]
        public string tthai { get; set; }

        [JsonPropertyName("ttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttkhac { get; set; }

        [JsonPropertyName("tttbao")]
        public string tttbao { get; set; }

        [JsonPropertyName("ttttkhac")]
        [System.Text.Json.Serialization.JsonConverter(typeof(JsonStringConverter))]
        public string ttttkhac { get; set; }

        [JsonPropertyName("ttxly")]
        public string ttxly { get; set; }

        [JsonPropertyName("tvandnkntt")]
        public string tvandnkntt { get; set; }

        [JsonPropertyName("mhso")]
        public string mhso { get; set; }

        [JsonPropertyName("ladhddt")]
        public string ladhddt { get; set; }

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
        public string bhphap { get; set; }

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

    }

    public class InvoiceSummary // Model cho DataGrid dgTongHop
    {
        public int STT { get; set; }
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
        public string Nbmst { get; set; }
        public string Khhdon { get; set; }
        public string Shdon { get; set; }
        public string Khmshdon { get; set; }
        public bool IsScoQuery { get; set; }
        public string Tdlap { get; set; }
    }
}

// --- END OF FILE InvoiceModels.cs ---