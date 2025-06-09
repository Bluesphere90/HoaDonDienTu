using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HoaDonDienTu.Models.Database // Hoặc namespace bạn chọn
{
    // =========================================================================
    // MODELS FOR SQLITE DATABASE TABLES
    // =========================================================================

    /// <summary>
    /// Represents the header/general information of an invoice stored in SQLite.
    /// This class is used for both InputInv and OutputInv tables.
    /// </summary>
    public class SqliteInvoiceHeader
    {
        // Các cột từ bảng InputInv / OutputInv
        // Tên thuộc tính khớp với tên cột trong SQLite (viết thường)

        public string nbmst { get; set; }
        public string khmshdon { get; set; }
        public string khhdon { get; set; }
        public string shdon { get; set; }
        public string cqt { get; set; }
        public string cttkhac { get; set; }       // JSON string for complex data
        public string dvtte { get; set; }
        public string hdon { get; set; }
        public string hsgcma { get; set; }
        public string hsgoc { get; set; }
        public string hthdon { get; set; }
        public string htttoan { get; set; }       // Payment method code/short text
        public string id { get; set; }            // PRIMARY KEY
        public string idtbao { get; set; }
        public string khdon { get; set; }
        public string khhdgoc { get; set; }
        public string khmshdgoc { get; set; }
        public string lhdgoc { get; set; }
        public string mhdon { get; set; }
        public string mtdiep { get; set; }
        public string mtdtchieu { get; set; }
        public string nbdchi { get; set; }
        public string chma { get; set; }          // Consider if needed
        public string chten { get; set; }         // Consider if needed
        public string nbhdktngay { get; set; }
        public string nbhdktso { get; set; }
        public string nbhdso { get; set; }
        public string nblddnbo { get; set; }
        public string nbptvchuyen { get; set; }
        public string nbstkhoan { get; set; }
        public string nbten { get; set; }
        public string nbtnhang { get; set; }
        public string nbtnvchuyen { get; set; }
        public string nbttkhac { get; set; }      // JSON string for complex data
        public string ncma { get; set; }          // ISO DateTime string
        public string ncnhat { get; set; }        // ISO DateTime string
        public string ngcnhat { get; set; }
        public string nky { get; set; }           // ISO DateTime string (Ngày ký)
        public string nmdchi { get; set; }
        public string nmmst { get; set; }
        public string nmstkhoan { get; set; }
        public string nmten { get; set; }
        public string nmtnhang { get; set; }
        public string nmtnmua { get; set; }
        public string nmttkhac { get; set; }      // JSON string for complex data
        public string ntao { get; set; }          // ISO DateTime string
        public string ntnhan { get; set; }         // ISO DateTime string
        public string pban { get; set; }
        public string ptgui { get; set; }
        public string shdgoc { get; set; }
        public string tchat { get; set; }
        public string tdlap { get; set; }         // ISO DateTime string (Ngày lập)
        public string tgia { get; set; }          // Could be decimal or string for currency symbols
        public decimal? tgtcthue { get; set; }    // Total amount before tax
        public decimal? tgtthue { get; set; }     // Total tax amount
        public string tgtttbchu { get; set; }   // Total amount in words
        public decimal? tgtttbso { get; set; }    // Total payment amount (numeric)
        public string thdon { get; set; }
        public string thlap { get; set; }
        public string thttlphi { get; set; }     // JSON string for fee structure
        public string thttltsuat { get; set; }  // JSON string for tax rate summary
        public string tlhdon { get; set; }
        public decimal? ttcktmai { get; set; }    // Total discount amount
        public string tthai { get; set; }         // Invoice status code
        public string ttkhac { get; set; }        // JSON string for other general info
        public string tttbao { get; set; }
        public string ttttkhac { get; set; }      // JSON string for other transaction info
        public string ttxly { get; set; }         // Processing status code
        public string tvandnkntt { get; set; }
        public string mhso { get; set; }
        public string ladhddt { get; set; }
        public string mkhang { get; set; }
        public string nbsdthoai { get; set; }
        public string nbdctdtu { get; set; }
        public string nbfax { get; set; }
        public string nbwebsite { get; set; }
        public string nbcks { get; set; }         // JSON string for seller's digital signature info
        public string nmsdthoai { get; set; }
        public string nmdctdtu { get; set; }
        public string nmcmnd { get; set; }
        public string nmcks { get; set; }         // JSON string for buyer's digital signature info (if any)
        public string bhphap { get; set; }
        public string hddunlap { get; set; }
        public string gchdgoc { get; set; }
        public string tbhgtngay { get; set; }
        public string bhpldo { get; set; }
        public string bhpcbo { get; set; }
        public string bhpngay { get; set; }
        public string tdlhdgoc { get; set; }
        public string tgtphi { get; set; }        // Should be decimal? if it's a fee amount
        public string unhiem { get; set; }
        public string mstdvnunlhdon { get; set; }
        public string tdvnunlhdon { get; set; }
        public string nbmdvqhnsach { get; set; }
        public string nbsqdinh { get; set; }
        public string nbncqdinh { get; set; }
        public string nbcqcqdinh { get; set; }
        public string nbhtban { get; set; }
        public string nmmdvqhnsach { get; set; }
        public string nmddvchden { get; set; }
        public string nmtgvchdtu { get; set; }
        public string nmtgvchdden { get; set; }
        public string nbtnban { get; set; }
        public string dcdvnunlhdon { get; set; }
        public string dksbke { get; set; }
        public string dknlbke { get; set; }
        public string thtttoan { get; set; }      // Payment method description (check if different from htttoan)
        public string msttcgp { get; set; }
        public string cqtcks { get; set; }        // JSON string for CQT's digital signature info
        public string gchu { get; set; }
        public string kqcht { get; set; }
        public string hdntgia { get; set; }
        public string tgtkcthue { get; set; }     // Should be decimal? (Tổng tiền không chịu thuế)
        public string tgtkhac { get; set; }       // Should be decimal? (Tổng tiền khác)
        public string nmshchieu { get; set; }
        public string nmnchchieu { get; set; }
        public string nmnhhhchieu { get; set; }
        public string nmqtich { get; set; }
        public string ktkhthue { get; set; }
        public string qrcode { get; set; }
        public string ttmstten { get; set; }
        public string ladhddtten { get; set; }
        public string hdxkhau { get; set; }
        public string hdxkptquan { get; set; }
        public string hdgktkhthue { get; set; }
        public string hdonLquans { get; set; }    // JSON string for related invoices array
        public string tthdclquan { get; set; }    // Should be int/bool (0/1)
        public string pdndungs { get; set; }
        public string hdtbssrses { get; set; }
        public string hdTrung { get; set; }
        public string isHDTrung { get; set; }     // Should be int/bool (0/1)

        // Column added for cache management
        public string LastDownloadedDetailDate { get; set; } // TEXT NOT NULL, ISO DateTime string
    }

    /// <summary>
    /// Represents a single item/line detail of an invoice stored in SQLite.
    /// This class is used for both InputInvDetails and OutputInvDetails tables.
    /// </summary>
    public class SqliteInvoiceItem
    {
        public string idhdon { get; set; }     // FOREIGN KEY to SqliteInvoiceHeader.id
        public string id { get; set; }         // PRIMARY KEY of this item
        public decimal? dgia { get; set; }       // Unit price
        public string dvtinh { get; set; }     // Unit of measure
        public string ltsuat { get; set; }     // Tax rate type (e.g., "10%", "KCT")
        public decimal? sluong { get; set; }     // Quantity
        public string stbchu { get; set; }     // Amount in words for item (usually null)
        public decimal? stckhau { get; set; }    // Discount amount for item
        public int? stt { get; set; }          // Original sequence number from API (if meaningful)
        public string tchat { get; set; }      // Item type/nature
        public string ten { get; set; }        // Item name/description
        public string thtcthue { get; set; }   // (May not be needed if other amount fields are present)
        public decimal? thtien { get; set; }     // Amount before tax for item
        public decimal? tlckhau { get; set; }    // Discount rate for item
        public decimal? tsuat { get; set; }      // Tax rate (numeric, e.g., 0.1 for 10%)
        public decimal? tthue { get; set; }      // Tax amount for item
        public int? sxep { get; set; }         // Actual sort order for display
        public string ttkhac { get; set; }     // JSON string for other item-specific info
        public string dvtte { get; set; }      // Currency for item (usually null, inherits from header)
        public decimal? tgia { get; set; }       // Exchange rate for item (usually null)
        public string tthhdtrung { get; set; } // Specific field, clarify its purpose
    }
}
