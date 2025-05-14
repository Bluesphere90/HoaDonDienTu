using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HoaDonDienTu.Models
{
    // Classes mô hình dữ liệu cho hóa đơn
    public class InvoiceQueryResult
    {
        [JsonProperty("datas")]
        public List<InvoiceSummaryData> Datas { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }

    public class InvoiceSummaryData
    {
        [JsonProperty("nbmst")]
        public string Nbmst { get; set; }

        [JsonProperty("nban")]
        public string Nban { get; set; }

        [JsonProperty("khhdon")]
        public string Khhdon { get; set; }

        [JsonProperty("shdon")]
        public string Shdon { get; set; }

        [JsonProperty("khmshdon")]
        public string Khmshdon { get; set; }

        [JsonProperty("tdlap")]
        public string Tdlap { get; set; }

        [JsonProperty("tgtttbso")]
        public string Tgtttbso { get; set; }

        [JsonProperty("tthai")]
        public string Tthai { get; set; }
    }

    public class InvoiceDetailResult
    {
        [JsonProperty("hdhhdvu")]
        public List<InvoiceItemData> Hdhhdvu { get; set; }
    }

    public class InvoiceItemData
    {
        [JsonProperty("thhdv")]
        public string Thhdv { get; set; }

        [JsonProperty("dvt")]
        public string Dvt { get; set; }

        [JsonProperty("sluong")]
        public string Sluong { get; set; }

        [JsonProperty("dgia")]
        public string Dgia { get; set; }

        [JsonProperty("thtien")]
        public string Thtien { get; set; }
    }

    public class InvoiceIdentifier
    {
        public string Nbmst { get; set; }
        public string Khhdon { get; set; }
        public string Shdon { get; set; }
        public string Khmshdon { get; set; }
        public bool IsScoQuery { get; set; }
        public string Tdlap { get; set; }
    }

    public class InvoiceSummary
    {
        public int STT { get; set; }
        public string NgayHD { get; set; }
        public string MST { get; set; }
        public string TenDV { get; set; }
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public string TrangThai { get; set; }
        public string TongTien { get; set; }
    }

    public class InvoiceDetail
    {
        public int STT { get; set; }
        public string NgayHD { get; set; }
        public string MST { get; set; }
        public string KyHieu { get; set; }
        public string SoHD { get; set; }
        public string MaHH { get; set; }
        public string TenHH { get; set; }
        public string DonVi { get; set; }
        public string SoLuong { get; set; }
        public string DonGia { get; set; }
        public string ThanhTien { get; set; }
    }
}