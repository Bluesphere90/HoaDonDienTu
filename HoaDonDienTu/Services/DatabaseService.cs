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
using System.Linq;
using System.Data;     // Cho FirstOrDefault

namespace HoaDonDienTu.Services
{
    public class DatabaseService : IDisposable
    {
        private readonly string _databaseDirectory;
        //private string _currentDbFilePath;
        private string _connectionString;
        //private SqliteConnection _connection;
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

            try
            {
                // Tạo đường dẫn file database
                string dbFilePath = Path.Combine(_databaseDirectory, $"{SanitizeFileName(companyTaxCode)}.db");

                // Tạo connection string
                _connectionString = $"Data Source={dbFilePath};Foreign Keys=True;";

                Debug.WriteLine($"InitializeDatabaseForUser: Setting connection string for {dbFilePath}");

                // Test connection và tạo tables nếu cần
                await Task.Run(() =>
                {
                    using (var connection = new SqliteConnection(_connectionString))
                    {
                        connection.Open();
                        Debug.WriteLine($"Connection test successful. State: {connection.State}");

                        // Tạo tables nếu chưa có
                        CreateTablesIfNotExists(connection);
                    }
                });

                Debug.WriteLine($"Database initialized successfully for {companyTaxCode}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi khởi tạo DB cho MST {companyTaxCode}: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
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

        private void CreateTablesIfNotExists(SqliteConnection connection)
        {
            if (connection == null || connection.State != System.Data.ConnectionState.Open)
            {
                throw new ArgumentException("Connection must be open", nameof(connection));
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
                        // Thử tìm tên resource một cách linh hoạt hơn
                        string foundResourceName = assembly.GetManifestResourceNames()
                                                  .FirstOrDefault(rn => rn.EndsWith("Schema.sql", StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(foundResourceName))
                        {
                            Debug.WriteLine($"Tìm thấy resource schema với tên: {foundResourceName}");
                            using (Stream foundStream = assembly.GetManifestResourceStream(foundResourceName))
                            using (StreamReader reader = new StreamReader(foundStream))
                            {
                                scriptContent = reader.ReadToEnd();
                            }
                        }
                        else
                        {
                            var allResources = string.Join(", ", assembly.GetManifestResourceNames());
                            Debug.WriteLine($"Các resource có sẵn: {allResources}");
                            throw new FileNotFoundException($"Không thể tìm thấy resource file '{resourceName}'. Đảm bảo Build Action là Embedded Resource.");
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
                throw new Exception("Nội dung script schema không được rỗng.");
            }

            using (var command = connection.CreateCommand())
            {
                command.CommandText = scriptContent;
                try
                {
                    command.ExecuteNonQuery();
                    Debug.WriteLine($"Thực thi script tạo schema thành công.");
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
            if (string.IsNullOrEmpty(invoiceId) || string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine($"InvoiceHeaderExists: Invalid parameters. invoiceId={invoiceId}, connectionString={!string.IsNullOrEmpty(_connectionString)}");
                return false;
            }

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    string tableName = isInputInvoice ? "InputInv" : "OutputInv";
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT COUNT(1) FROM {tableName} WHERE id = @id";
                        command.Parameters.AddWithValue("@id", invoiceId);

                        var result = Convert.ToInt32(command.ExecuteScalar());
                        Debug.WriteLine($"InvoiceHeaderExists: {tableName}, ID={invoiceId}, Exists={result > 0}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi InvoiceHeaderExists: {ex.Message}");
                return false;
            }
        }

        public bool InvoiceDetailExists (string invoiceDetailId, bool isInputInvoice)
        {
            if (string.IsNullOrEmpty(invoiceDetailId) && string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine($"InvoiceDetailExists: Invalid parameters. invoiceId={invoiceDetailId}, connectionString={!string.IsNullOrEmpty(_connectionString)}");
                return false;
            }

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    string tableName = isInputInvoice ? "InputInvDetails" : "OutputInvDetails";
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT COUNT(1) FROM {tableName} WHERE id = @id";
                        command.Parameters.AddWithValue("@id", invoiceDetailId);
                        var result = Convert.ToInt32(command.ExecuteScalar());
                        Debug.WriteLine($"InvoiceDetailExists: {tableName}, ID={invoiceDetailId}, Exists={result > 0}");
                        return result > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi InvoiceDetailExists: {ex.Message}");
                return false;
            }
        }

        public void SaveInvoiceSummaryData(InvoiceSummaryData summaryData, bool isInputInvoice)
        {
            Debug.WriteLine("=== DEBUG SaveInvoiceSummaryData START ===");
            Debug.WriteLine($"summaryData null? {summaryData == null}");
            Debug.WriteLine($"summaryData.id: {summaryData?.id}");
            Debug.WriteLine($"isInputInvoice: {isInputInvoice}");
            Debug.WriteLine($"connectionString null? {string.IsNullOrEmpty(_connectionString)}");
            Debug.WriteLine("=========================================");

            if (summaryData == null || string.IsNullOrEmpty(summaryData.id))
            {
                Debug.WriteLine("SaveInvoiceSummaryData: summaryData null hoặc id rỗng.");
                return;
            }

            if (string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine("SaveInvoiceSummaryData: Connection string chưa được khởi tạo.");
                return;
            }

            string headerTable = isInputInvoice ? "InputInv" : "OutputInv";

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    Debug.WriteLine($"Connection opened successfully. State: {connection.State}");

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            // Kiểm tra tồn tại
                            bool headerExists = false;
                            using (var checkCommand = connection.CreateCommand())
                            {
                                checkCommand.Transaction = transaction;
                                checkCommand.CommandText = $"SELECT COUNT(1) FROM {headerTable} WHERE id = @id";
                                checkCommand.Parameters.AddWithValue("@id", summaryData.id);
                                headerExists = Convert.ToInt32(checkCommand.ExecuteScalar()) > 0;
                            }

                            Debug.WriteLine($"Header exists: {headerExists}");

                            using (var cmdHeader = connection.CreateCommand())
                            {
                                cmdHeader.Transaction = transaction;

                                if (headerExists)
                                {
                                    cmdHeader.CommandText = BuildUpdateInvoiceSummaryCommand(headerTable);
                                    AddSummaryParametersToCommand(cmdHeader, summaryData, true);
                                    Debug.WriteLine($"Executing UPDATE for summary ID: {summaryData.id}");
                                }
                                else
                                {
                                    cmdHeader.CommandText = BuildInsertInvoiceSummaryCommand(headerTable);
                                    AddSummaryParametersToCommand(cmdHeader, summaryData, false);
                                    Debug.WriteLine($"Executing INSERT for summary ID: {summaryData.id}");
                                    Debug.WriteLine($"Query: {cmdHeader.CommandText}");
                                }

                                int rowsAffected = cmdHeader.ExecuteNonQuery();
                                Debug.WriteLine($"Rows affected: {rowsAffected}");
                            }

                            transaction.Commit();
                            Debug.WriteLine($"Transaction committed successfully for summary ID: {summaryData.id}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error in transaction: {ex.Message}");
                            try
                            {
                                transaction.Rollback();
                                Debug.WriteLine("Transaction rolled back successfully");
                            }
                            catch (Exception rbEx)
                            {
                                Debug.WriteLine($"Rollback error: {rbEx.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi SaveInvoiceSummaryData cho ID {summaryData.id}: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public void SaveInvoiceDetailData(HangHoaDichVuRawData detailData, bool isInputInvoice)
        {
            if (detailData == null || string.IsNullOrEmpty(detailData.id)) 
            {
                Debug.WriteLine("SaveInvoiceDetailData: detailData null hoặc id rỗng.");
                return;
            }

            string detailTable = isInputInvoice ? "InputInvDetails" : "OutputInvDetails";
            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    Debug.WriteLine($"Connection opened successfully. State: {connection.State}");

                    using (var transaction = connection.BeginTransaction())
                    {
                        try
                        {
                            using (var cmdHeader = connection.CreateCommand())
                            {
                                cmdHeader.Transaction = transaction;
                                cmdHeader.CommandText = BuildInsertInvoiceDetailCommand(detailTable);
                                AddDetailParametersToCommand(cmdHeader, detailData,false);
                                int rowsAffected = cmdHeader.ExecuteNonQuery();
                            }
                            transaction.Commit();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                transaction.Rollback();
                                Debug.WriteLine("Transaction rolled back successfully");
                            }
                            catch (Exception rbEx)
                            {
                                Debug.WriteLine($"Rollback error: {rbEx.Message}");
                            }
                            throw;
                        }
                    }
                }
            }
            catch (Exception)
            {

                throw;
            }
        }


        // Method để lấy danh sách invoice summaries từ database (đơn giản hóa)
        public List<InvoiceSummary> GetInvoiceSummariesFromDb(bool isInputInvoice, DateTime? fromDate = null, DateTime? toDate = null, string statusFilter = null, string processingStatusFilter = null)
        {
            var summaries = new List<InvoiceSummary>();

            if (string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine("GetInvoiceSummariesFromDb: Connection string chưa được khởi tạo.");
                return summaries;
            }

            string headerTable = isInputInvoice ? "InputInv" : "OutputInv";
            var whereConditions = new List<string>();
            var parameters = new List<SqliteParameter>();

            // Thêm điều kiện lọc theo ngày
            if (fromDate.HasValue)
            {
                whereConditions.Add("DATE(tdlap) >= DATE(@fromDate)");
                parameters.Add(new SqliteParameter("@fromDate", fromDate.Value.ToString("yyyy-MM-dd")));
            }

            if (toDate.HasValue)
            {
                whereConditions.Add("DATE(tdlap) <= DATE(@toDate)");
                parameters.Add(new SqliteParameter("@toDate", toDate.Value.ToString("yyyy-MM-dd")));
            }

            // Thêm điều kiện lọc theo trạng thái
            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "All")
            {
                whereConditions.Add("tthai = @statusFilter");
                parameters.Add(new SqliteParameter("@statusFilter", statusFilter));
            }

            if (!string.IsNullOrEmpty(processingStatusFilter) && processingStatusFilter != "All")
            {
                whereConditions.Add("ttxly = @processingStatusFilter");
                parameters.Add(new SqliteParameter("@processingStatusFilter", processingStatusFilter));
            }

            string whereClause = whereConditions.Any() ? "WHERE " + string.Join(" AND ", whereConditions) : "";
            string sql = $@"
        SELECT 
            id, tlhdon, khmshdon, khhdon, shdon, tdlap, dvtte, tgia,
            nbten, nbmst, nbdchi, nmten, nmmst, nmdchi,
            tgtcthue, tgtthue, tthai, ttxly
        FROM {headerTable} 
        {whereClause}
        ORDER BY tdlap DESC, khmshdon ASC, shdon ASC";

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    Debug.WriteLine($"Connection opened for GetInvoiceSummariesFromDb. State: {connection.State}");

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = sql;
                        foreach (var param in parameters)
                        {
                            command.Parameters.Add(param);
                        }

                        using (var reader = command.ExecuteReader())
                        {
                            int stt = 1;
                            while (reader.Read())
                            {
                                var summary = new InvoiceSummary
                                {
                                    STT = stt++,
                                    id = reader.IsDBNull("id") ? "" : reader.GetString("id"),
                                    TenHoaDon = reader.IsDBNull("tlhdon") ? "" : reader.GetString("tlhdon"),
                                    MauHoaDon = reader.IsDBNull("khmshdon") ? "" : reader.GetString("khmshdon"),
                                    KyHieu = reader.IsDBNull("khhdon") ? "" : reader.GetString("khhdon"),
                                    SoHD = reader.IsDBNull("shdon") ? "" : reader.GetString("shdon"),
                                    NgayHD = reader.IsDBNull("tdlap") ? "" : Helper.FormatHelper.FormatInvoiceDate(reader.GetString("tdlap")),
                                    DVTienTe = reader.IsDBNull("dvtte") ? "" : reader.GetString("dvtte"),
                                    TyGia = reader.IsDBNull("tgia") ? (decimal?)null : reader.GetDecimal("tgia"),
                                    TenNguoiBan = reader.IsDBNull("nbten") ? "" : reader.GetString("nbten"),
                                    MSTNguoiBan = reader.IsDBNull("nbmst") ? "" : reader.GetString("nbmst"),
                                    DiaChiNguoiBan = reader.IsDBNull("nbdchi") ? "" : reader.GetString("nbdchi"),
                                    TenNguoiMua = reader.IsDBNull("nmten") ? "" : reader.GetString("nmten"),
                                    MSTNguoiMua = reader.IsDBNull("nmmst") ? "" : reader.GetString("nmmst"),
                                    DiaChiNguoiMua = reader.IsDBNull("nmdchi") ? "" : reader.GetString("nmdchi"),
                                    TongTienChuaThue = reader.IsDBNull("tgtcthue") ? (decimal?)null : reader.GetDecimal("tgtcthue"),
                                    TongTienThue = reader.IsDBNull("tgtthue") ? (decimal?)null : reader.GetDecimal("tgtthue"),
                                    TrangThai = Helper.FormatHelper.GetInvoiceStatusDescription(reader.IsDBNull("tthai") ? "" : reader.GetString("tthai")),
                                    KetQuaKiemTraHoaDon = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(reader.IsDBNull("ttxly") ? "" : reader.GetString("ttxly"))
                                };
                                summaries.Add(summary);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy invoice summaries từ DB: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Debug.WriteLine($"Đã lấy {summaries.Count} invoice summaries từ database");
            return summaries;
        }


        public string GetLastDownloadedDate(string invoiceId, bool isInputInvoice)
        {
            if (string.IsNullOrEmpty(invoiceId) || string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine($"GetLastDownloadedDate: Invalid parameters. invoiceId={invoiceId}, connectionString={!string.IsNullOrEmpty(_connectionString)}");
                return null;
            }

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();

                    string tableName = isInputInvoice ? "InputInv" : "OutputInv";
                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = $"SELECT LastDownloadedDetailDate FROM {tableName} WHERE id = @id;";
                        command.Parameters.AddWithValue("@id", invoiceId);

                        var result = command.ExecuteScalar();
                        string dateStr = (result != null && result != DBNull.Value) ? result.ToString() : null;
                        Debug.WriteLine($"GetLastDownloadedDate: {tableName}, ID={invoiceId}, Date={dateStr}");
                        return dateStr;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi GetLastDownloadedDate: {ex.Message}");
                return null;
            }
        }

        
        // Các hàm Build...Command và AddParameters... cần được viết đầy đủ
        private string BuildInsertInvoiceSummaryCommand(string tableName)
        {
            var properties = typeof(InvoiceSummaryData).GetProperties().Select(p => p.Name);

            //Debug.WriteLine("=== Properties từ InvoiceSummaryData ===");
            //foreach (var prop in properties)
            //{
            //    Debug.WriteLine($"Property: {prop}");
            //}

            string columns = string.Join(", ", properties);
            string parameters = string.Join(", ", properties.Select(p => "@" + p));

            //Debug.WriteLine($"=== SQL Command ===");
            //Debug.WriteLine($"Columns: {columns}");
            //Debug.WriteLine($"Parameters: {parameters}");

            return $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";
        }

        private string BuildUpdateInvoiceSummaryCommand(string tableName)
        {
            var properties = typeof(InvoiceSummaryData).GetProperties().Where(p => p.Name != "id").Select(p => p.Name); // Không update khóa chính
            string setClauses = string.Join(", ", properties.Select(p => $"{p} = @{p}"));
            return $"UPDATE {tableName} SET {setClauses} WHERE id = @id;";
        }

        private string BuildInsertInvoiceDetailCommand(string tableName)
        {
             var allProperties = typeof(InvoiceDetailData).GetProperties().Select(p => p.Name).ToHashSet();
            var desiredColumns = new[] { 
                "idhdon", "id", "dgia", "dvtinh", "ltsuat", "sluong",
                "stbchu", "stckhau", "stt", "tchat", "ten", "thtcthue", 
                "thtien", "tlckhau", "tsuat", "tthue","sxep", "ttkhac", 
                "dvtte", "tgia", "tthhdtrung"
            };

            // Chỉ lấy những cột vừa mong muốn vừa tồn tại trong class
            var selectedColumns = desiredColumns.Where(col => allProperties.Contains(col));

            string columns = string.Join(", ", selectedColumns);
            string parameters = string.Join(", ", selectedColumns.Select(p => "@" + p));
            return $"INSERT INTO {tableName} ({columns}) VALUES ({parameters});";
        }

        private void AddSummaryParametersToCommand(SqliteCommand command, InvoiceSummaryData summaryData, bool isUpdate)
        {
            foreach (var prop in typeof(InvoiceSummaryData).GetProperties())
            {
                string paramName = $"@{prop.Name}";
                if (command.CommandText.Contains(paramName))
                {
                    object value = prop.GetValue(summaryData);

                    // XỬ LÝ ĐặC BIỆT cho LastDownloadedDetailDate
                    if (prop.Name == "LastDownloadedDetailDate")
                    {
                        value = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture);
                    }
                    else if (value != null && prop.PropertyType == typeof(object))
                    {
                        value = value.ToString();
                    }

                    command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                }
            }
        }

        private void AddDetailParametersToCommand(SqliteCommand command, HangHoaDichVuRawData detailData, bool isUpdate)
        {
            foreach (var prop in typeof(InvoiceSummaryData).GetProperties())
            {
                string paramName = $"@{prop.Name}";
                if (command.CommandText.Contains(paramName))
                {
                    object value = prop.GetValue(detailData);

                    if (value != null && prop.PropertyType == typeof(object))
                    {
                        value = value.ToString();
                    }

                    command.Parameters.AddWithValue(paramName, value ?? DBNull.Value);
                }
            }
        }

        public List<InvoiceDisplayItem> GetInvoiceDisplayItemsFromDb(string invoiceApiId, bool isInputInvoice, string companyTaxCode)
        {
            var displayItems = new List<InvoiceDisplayItem>();

            if (string.IsNullOrEmpty(invoiceApiId) || string.IsNullOrEmpty(_connectionString))
            {
                Debug.WriteLine($"GetInvoiceDisplayItemsFromDb: Invalid parameters. invoiceApiId={invoiceApiId}, connectionString={!string.IsNullOrEmpty(_connectionString)}");
                return displayItems;
            }

            string headerTable = isInputInvoice ? "InputInv" : "OutputInv";
            string detailTable = isInputInvoice ? "InputInvDetails" : "OutputInvDetails";

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    Debug.WriteLine($"Connection opened for GetInvoiceDisplayItemsFromDb. State: {connection.State}");

                    // Lấy thông tin header
                    SqliteInvoiceHeader header = null;
                    using (var cmdHeader = connection.CreateCommand())
                    {
                        cmdHeader.CommandText = $"SELECT * FROM {headerTable} WHERE id = @id";
                        cmdHeader.Parameters.AddWithValue("@id", invoiceApiId);

                        using (var reader = cmdHeader.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                header = MapReaderToSqliteHeader(reader);
                                Debug.WriteLine($"Found header for invoice ID: {invoiceApiId}");
                            }
                            else
                            {
                                Debug.WriteLine($"No header found for invoice ID: {invoiceApiId}");
                                return displayItems;
                            }
                        }
                    }

                    // Lấy danh sách chi tiết
                    var items = new List<SqliteInvoiceItem>();
                    using (var cmdItems = connection.CreateCommand())
                    {
                        cmdItems.CommandText = $"SELECT * FROM {detailTable} WHERE idhdon = @idhdon ORDER BY sxep ASC";
                        cmdItems.Parameters.AddWithValue("@idhdon", invoiceApiId);

                        using (var reader = cmdItems.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(MapReaderToSqliteItem(reader));
                            }
                        }
                    }

                    Debug.WriteLine($"Found {items.Count} detail items for invoice ID: {invoiceApiId}");

                    // Tạo display items
                    if (items.Any())
                    {
                        foreach (var dbItem in items)
                        {
                            var displayItem = new InvoiceDisplayItem
                            {
                                // Thông tin chung từ header
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

                                // Thông tin chi tiết từ item
                                SoThuTuDong = dbItem.sxep,
                                MaHHDV = "", // API không cung cấp mã hàng hóa rõ ràng
                                TenHHDV = dbItem.ten,
                                DonViTinh = dbItem.dvtinh,
                                SoLuong = dbItem.sluong,
                                DonGia = dbItem.dgia,
                                SoTienChietKhau = dbItem.stckhau,
                                LoaiThueSuat = dbItem.ltsuat,
                                ThanhTienChuaThue = dbItem.thtien,
                                TienThue = dbItem.tthue,

                                // Thông tin tổng từ header (chỉ hiển thị ở dòng đầu tiên)
                                TongTienChuaThue_HD = dbItem.sxep == 1 ? header.tgtcthue : null,
                                TongTienThue_HD = dbItem.sxep == 1 ? header.tgtthue : null,
                                TongTienChietKhauTM_HD = dbItem.sxep == 1 ? header.ttcktmai : null,
                                TongTienThanhToan_HD = dbItem.sxep == 1 ? header.tgtttbso : null,
                                TongTienThanhToanBangChu_HD = dbItem.sxep == 1 ? header.tgtttbchu : "",

                                // Trạng thái
                                TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(header.tthai),
                                TinhTrangXuLy_HD = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(header.ttxly)
                            };

                            displayItems.Add(displayItem);
                        }
                    }
                    else
                    {
                        // Trường hợp không có chi tiết
                        var displayItemMsg = new InvoiceDisplayItem
                        {
                            MauSoHoaDon = header.khmshdon,
                            KyHieuHoaDon = header.khhdon,
                            SoHoaDon = header.shdon,
                            NgayLapHoaDon = Helper.FormatHelper.FormatInvoiceDate(header.tdlap),
                            TenNguoiBan = header.nbten,
                            TenNguoiMua = header.nmten,
                            TenHHDV = "Không có dòng chi tiết nào được lưu trong DB cho hóa đơn này.",
                            TongTienThanhToan_HD = header.tgtttbso,
                            TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(header.tthai),
                            TinhTrangXuLy_HD = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(header.ttxly)
                        };
                        displayItems.Add(displayItemMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi GetInvoiceDisplayItemsFromDb cho ID {invoiceApiId}: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }

            Debug.WriteLine($"Returning {displayItems.Count} display items for invoice ID: {invoiceApiId}");
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

        public void Dispose()
        {
            // Không còn connection để dispose
            // Chỉ cần clear connection string
            _connectionString = null;
            Debug.WriteLine("DatabaseService disposed");
        }

        public bool IsConnectionNull()
        {
            return string.IsNullOrEmpty(_connectionString);
        }

        public string GetConnectionState()
        {
            if (string.IsNullOrEmpty(_connectionString))
                return "NO_CONNECTION_STRING";

            try
            {
                using (var connection = new SqliteConnection(_connectionString))
                {
                    connection.Open();
                    return connection.State.ToString();
                }
            }
            catch (Exception ex)
            {
                return $"ERROR: {ex.Message}";
            }
        }

        public string GetCurrentDbPath()
        {
            if (string.IsNullOrEmpty(_connectionString))
                return "NO_CONNECTION_STRING";

            try
            {
                var builder = new SqliteConnectionStringBuilder(_connectionString);
                return builder.DataSource;
            }
            catch (Exception ex)
            {
                return $"ERROR_PARSING: {ex.Message}";
            }
        }
    }



}
// --- END OF FILE DatabaseService.cs ---