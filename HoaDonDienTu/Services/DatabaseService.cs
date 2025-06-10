// --- START OF FILE DatabaseService.cs ---
using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using HoaDonDienTu.Models.Database; // Namespace của SqliteInvoiceHeader, SqliteInvoiceItem
using HoaDonDienTu.Models;          // Namespace của InvoiceDetailApiResponse, InvoiceItemRawData, FormatHelper
using System.Diagnostics;
using System.Globalization;
using System.Reflection; // Cho Assembly và Embedded Resource
using System.Linq;     // Cho FirstOrDefault

namespace HoaDonDienTu.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _databaseDirectory;
        private string _currentDbFilePath;
        private SqliteConnection _connection;
        private readonly object _dbLock = new object();

        public DatabaseService()
        {
            string baseAppPath = AppDomain.CurrentDomain.BaseDirectory;
            _databaseDirectory = Path.Combine(baseAppPath, "Data");

            if (!Directory.Exists(_databaseDirectory))
            {
                Directory.CreateDirectory(_databaseDirectory);
            }
        }

        public async Task InitializeDatabaseForUser(string companyTaxCode)
        {
            if (string.IsNullOrWhiteSpace(companyTaxCode))
            {
                throw new ArgumentException("Mã số thuế không được để trống.", nameof(companyTaxCode));
            }

            _currentDbFilePath = Path.Combine(_databaseDirectory, $"{SanitizeFileName(companyTaxCode)}.db");

            lock (_dbLock)
            {
                CloseConnection();
                _connection = new SqliteConnection($"Data Source={_currentDbFilePath}");
                try
                {
                    _connection.Open();
                    // Bật foreign keys cho kết nối này (quan trọng)
                    using (var command = _connection.CreateCommand())
                    {
                        command.CommandText = "PRAGMA foreign_keys = ON;";
                        command.ExecuteNonQuery();
                    }
                    CreateTablesIfNotExists();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi mở hoặc tạo DB cho MST {companyTaxCode}: {ex.Message}");
                    throw;
                }
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName.Replace(".", "_"); // Thay thế cả dấu chấm để tránh tên file như 123.456.sqlite
        }

        private void CreateTablesIfNotExists()
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open)
            {
                Debug.WriteLine("Lỗi: Kết nối DB chưa được mở để tạo bảng.");
                return;
            }

            string scriptContent = "";
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                
                string resourceName = "HoaDonDienTu.DatabaseScripts.Schema.sql";

                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        // Thử tìm tên resource một cách linh hoạt hơn nếu tên đầy đủ không đúng
                        string foundResourceName = assembly.GetManifestResourceNames()
                                                  .FirstOrDefault(rn => rn.EndsWith("Schema.sql", StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(foundResourceName))
                        {
                            Debug.WriteLine($"Tìm thấy resource schema với tên: {foundResourceName}. Hãy cân nhắc cập nhật 'resourceName' trong code.");
                            using (Stream foundStream = assembly.GetManifestResourceStream(foundResourceName))
                            using (StreamReader reader = new StreamReader(foundStream))
                            {
                                scriptContent = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            // Liệt kê tất cả các resource để debug nếu không tìm thấy
                            var allResources = string.Join(", ", assembly.GetManifestResourceNames());
                            Debug.WriteLine($"Các resource có sẵn: {allResources}");
                            throw new FileNotFoundException($"Không thể tìm thấy resource file '{resourceName}' hoặc bất kỳ file nào kết thúc bằng 'Schema.sql'. Đảm bảo Build Action là Embedded Resource và tên resource chính xác.");
                        }
                    }
                    else
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            scriptContent = reader.ReadToEnd();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi đọc script schema từ Embedded Resource: {ex.Message}");
                throw;
            }

            if (string.IsNullOrWhiteSpace(scriptContent))
            {
                Debug.WriteLine("Lỗi: Nội dung script schema rỗng.");
                throw new Exception("Nội dung script schema không được rỗng.");
            }

            using (var command = _connection.CreateCommand())
            {
                command.CommandText = scriptContent;
                try
                {
                    command.ExecuteNonQuery();
                    Debug.WriteLine($"Thực thi script tạo schema cho DB '{Path.GetFileName(_currentDbFilePath)}' thành công.");
                }
                catch (SqliteException ex)
                {
                    Debug.WriteLine($"Lỗi khi thực thi script schema: {ex.Message} (ErrorCode: {ex.SqliteErrorCode})");
                    throw;
                }
            }
        }

        public bool InvoiceHeaderExists(string invoiceId, bool isInputInvoice)
        {
            if (string.IsNullOrEmpty(invoiceId) || _connection == null || _connection.State != System.Data.ConnectionState.Open) return false;

            string tableName = isInputInvoice ? "InputInv" : "OutputInv";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"SELECT COUNT(1) FROM {tableName} WHERE id = @id";
                command.Parameters.AddWithValue("@id", invoiceId);
                return Convert.ToInt32(command.ExecuteScalar()) > 0;
            }
        }


        public string GetLastDownloadedDate(string invoiceId, bool isInputInvoice)
        {
            if (string.IsNullOrEmpty(invoiceId) || _connection == null || _connection.State != System.Data.ConnectionState.Open) return null;

            string tableName = isInputInvoice ? "InputInv" : "OutputInv";
            using (var command = _connection.CreateCommand())
            {
                command.CommandText = $"SELECT LastDownloadedDetailDate FROM {tableName} WHERE id = @id;";
                command.Parameters.AddWithValue("@id", invoiceId);
                var result = command.ExecuteScalar();
                return (result != null && result != DBNull.Value) ? result.ToString() : null;
            }
        }

        public void SaveInvoiceDetail(InvoiceDetailApiResponse apiResponse, bool isInputInvoice, string companyTaxCode)
        {
            if (_connection == null || _connection.State != System.Data.ConnectionState.Open || apiResponse == null)
            {
                Debug.WriteLine("SaveInvoiceDetail: Kết nối DB không hợp lệ hoặc apiResponse là null.");
                return;
            }

            // API chi tiết trả về trường 'id' ở cấp gốc của JSON, đây là ID duy nhất của hóa đơn đó.
            // Chúng ta sẽ dùng nó làm khóa chính cho bảng Header.

            string invoiceApiId = (string)apiResponse.GetType().GetProperty("id")?.GetValue(apiResponse) ??
                                  apiResponse.ApiMaSoThueNguoiBan + apiResponse.ApiMauSoHoaDon + apiResponse.ApiKyHieuHoaDon + apiResponse.ApiSoHoaDon; // Dự phòng nếu ko có 'id'

            if (string.IsNullOrEmpty(invoiceApiId))
            {
                Debug.WriteLine("SaveInvoiceDetail: Không thể xác định ID cho hóa đơn từ apiResponse.");
                return;
            }


            string headerTable = isInputInvoice ? "InputInv" : "OutputInv";
            string detailTable = isInputInvoice ? "InputInvDetails" : "OutputInvDetails";

            lock (_dbLock)
            {
                using (var transaction = _connection.BeginTransaction())
                {
                    try
                    {
                        var sqliteHeader = MapApiResponseToSqliteHeader(apiResponse, invoiceApiId);
                        bool headerExists = InvoiceHeaderExists(sqliteHeader.id, isInputInvoice);

                        using (var cmdHeader = _connection.CreateCommand())
                        {
                            cmdHeader.Transaction = transaction;
                            if (headerExists)
                            {
                                cmdHeader.CommandText = BuildUpdateInvoiceHeaderCommand(headerTable);
                                AddParametersToCommand(cmdHeader, sqliteHeader, true); // isUpdate = true
                            }
                            else
                            {
                                cmdHeader.CommandText = BuildInsertInvoiceHeaderCommand(headerTable);
                                AddParametersToCommand(cmdHeader, sqliteHeader, false); // isUpdate = false
                            }
                            cmdHeader.ExecuteNonQuery();
                        }

                        using (var cmdDeleteDetails = _connection.CreateCommand())
                        {
                            cmdDeleteDetails.Transaction = transaction;
                            cmdDeleteDetails.CommandText = $"DELETE FROM {detailTable} WHERE idhdon = @idhdon;";
                            cmdDeleteDetails.Parameters.AddWithValue("@idhdon", sqliteHeader.id);
                            cmdDeleteDetails.ExecuteNonQuery();
                        }

                        if (apiResponse.Hdhhdvu != null)
                        {
                            foreach (var rawItem in apiResponse.Hdhhdvu)
                            {
                                var sqliteItem = MapRawItemToSqliteItem(rawItem, sqliteHeader.id);
                                using (var cmdDetail = _connection.CreateCommand())
                                {
                                    cmdDetail.Transaction = transaction;
                                    cmdDetail.CommandText = BuildInsertInvoiceItemCommand(detailTable);
                                    AddParametersToCommand(cmdDetail, sqliteItem);
                                    cmdDetail.ExecuteNonQuery();
                                }
                            }
                        }
                        transaction.Commit();
                        Debug.WriteLine($"Đã lưu/cập nhật HĐ ID: {sqliteHeader.id} vào bảng {headerTable}");
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi lưu hóa đơn vào DB (ID: {invoiceApiId}): {ex.ToString()}");
                        try { transaction.Rollback(); } catch (Exception rbEx) { Debug.WriteLine($"Lỗi rollback: {rbEx.Message}"); }
                        throw;
                    }
                }
            }
        }

        private SqliteInvoiceHeader MapApiResponseToSqliteHeader(InvoiceDetailApiResponse apiResp, string invoiceApiId)
        {
            // Đây là nơi bạn ánh xạ TẤT CẢ các trường từ apiResp
            // sang các thuộc tính của SqliteInvoiceHeader
            var header = new SqliteInvoiceHeader
            {
                id = invoiceApiId, // Sử dụng ID từ API Detail response
                khmshdon = apiResp.ApiMauSoHoaDon,
                khhdon = apiResp.ApiKyHieuHoaDon,
                shdon = apiResp.ApiSoHoaDon,
                tdlap = apiResp.ApiNgayLapHoaDonISO,
                nky = apiResp.ApiNgayKyISO,
                nbmst = apiResp.ApiMaSoThueNguoiBan,
                nbten = apiResp.ApiTenNguoiBan,
                nbdchi = apiResp.ApiDiaChiNguoiBan,
                nmmst = apiResp.ApiMaSoThueNguoiMua,
                nmten = apiResp.ApiTenNguoiMua,
                nmdchi = apiResp.ApiDiaChiNguoiMua,
                dvtte = apiResp.ApiDonViTienTe,
                tgia = apiResp.ApiTyGia?.ToString(CultureInfo.InvariantCulture),
                htttoan = apiResp.ApiHinhThucThanhToanCode ?? apiResp.ApiHinhThucThanhToanText,
                thtttoan = apiResp.ApiHinhThucThanhToanCode ?? apiResp.ApiHinhThucThanhToanText,
                tgtcthue = apiResp.ApiTongTienChuaThue,
                tgtthue = apiResp.ApiTongTienThue,
                ttcktmai = apiResp.ApiTongTienChietKhauTM,
                tgtttbso = apiResp.ApiTongTienThanhToan,
                tgtttbchu = apiResp.ApiTongTienThanhToanBangChu,
                tthai = apiResp.ApiTrangThaiHD_Code,
                ttxly = apiResp.ApiTinhTrangXuLy_Code,
                LastDownloadedDetailDate = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),

                // Các trường còn lại từ SqliteInvoiceHeader cần được ánh xạ từ apiResp nếu có
                // Ví dụ, nếu apiResp có các trường tương ứng:
                cqt = (string)apiResp.GetType().GetProperty("cqt")?.GetValue(apiResp), // Lấy động nếu tên khác
                hdon = (string)apiResp.GetType().GetProperty("hdon")?.GetValue(apiResp),
                // ... và cứ thế cho tất cả các trường khác bạn muốn lưu ...
                // Nếu một trường trong apiResp là một object hoặc array phức tạp (ví dụ cttkhac),
                // bạn có thể serialize nó thành JSON string ở đây:
                // cttkhac = apiResp.cttkhac != null ? JsonConvert.SerializeObject(apiResp.cttkhac) : null,

            };
            // Gán các trường từ JSON response vào header.
            // Ví dụ: header.pban = apiResp.pban; (nếu bạn thêm pban vào InvoiceDetailApiResponse)
            // Cần đảm bảo InvoiceDetailApiResponse có đủ các trường bạn muốn lấy từ JSON gốc.
            // Hoặc bạn parse trực tiếp từ JObject nếu không muốn thêm hết vào class C#.
            return header;
        }

        private SqliteInvoiceItem MapRawItemToSqliteItem(InvoiceItemRawData rawItem, string invoiceHeaderId)
        {
            return new SqliteInvoiceItem
            {
                idhdon = invoiceHeaderId,
                id = Guid.NewGuid().ToString(), // Hoặc lấy từ rawItem.ItemIdRaw nếu API cung cấp
                ten = rawItem.TenHHDVRaw,
                dvtinh = rawItem.DonViTinhRaw,
                sluong = rawItem.SoLuongRaw,
                dgia = rawItem.DonGiaRaw,
                stckhau = rawItem.SoTienChietKhauRaw,
                ltsuat = rawItem.LoaiThueSuatRaw,
                tsuat = rawItem.ThueSuatValueRaw,
                thtien = rawItem.ThanhTienChuaThueRaw,
                tthue = rawItem.TienThueRaw,
                sxep = rawItem.SoThuTuDongRaw,
                // ttkhac = rawItem.ThongTinKhacChiTiet != null ? JsonConvert.SerializeObject(rawItem.ThongTinKhacChiTiet) : null,
                // ... các trường khác ...
            };
        }

        // Các hàm Build...Command và AddParameters... cần được viết đầy đủ
        private string BuildInsertInvoiceHeaderCommand(string tableName)
        {
            // Tạo danh sách tên cột từ các thuộc tính của SqliteInvoiceHeader (trừ khi có attribute ignore)
            var properties = typeof(SqliteInvoiceHeader).GetProperties().Select(p => p.Name);
            string columns = string.Join(", ", properties);
            string parameters = string.Join(", ", properties.Select(p => "@" + p));
            return $"INSERT OR IGNORE INTO {tableName} ({columns}) VALUES ({parameters});";
        }

        private string BuildUpdateInvoiceHeaderCommand(string tableName)
        {
            var properties = typeof(SqliteInvoiceHeader).GetProperties().Where(p => p.Name != "id").Select(p => p.Name); // Không update khóa chính
            string setClauses = string.Join(", ", properties.Select(p => $"{p} = @{p}"));
            return $"UPDATE {tableName} SET {setClauses} WHERE id = @id;";
        }

        private string BuildInsertInvoiceItemCommand(string tableName)
        {
            var properties = typeof(SqliteInvoiceItem).GetProperties().Select(p => p.Name);
            string columns = string.Join(", ", properties);
            string parameters = string.Join(", ", properties.Select(p => "@" + p));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";
        }

        private void AddParametersToCommand(SqliteCommand command, SqliteInvoiceHeader header, bool isUpdate)
        {
            foreach (var prop in typeof(SqliteInvoiceHeader).GetProperties())
            {
                if (isUpdate && prop.Name == "id" && command.CommandText.StartsWith("UPDATE")) // id chỉ dùng trong WHERE clause của UPDATE
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(header) ?? DBNull.Value);
                    continue;
                }
                if (command.CommandText.Contains($"@{prop.Name}")) // Chỉ thêm nếu parameter tồn tại trong câu lệnh
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(header) ?? DBNull.Value);
                }
            }
        }


        private void AddParametersToCommand(SqliteCommand command, SqliteInvoiceItem item)
        {
            foreach (var prop in typeof(SqliteInvoiceItem).GetProperties())
            {
                if (command.CommandText.Contains($"@{prop.Name}"))
                {
                    command.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(item) ?? DBNull.Value);
                }
            }
        }

        public List<InvoiceDisplayItem> GetInvoiceDisplayItemsFromDb(string invoiceApiId, bool isInputInvoice, string companyTaxCode /* không dùng đến nhưng để đó nếu cần*/)
        {
            if (string.IsNullOrEmpty(invoiceApiId) || _connection == null || _connection.State != System.Data.ConnectionState.Open)
                return new List<InvoiceDisplayItem>();

            var displayItems = new List<InvoiceDisplayItem>();
            SqliteInvoiceHeader header = null;
            List<SqliteInvoiceItem> items = new List<SqliteInvoiceItem>();

            string headerTable = isInputInvoice ? "InputInv" : "OutputInv";
            string detailTable = isInputInvoice ? "InputInvDetails" : "OutputInvDetails";

            lock (_dbLock)
            {
                using (var cmdHeader = _connection.CreateCommand())
                {
                    cmdHeader.CommandText = $"SELECT * FROM {headerTable} WHERE id = @id";
                    cmdHeader.Parameters.AddWithValue("@id", invoiceApiId);
                    using (var reader = cmdHeader.ExecuteReader())
                    {
                        if (reader.Read()) header = MapReaderToSqliteHeader(reader);
                    }
                }

                if (header == null) return displayItems;

                using (var cmdItems = _connection.CreateCommand())
                {
                    cmdItems.CommandText = $"SELECT * FROM {detailTable} WHERE idhdon = @idhdon ORDER BY sxep ASC";
                    cmdItems.Parameters.AddWithValue("@idhdon", invoiceApiId);
                    using (var reader = cmdItems.ExecuteReader())
                    {
                        while (reader.Read()) items.Add(MapReaderToSqliteItem(reader));
                    }
                }

                if (items.Any())
                {
                    foreach (var dbItem in items)
                    {
                        var displayItem = new InvoiceDisplayItem
                        {
                            MauSoHoaDon = header.khmshdon,
                            KyHieuHoaDon = header.khhdon,
                            SoHoaDon = header.shdon,
                            NgayLapHoaDon = Helper.FormatHelper.FormatInvoiceDate(header.tdlap),
                            NgayKy = Helper.FormatHelper.FormatInvoiceDate(header.nky),
                            MaCQT = header.cqt,
                            DonViTienTe = header.dvtte,
                            TyGia = decimal.TryParse(header.tgia, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal tgVal) ? tgVal : (decimal?)null,
                            TenNguoiBan = header.nbten,
                            MaSoThueNguoiBan = header.nbmst,
                            DiaChiNguoiBan = header.nbdchi,
                            TenNguoiMua = header.nmten,
                            MaSoThueNguoiMua = header.nmmst,
                            DiaChiNguoiMua = header.nmdchi,
                            HinhThucThanhToan = header.thtttoan ?? header.htttoan,
                            SoThuTuDong = dbItem.sxep,
                            MaHHDV = "",
                            TenHHDV = dbItem.ten,
                            DonViTinh = dbItem.dvtinh,
                            SoLuong = dbItem.sluong,
                            DonGia = dbItem.dgia,
                            SoTienChietKhau = dbItem.stckhau,
                            LoaiThueSuat = dbItem.ltsuat,
                            ThanhTienChuaThue = dbItem.thtien,
                            TienThue = dbItem.tthue,
                            TongTienChuaThue_HD = header.tgtcthue,
                            TongTienThue_HD = header.tgtthue,
                            TongTienChietKhauTM_HD = header.ttcktmai,
                            TongTienThanhToan_HD = header.tgtttbso,
                            TongTienThanhToanBangChu_HD = header.tgtttbchu,
                            TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(header.tthai),
                            TinhTrangXuLy_HD = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(header.ttxly)
                        };
                        displayItems.Add(displayItem);
                    }
                }
                else
                {
                    var displayItemMsg = new InvoiceDisplayItem
                    {
                        MauSoHoaDon = header.khmshdon,
                        KyHieuHoaDon = header.khhdon,
                        SoHoaDon = header.shdon,
                        TenHHDV = "Không có dòng chi tiết nào được lưu trong DB cho hóa đơn này.",
                        TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(header.tthai)
                    };
                    displayItems.Add(displayItemMsg);
                }
            }
            return displayItems;
        }

        private SqliteInvoiceHeader MapReaderToSqliteHeader(SqliteDataReader reader)
        {
            var header = new SqliteInvoiceHeader();
            var properties = typeof(SqliteInvoiceHeader).GetProperties();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                if (property != null && !reader.IsDBNull(i))
                {
                    object value = reader.GetValue(i);
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var convertedValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                    property.SetValue(header, convertedValue);
                }
            }
            return header;
        }
        private SqliteInvoiceItem MapReaderToSqliteItem(SqliteDataReader reader)
        {
            var item = new SqliteInvoiceItem();
            var properties = typeof(SqliteInvoiceItem).GetProperties();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                var columnName = reader.GetName(i);
                var property = properties.FirstOrDefault(p => p.Name.Equals(columnName, StringComparison.OrdinalIgnoreCase));
                if (property != null && !reader.IsDBNull(i))
                {
                    object value = reader.GetValue(i);
                    Type propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                    var convertedValue = Convert.ChangeType(value, propertyType, CultureInfo.InvariantCulture);
                    property.SetValue(item, convertedValue);
                }
            }
            return item;
        }

        public void CloseConnection()
        {
            if (_connection?.State == System.Data.ConnectionState.Open)
            {
                _connection.Close();
            }
        }

        public void Dispose()
        {
            CloseConnection();
            _connection?.Dispose();
        }
    }
}
// --- END OF FILE DatabaseService.cs ---