// --- START OF FILE InvoiceWindow.xaml.cs ---
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.IO.Compression;
using HoaDonDienTu.Models;
using System.Windows.Forms; // For FolderBrowserDialog
using System.Globalization; // For CultureInfo
using OfficeOpenXml;         // For EPPlus
using OfficeOpenXml.Style;    // For EPPlus Style
using System.Drawing;         // For System.Drawing.Color (EPPlus)
using System.Xml.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;      // For XDocument, XElement (XML Export)
using HoaDonDienTu.Services;



namespace HoaDonDienTu
{
    public partial class InvoiceWindow : Window
    {
        private HttpClient client;
        private bool isMuaVao = true;
        private DatabaseService databaseService;
        private string currentCompanyTaxCode;
        private ObservableCollection<InvoiceSummary> invoiceSummaryList = new ObservableCollection<InvoiceSummary>();
        private ObservableCollection<InvoiceDisplayItem> invoiceDetailList = new ObservableCollection<InvoiceDisplayItem>();
        private ObservableCollection<FileInfo> xmlFileList = new ObservableCollection<FileInfo>();

        private List<KeyValuePair<string, string>> invoiceStatusList = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> checkResultList = new List<KeyValuePair<string, string>>();

        public InvoiceWindow(DatabaseService dbService, string companyTaxCode)
        {
            try
            {
                InitializeComponent();
                // Lưu reference đến database service và company info
                databaseService = dbService ?? throw new ArgumentNullException(nameof(dbService));
                currentCompanyTaxCode = companyTaxCode ?? throw new ArgumentNullException(nameof(companyTaxCode));

                Debug.WriteLine($"InvoiceWindow khởi tạo với MST: {currentCompanyTaxCode}");

                invoiceSummaryList = new ObservableCollection<InvoiceSummary>();
                invoiceDetailList = new ObservableCollection<InvoiceDisplayItem>();
                xmlFileList = new ObservableCollection<FileInfo>();

                invoiceStatusList = new List<KeyValuePair<string, string>>();
                checkResultList = new List<KeyValuePair<string, string>>();

                client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

                if (string.IsNullOrEmpty(App.AuthToken))
                {
                    System.Windows.MessageBox.Show("Token xác thực không hợp lệ hoặc đã hết hạn. Vui lòng đăng nhập lại.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    var loginWindow = new MainWindow();
                    loginWindow.Show();
                    this.Close();
                    return;
                }
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AuthToken);

                dgTongHop.ItemsSource = invoiceSummaryList;
                dgChiTiet.ItemsSource = invoiceDetailList;
                lvXMLFiles.ItemsSource = xmlFileList;

                dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dpDenNgay.SelectedDate = DateTime.Now;

                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HoaDonDienTu");
                if (!Directory.Exists(defaultPath))
                {
                    try { Directory.CreateDirectory(defaultPath); }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Không thể tạo thư mục mặc định: {ex.Message}");
                        defaultPath = Path.GetTempPath();
                    }
                }
                txtXMLFolderPath.Text = defaultPath;
                this.Title = $"Quản Lý Hóa Đơn Điện Tử - MST: {currentCompanyTaxCode}";
                LoadStatusAndCheckResultLists();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo InvoiceWindow: {ex.Message}");
                if (ex.InnerException != null) Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                System.Windows.MessageBox.Show($"Lỗi khởi tạo cửa sổ hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        // ... (LoadStatusAndCheckResultLists, UpdateKQKTList, UpdateUIState, optMua_Changed, optBan_Changed, etc. giữ nguyên) ...
        #region UI Event Handlers (Filters, Buttons, etc. - Giữ nguyên các hàm bạn đã có)
        private void LoadStatusAndCheckResultLists()
        {
            invoiceStatusList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("All", "Tất cả"),
                new KeyValuePair<string, string>("1", "Hóa đơn mới"),
                new KeyValuePair<string, string>("2", "Hóa đơn thay thế"),
                new KeyValuePair<string, string>("3", "Hóa đơn điều chỉnh"),
                new KeyValuePair<string, string>("4", "Hóa đơn bị thay thế"),
                new KeyValuePair<string, string>("5", "Hóa đơn đã bị điều chỉnh"),
                new KeyValuePair<string, string>("6", "Hóa đơn bị hủy")
            };
            UpdateKQKTList(); // Gọi để khởi tạo checkResultList và cboKQKT
            cboTTHD.DisplayMemberPath = "Value";
            cboTTHD.SelectedValuePath = "Key";
            cboTTHD.ItemsSource = invoiceStatusList;
            if (cboTTHD.Items.Count > 0) cboTTHD.SelectedIndex = 0;
        }

        private void UpdateKQKTList()
        {
            if (cboKQKT == null) return;
            if (isMuaVao)
            {
                checkResultList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", "Tất cả"),
                    new KeyValuePair<string, string>("0", "Chưa kiểm tra"),
                    new KeyValuePair<string, string>("1", "Hợp lệ"),
                    new KeyValuePair<string, string>("2", "Không hợp lệ")
                };
            }
            else
            {
                checkResultList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", "Tất cả"),
                    new KeyValuePair<string, string>("0", "Tổng cục thuế đã nhận"),
                    new KeyValuePair<string, string>("1", "Hợp lệ"),
                    new KeyValuePair<string, string>("2", "Sai MST NNT"),
                    new KeyValuePair<string, string>("3", "Sai tên NNT"),
                    new KeyValuePair<string, string>("4", "Sai địa chỉ NNT"),
                    new KeyValuePair<string, string>("5", "Không đủ ĐK kiểm tra"),
                    new KeyValuePair<string, string>("6", "Đang kiểm tra CQT"),
                    new KeyValuePair<string, string>("7", "Sai định dạng CQT"),
                    new KeyValuePair<string, string>("8", "HĐ không tồn tại trên HT CQT"),
                    new KeyValuePair<string, string>("9", "Sai Mã CQT trên HĐ")
                };
            }
            cboKQKT.DisplayMemberPath = "Value";
            cboKQKT.SelectedValuePath = "Key";
            cboKQKT.ItemsSource = checkResultList;
            if (cboKQKT.Items.Count > 0) cboKQKT.SelectedIndex = 0;
        }
        private void UpdateUIState()
        {
            txtXMLFolderPath.IsEnabled = chkXmlZip.IsChecked == true;
            btnChonFolder.IsEnabled = chkXmlZip.IsChecked == true;

            if (chkXmlZip.IsChecked == true && !string.IsNullOrEmpty(txtXMLFolderPath.Text) && !Directory.Exists(txtXMLFolderPath.Text))
            {
                try { Directory.CreateDirectory(txtXMLFolderPath.Text); }
                catch (Exception ex) { Debug.WriteLine($"Lỗi khi tạo thư mục: {ex.Message}"); }
            }
        }
        private void optMua_Changed(object sender, RoutedEventArgs e)
        {
            if (optMua.IsChecked == true) { isMuaVao = true; UpdateKQKTList(); }
        }
        private void optBan_Changed(object sender, RoutedEventArgs e)
        {
            if (optBan.IsChecked == true) { isMuaVao = false; UpdateKQKTList(); }
        }
        private void cboTTHD_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void cboKQKT_SelectionChanged(object sender, SelectionChangedEventArgs e) { }
        private void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ValidateDateRange();
        private void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ValidateDateRange();
        private void ValidateDateRange()
        {
            if (dpTuNgay.SelectedDate.HasValue && dpDenNgay.SelectedDate.HasValue)
            {
                lblSaiNgay.Visibility = dpTuNgay.SelectedDate.Value > dpDenNgay.SelectedDate.Value ? Visibility.Visible : Visibility.Collapsed;
            }
            else { lblSaiNgay.Visibility = Visibility.Collapsed; }
        }
        private void chkXmlZip_Checked(object sender, RoutedEventArgs e) => UpdateUIState();
        private void chkXmlZip_Unchecked(object sender, RoutedEventArgs e) => UpdateUIState();
        private void btnChonFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Chọn thư mục để lưu file Zip XML";
                dialog.ShowNewFolderButton = true;
                if (!string.IsNullOrEmpty(txtXMLFolderPath.Text) && Directory.Exists(txtXMLFolderPath.Text))
                {
                    dialog.SelectedPath = txtXMLFolderPath.Text;
                }
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txtXMLFolderPath.Text = dialog.SelectedPath;
                }
            }
        }
        private async void btnTaiHoaDon_Click(object sender, RoutedEventArgs e)
        {
            // Debug: Kiểm tra binding
            Debug.WriteLine($"=== DEBUG: btnTaiHoaDon_Click bắt đầu ===");
            Debug.WriteLine($"invoiceSummaryList null? {invoiceSummaryList == null}");
            Debug.WriteLine($"dgTongHop.ItemsSource null? {dgTongHop.ItemsSource == null}");
            Debug.WriteLine($"dgTongHop.ItemsSource == invoiceSummaryList? {dgTongHop.ItemsSource == invoiceSummaryList}");
            Debug.WriteLine($"invoiceSummaryList.Count hiện tại: {invoiceSummaryList?.Count ?? -1}");

            if (lblSaiNgay.Visibility == Visibility.Visible)
            {
                System.Windows.MessageBox.Show("Khoảng thời gian không hợp lệ. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            if (!dpTuNgay.SelectedDate.HasValue || !dpDenNgay.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn khoảng thời gian tìm kiếm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning); return;
            }
            bool clearOldData = false;
            if (invoiceSummaryList.Any() || invoiceDetailList.Any())
            {
                var result = System.Windows.MessageBox.Show("Bạn có muốn xóa dữ liệu cũ không?\nChọn [Yes] để xóa hoặc [No] để ghi kế tiếp.", "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                clearOldData = (result == MessageBoxResult.Yes);
            }
            if (clearOldData)
            {
                Debug.WriteLine("=== Xóa dữ liệu cũ ===");
                invoiceSummaryList.Clear();
                invoiceDetailList.Clear();
                xmlFileList.Clear();
                Debug.WriteLine($"Sau khi clear: invoiceSummaryList.Count = {invoiceSummaryList.Count}");
            }

            bool isBackground = chkBackgroundMode.IsChecked == true;
            Debug.WriteLine($"=== Bắt đầu DownloadInvoicesAsync, Background = {isBackground} ===");

            await DownloadInvoicesAsync(isBackground);
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            App.AuthToken = null;
            var loginWindow = new MainWindow();
            loginWindow.Show();
            this.Close();
        }
        private void btnChiTietXML_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.MessageBox.Show("Chức năng trích xuất XML sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Data Fetching (DownloadInvoicesAsync, GetInvoiceSummariesAsync, etc. - Giữ nguyên)

        // Trong InvoiceWindow.xaml.cs

        private async Task DownloadInvoicesAsync(bool isBackgroundMode = false)
        {
            try
            {
                Debug.WriteLine("=== DownloadInvoicesAsync bắt đầu ===");
                lblStatus.Text = "Đang tải dữ liệu...";
                btnTaiHoaDon.IsEnabled = false;
                btnOpenExportMenu.IsEnabled = false;

                DateTime tuNgay = dpTuNgay.SelectedDate.Value;
                DateTime denNgay = dpDenNgay.SelectedDate.Value;
                string tthai = (string)cboTTHD.SelectedValue;
                string ttxly = (string)cboKQKT.SelectedValue;

                Debug.WriteLine($"Khoảng thời gian: {tuNgay:dd/MM/yyyy} - {denNgay:dd/MM/yyyy}");
                Debug.WriteLine($"Trạng thái HĐ: {tthai}, Kết quả kiểm tra: {ttxly}");
                Debug.WriteLine($"Loại tìm kiếm: {(isMuaVao ? "Mua vào" : "Bán ra")}");

                var datePeriods = SplitDateRange(tuNgay, denNgay);
                List<InvoiceIdentifier> invoiceIdentifiers = new List<InvoiceIdentifier>();
                var stopwatch = Stopwatch.StartNew();

                // 1. Tải danh sách hóa đơn tổng hợp
                bool shouldDisplaySummary = chkTH.IsChecked == true;
                bool needToFetchIdentifiers = chkTH.IsChecked == true || chkCT.IsChecked == true || chkXmlZip.IsChecked == true;

                Debug.WriteLine($"shouldDisplaySummary: {shouldDisplaySummary}");
                Debug.WriteLine($"needToFetchIdentifiers: {needToFetchIdentifiers}");
                Debug.WriteLine($"chkTH.IsChecked: {chkTH.IsChecked}");
                Debug.WriteLine($"chkCT.IsChecked: {chkCT.IsChecked}");
                Debug.WriteLine($"chkXmlZip.IsChecked: {chkXmlZip.IsChecked}");

                if (needToFetchIdentifiers)
                {
                    if (shouldDisplaySummary)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = "Đang tải danh sách hóa đơn tổng hợp...");
                    }
                    else
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = "Đang tìm kiếm hóa đơn để xử lý...");
                    }
                    
                    Debug.WriteLine($"Số periods: {datePeriods.Count}");

                    foreach (var period in datePeriods)
                    {
                        Debug.WriteLine($"Xử lý period: {period.Item1:dd/MM/yyyy} - {period.Item2:dd/MM/yyyy}");

                        string baseUrl = isMuaVao ? "https://hoadondientu.gdt.gov.vn:30000/query/invoices/purchase"
                                                  : "https://hoadondientu.gdt.gov.vn:30000/query/invoices/sold";
                        string baseUrlSco = isMuaVao ? "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/purchase"
                                                     : "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold";

                        string search = $"tdlap=ge={period.Item1:dd/MM/yyyy}T00:00:00;tdlap=le={period.Item2:dd/MM/yyyy}T23:59:59";
                        if (tthai != "All") search += $";tthai=={tthai}";
                        if (ttxly != "All") search += $";ttxly=={ttxly}";
                        string sort = "tdlap:asc,khmshdon:asc,shdon:asc";
                        int size = 50;

                        string urlQuery = $"{baseUrl}?sort={sort}&size={size}&search={search}";
                        string urlScoQuery = $"{baseUrlSco}?sort={sort}&size={size}&search={search}";

                        Debug.WriteLine($"URL Query: {urlQuery}");
                        Debug.WriteLine($"URL SCO Query: {urlScoQuery}");

                        Debug.WriteLine($"Trước khi gọi GetInvoiceSummariesAsync (Query), invoiceSummaryList.Count = {invoiceSummaryList.Count}");
                        await GetInvoiceSummariesAsync(urlQuery, invoiceIdentifiers, invoiceSummaryList.Count, false, !shouldDisplaySummary);
                        Debug.WriteLine($"Sau khi gọi GetInvoiceSummariesAsync (Query), invoiceSummaryList.Count = {invoiceSummaryList.Count}");
                        await System.Windows.Threading.Dispatcher.Yield();

                        Debug.WriteLine($"Trước khi gọi GetInvoiceSummariesAsync (SCO), invoiceSummaryList.Count = {invoiceSummaryList.Count}");
                        await GetInvoiceSummariesAsync(urlScoQuery, invoiceIdentifiers, invoiceSummaryList.Count, true, !shouldDisplaySummary);
                        Debug.WriteLine($"Sau khi gọi GetInvoiceSummariesAsync (SCO), invoiceSummaryList.Count = {invoiceSummaryList.Count}");
                        await System.Windows.Threading.Dispatcher.Yield();
                    }

                    // Debug: Kiểm tra số lượng dữ liệu sau khi tải
                    Debug.WriteLine($"=== Kết thúc việc tải summary ===");
                    Debug.WriteLine($"Tổng hóa đơn tìm thấy: {invoiceIdentifiers.Count}");

                    if (shouldDisplaySummary)
                    {
                        Debug.WriteLine($"Số dòng trong invoiceSummaryList: {invoiceSummaryList.Count}");
                        Debug.WriteLine($"dgTongHop.Items.Count: {dgTongHop.Items.Count}");

                        // Kiểm tra một vài item đầu tiên
                        for (int i = 0; i < Math.Min(3, invoiceSummaryList.Count); i++)
                        {
                            var item = invoiceSummaryList[i];
                            Debug.WriteLine($"Item {i}: STT={item.STT}, SoHD={item.SoHD}, TenNguoiBan={item.TenNguoiBan}");
                        }
                    }
                }

                // 2. Tải chi tiết hóa đơn
                if (chkCT.IsChecked == true && invoiceIdentifiers.Any())
                {
                    string initialStatus = isBackgroundMode ? "Đang tải chi tiết (nền)..." : "Đang tải chi tiết hóa đơn...";
                    System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = initialStatus);

                    int baseDetailDelay = isBackgroundMode ? 3000 : 500;

                    for (int i = 0; i < invoiceIdentifiers.Count; i++)
                    {
                        string currentStatus = $"{(isBackgroundMode ? "Tải nền: " : "")}Chi tiết HĐ {i + 1}/{invoiceIdentifiers.Count} ({invoiceIdentifiers[i].Shdon})...";
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = currentStatus);

                        if (i > 0) // Không delay cho item đầu tiên
                        {
                            await Task.Delay(baseDetailDelay);
                        }
                        await System.Windows.Threading.Dispatcher.Yield();
                        await GetInvoiceDetailsAsync(invoiceIdentifiers[i], isBackgroundMode);
                    }
                }

                // 3. Tải XML/HTML
                if (chkXmlZip.IsChecked == true && invoiceIdentifiers.Any())
                {
                    System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = "Đang tải file XML/HTML...");
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string baseUserSelectedFolder = txtXMLFolderPath.Text; // Thư mục người dùng chọn
                    string sessionTimestampFolder = Path.Combine(baseUserSelectedFolder, $"HoaDonTaiVe_{timestamp}");

                    if (!Directory.Exists(sessionTimestampFolder)) Directory.CreateDirectory(sessionTimestampFolder);

                    int xmlDelay = isBackgroundMode ? 1500 : 200; // Delay ngắn hơn cho việc tải file

                    for (int i = 0; i < invoiceIdentifiers.Count; i++)
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = $"Đang tải XML/HTML {i + 1}/{invoiceIdentifiers.Count} ({invoiceIdentifiers[i].Shdon})...");
                        if (i > 0) await Task.Delay(xmlDelay); // Delay giữa các lần tải XML
                        await System.Windows.Threading.Dispatcher.Yield();

                        // Thư mục giải nén cụ thể cho mỗi hóa đơn, nằm trong sessionTimestampFolder
                        string invoiceSpecificExtractFolder = Path.Combine(sessionTimestampFolder, $"{invoiceIdentifiers[i].Nbmst}_{invoiceIdentifiers[i].Khhdon.Replace("/", "-")}_{invoiceIdentifiers[i].Shdon}");
                        if (!Directory.Exists(invoiceSpecificExtractFolder)) Directory.CreateDirectory(invoiceSpecificExtractFolder);

                        // File ZIP sẽ được lưu vào sessionTimestampFolder, sau đó giải nén vào invoiceSpecificExtractFolder
                        await DownloadInvoiceXMLAsync(invoiceIdentifiers[i], sessionTimestampFolder, invoiceSpecificExtractFolder);
                    }
                    UpdateXMLFileList(sessionTimestampFolder); // Liệt kê file từ thư mục chứa các ZIP và các thư mục con đã giải nén
                }

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = $"Hoàn thành. Thời gian xử lý: {elapsed:hh\\:mm\\:ss}");

                Debug.WriteLine($"=== Kết thúc DownloadInvoicesAsync ===");
                Debug.WriteLine($"invoiceSummaryList.Count cuối cùng: {invoiceSummaryList.Count}");
                Debug.WriteLine($"dgTongHop.Items.Count cuối cùng: {dgTongHop.Items.Count}");

                System.Windows.MessageBox.Show("Tải hóa đơn hoàn tất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"LỖI DownloadInvoicesAsync: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");

                System.Windows.MessageBox.Show($"Lỗi khi tải hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = "Đã xảy ra lỗi khi tải hóa đơn");
                Debug.WriteLine($"Lỗi DownloadInvoicesAsync: {ex.ToString()}");
            }
            finally
            {
                btnTaiHoaDon.IsEnabled = true;
                btnOpenExportMenu.IsEnabled = true;
            }
        }

        private List<Tuple<DateTime, DateTime>> SplitDateRange(DateTime startDate, DateTime endDate)
        {
            var periods = new List<Tuple<DateTime, DateTime>>();
            DateTime currentStart = startDate;
            while (currentStart <= endDate)
            {
                DateTime currentEnd = new DateTime(currentStart.Year, currentStart.Month, DateTime.DaysInMonth(currentStart.Year, currentStart.Month));
                periods.Add(new Tuple<DateTime, DateTime>(currentStart, currentEnd > endDate ? endDate : currentEnd));
                currentStart = currentEnd.AddDays(1);
            }
            return periods;
        }
        private async Task GetInvoiceSummariesAsync(string initialUrl, List<InvoiceIdentifier> invoiceIdentifiers, int startIndex, bool isScoQuery = false, bool silentMode = false)
        {
            Debug.WriteLine($"=== GetInvoiceSummariesAsync bắt đầu ===");
            Debug.WriteLine($"URL: {initialUrl}");
            Debug.WriteLine($"isScoQuery: {isScoQuery}, silentMode: {silentMode}");
            Debug.WriteLine($"startIndex: {startIndex}");

            string currentUrl = initialUrl;
            int currentIndex = startIndex;
            int pageCount = 0;

            try
            {
                while (!string.IsNullOrEmpty(currentUrl))
                {
                    pageCount++;
                    Debug.WriteLine($"--- Trang {pageCount}, URL: {currentUrl} ---");

                    var response = await client.GetAsync(currentUrl);
                    Debug.WriteLine($"Response Status: {response.StatusCode}");

                    //// --- BẮT ĐẦU CODE GHI FILE TXT ---
                    //string responseContentForLog = "ERROR_READING_CONTENT"; // Giá trị mặc định nếu không đọc được
                    //if (response != null && response.Content != null)
                    //{
                    //    try
                    //    {
                    //        responseContentForLog = await response.Content.ReadAsStringAsync(); // Đọc content một lần

                    //        // Tạo tên file dựa trên loại query và timestamp/page
                    //        string typeQuery = isScoQuery ? "SCO" : "Query";
                    //        string endpointType = currentUrl.Contains("purchase") ? "Purchase" : "Sold";

                    //        // Lấy một phần của query string để làm tên file dễ nhận biết hơn (nếu có)
                    //        string queryStringPart = "";
                    //        if (currentUrl.Contains("search="))
                    //        {
                    //            try
                    //            {
                    //                var uri = new Uri(currentUrl);
                    //                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                    //                string searchParam = query["search"];
                    //                if (!string.IsNullOrEmpty(searchParam) && searchParam.Length > 15) // Lấy 15 ký tự đầu của search param
                    //                {
                    //                    queryStringPart = "_" + new string(searchParam.Substring(0, 15)
                    //                        .Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray()); // Chỉ giữ ký tự an toàn cho tên file
                    //                }
                    //            }
                    //            catch { } // Bỏ qua nếu không parse được URI/query
                    //        }


                    //        string logFileName = $"SummaryResponse_{typeQuery}_{endpointType}{queryStringPart}_{DateTime.Now:yyyyMMddHHmmssfff}.txt";

                    //        // Lấy thư mục gốc của project (thường là nơi file .csproj tọa lạc)
                    //        // Điều này hoạt động tốt khi chạy từ Visual Studio.
                    //        // Nếu chạy file .exe đã build, nó sẽ là thư mục chứa file .exe (ví dụ: bin\Debug)
                    //        string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    //        // Để chắc chắn hơn là thư mục gốc của source code khi debug:
                    //        // string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; // Đi lùi 3 cấp từ bin/Debug/netX
                    //        // string projectDirectoryToLog = Path.Combine(solutionDirectory, "LoggedApiResponses"); // Tạo thư mục con

                    //        // Ghi vào thư mục output của project (bin/Debug hoặc bin/Release)
                    //        string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    //        string logFolder = Path.Combine(outputDirectory, "ApiLog_Summaries");
                    //        if (!Directory.Exists(logFolder))
                    //        {
                    //            Directory.CreateDirectory(logFolder);
                    //        }
                    //        string filePath = Path.Combine(logFolder, logFileName);

                    //        File.WriteAllText(filePath, $"URL: {currentUrl}\n\nResponse Body:\n{responseContentForLog}");
                    //        Debug.WriteLine($"Đã ghi Summary API Response vào: {filePath}");
                    //    }
                    //    catch (Exception logEx)
                    //    {
                    //        Debug.WriteLine($"Lỗi khi ghi Summary API Response ra file: {logEx.Message}");
                    //    }
                    //}
                    //// --- KẾT THÚC CODE GHI FILE TXT ---

                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Lỗi API (Summaries) {response.StatusCode}: {currentUrl}");
                        break;
                    }

                    // SỬA LỖI: Sử dụng responseContentForLog thay vì đọc lại content
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true,
                        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString, // Allow số dạng string
                        UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode // Ignore unknown fields
                    };

                    var result = System.Text.Json.JsonSerializer.Deserialize<InvoiceQueryResult>(await response.Content.ReadAsStringAsync(), options);

                    if (result?.Datas != null && result.Datas.Any())
                    {
                        foreach (var invoiceData in result.Datas)
                        {
                            currentIndex++;

                            // SỬA LỖI: Luôn thêm vào invoiceIdentifiers (cần cho việc tải chi tiết và XML)
                            invoiceIdentifiers.Add(new InvoiceIdentifier
                            {
                                Nbmst = invoiceData.nbmst,
                                Khhdon = invoiceData.khhdon,
                                Shdon = invoiceData.shdon,
                                Khmshdon = invoiceData.khmshdon,
                                IsScoQuery = isScoQuery,
                                Tdlap = invoiceData.tdlap
                            });

                            // SỬA LỖI: Chỉ hiển thị lên giao diện nếu không phải chế độ silent
                            if (!silentMode)
                            {
                                var summary = new InvoiceSummary
                                {
                                    STT = currentIndex,
                                    id = invoiceData.id,
                                    TenHoaDon = invoiceData.tlhdon,
                                    MauHoaDon = invoiceData.khmshdon,
                                    KyHieu = invoiceData.khhdon,
                                    SoHD = invoiceData.shdon,
                                    NgayHD = Helper.FormatHelper.FormatInvoiceDate(invoiceData.tdlap),
                                    DVTienTe = invoiceData.dvtte,
                                    TyGia = invoiceData.tgia,
                                    TenNguoiBan = invoiceData.nbten,
                                    MSTNguoiBan = invoiceData.nbmst,
                                    DiaChiNguoiBan = invoiceData.nbdchi,
                                    TenNguoiMua = invoiceData.nmten,
                                    MSTNguoiMua = invoiceData.nmmst,
                                    DiaChiNguoiMua = invoiceData.nmdchi,
                                    TongTienChuaThue = invoiceData.tgtcthue,
                                    TongTienThue = invoiceData.tgtthue,
                                    TrangThai = Helper.FormatHelper.GetInvoiceStatusDescription(invoiceData.tthai?.ToString()),
                                    KetQuaKiemTraHoaDon = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(invoiceData.ttxly?.ToString()),
                                };

                                // SỬA LỖI: Debug để kiểm tra dữ liệu được tạo
                                Debug.WriteLine($"Tạo summary: STT={summary.STT}, SoHD={summary.SoHD}, TenNguoiBan={summary.TenNguoiBan}");

                                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                                    invoiceSummaryList.Add(summary);
                                    Debug.WriteLine($"Đã thêm vào invoiceSummaryList, tổng: {invoiceSummaryList.Count}");
                                });
                            }
                        }

                        // Xử lý phân trang
                        if (!string.IsNullOrEmpty(result.State))
                        {
                            Uri baseUri = new Uri(currentUrl.Contains("?") ? currentUrl.Substring(0, currentUrl.IndexOf('?')) : currentUrl);
                            var queryParams = System.Web.HttpUtility.ParseQueryString(new Uri(currentUrl).Query);
                            queryParams["state"] = result.State;
                            currentUrl = $"{baseUri}?{queryParams}";
                        }
                        else
                        {
                            currentUrl = string.Empty;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Không có dữ liệu hoặc dữ liệu rỗng từ URL: {currentUrl}");
                        currentUrl = string.Empty;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi GetInvoiceSummariesAsync từ {initialUrl}: {ex.Message}");
                Debug.WriteLine($"StackTrace: {ex.StackTrace}");
            }
        }
        private async Task GetInvoiceDetailsAsync(InvoiceIdentifier invoiceIdentity, bool isBackgroundMode = false)
        {
            int maxRetries = 5;
            int currentRetry = 0;
            int baseDelayMillisecondsForRetry = isBackgroundMode ? 4000 : 1000; // Thời gian chờ ban đầu cho retry, dài hơn nếu là background

            string baseUrl = invoiceIdentity.IsScoQuery ?
                "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/detail?" :
                "https://hoadondientu.gdt.gov.vn:30000/query/invoices/detail?";
            string url = $"{baseUrl}nbmst={invoiceIdentity.Nbmst}&khhdon={invoiceIdentity.Khhdon}&shdon={invoiceIdentity.Shdon}&khmshdon={invoiceIdentity.Khmshdon}";

            while (currentRetry <= maxRetries)
            {
                try
                {
                    HttpResponseMessage response = null;
                    if (currentRetry > 0)
                    {
                        Random jitter = new Random();
                        int delayMilliseconds = (int)(baseDelayMillisecondsForRetry * Math.Pow(2, currentRetry - 1)) + jitter.Next(0, 501);

                        string retryMessage = $"HĐ {invoiceIdentity.Shdon}: Tạm dừng do giới hạn API/lỗi mạng, thử lại sau {delayMilliseconds / 1000}s... (Lần {currentRetry}/{maxRetries})";
                        Debug.WriteLine(retryMessage);
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = retryMessage);
                        await Task.Delay(delayMilliseconds);
                        await System.Windows.Threading.Dispatcher.Yield();
                        System.Windows.Application.Current.Dispatcher.Invoke(() => lblStatus.Text = $"Đang tải lại chi tiết hóa đơn {invoiceIdentity.Shdon} (Lần {currentRetry})...");
                    }

                    response = await client.GetAsync(url);

                    // --- BẮT ĐẦU CODE GHI FILE TXT ---
                    string responseContentForLog = "ERROR_READING_CONTENT"; // Giá trị mặc định nếu không đọc được
                    if (response != null && response.Content != null)
                    {
                        try
                        {
                            responseContentForLog = await response.Content.ReadAsStringAsync(); // Đọc content một lần

                            
                            string logFileName = $"DetailResponse_{DateTime.Now:yyyyMMddHHmmssfff}.txt";

                            // Lấy thư mục gốc của project (thường là nơi file .csproj tọa lạc)
                            // Điều này hoạt động tốt khi chạy từ Visual Studio.
                            // Nếu chạy file .exe đã build, nó sẽ là thư mục chứa file .exe (ví dụ: bin\Debug)
                            string projectDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            // Để chắc chắn hơn là thư mục gốc của source code khi debug:
                            // string solutionDirectory = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName; // Đi lùi 3 cấp từ bin/Debug/netX
                            // string projectDirectoryToLog = Path.Combine(solutionDirectory, "LoggedApiResponses"); // Tạo thư mục con

                            // Ghi vào thư mục output của project (bin/Debug hoặc bin/Release)
                            string outputDirectory = AppDomain.CurrentDomain.BaseDirectory;
                            string logFolder = Path.Combine(outputDirectory, "ApiLog_Summaries");
                            if (!Directory.Exists(logFolder))
                            {
                                Directory.CreateDirectory(logFolder);
                            }
                            string filePath = Path.Combine(logFolder, logFileName);

                            File.WriteAllText(filePath, $"Response Body:\n{responseContentForLog}");
                            Debug.WriteLine($"Đã ghi Detail API Response vào: {filePath}");
                        }
                        catch (Exception logEx)
                        {
                            Debug.WriteLine($"Lỗi khi ghi Summary API Response ra file: {logEx.Message}");
                        }
                    }
                    // --- KẾT THÚC CODE GHI FILE TXT ---

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var apiResponseData = System.Text.Json.JsonSerializer.Deserialize<InvoiceDetailApiResponse>(content);

                        if (apiResponseData != null)
                        {
                            string hinhThucTT = !string.IsNullOrEmpty(apiResponseData.ApiHinhThucThanhToanCode) ?
                                                apiResponseData.ApiHinhThucThanhToanCode :
                                                apiResponseData.ApiHinhThucThanhToanText;

                            if (apiResponseData.Hdhhdvu != null && apiResponseData.Hdhhdvu.Any())
                            {
                                foreach (var rawItem in apiResponseData.Hdhhdvu)
                                {
                                    var displayItem = new InvoiceDisplayItem
                                    {
                                        LoaiHoaDon = apiResponseData.ApiLoaiHoaDon,
                                        MauSoHoaDon = apiResponseData.ApiMauSoHoaDon,
                                        KyHieuHoaDon = apiResponseData.ApiKyHieuHoaDon,
                                        SoHoaDon = apiResponseData.ApiSoHoaDon,
                                        NgayLapHoaDon = Helper.FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayLapHoaDonISO),
                                        NgayKy = Helper.FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayKyISO),
                                        MaCQT = apiResponseData.ApiMaCQT,
                                        DonViTienTe = apiResponseData.ApiDonViTienTe,
                                        TyGia = apiResponseData.ApiTyGia,
                                        TenNguoiBan = apiResponseData.ApiTenNguoiBan,
                                        MaSoThueNguoiBan = apiResponseData.ApiMaSoThueNguoiBan,
                                        DiaChiNguoiBan = apiResponseData.ApiDiaChiNguoiBan,
                                        TenNguoiMua = apiResponseData.ApiTenNguoiMua,
                                        MaSoThueNguoiMua = apiResponseData.ApiMaSoThueNguoiMua,
                                        DiaChiNguoiMua = apiResponseData.ApiDiaChiNguoiMua,
                                        HinhThucThanhToan = hinhThucTT,
                                        SoThuTuDong = rawItem.SoThuTuDongRaw,
                                        TenHHDV = rawItem.TenHHDVRaw,
                                        DonViTinh = rawItem.DonViTinhRaw,
                                        SoLuong = rawItem.SoLuongRaw,
                                        DonGia = rawItem.DonGiaRaw,
                                        SoTienChietKhau = rawItem.SoTienChietKhauRaw,
                                        LoaiThueSuat = rawItem.LoaiThueSuatRaw,
                                        ThanhTienChuaThue = rawItem.ThanhTienChuaThueRaw,
                                        TienThue = rawItem.TienThueRaw,
                                        
                                        
                                        TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(apiResponseData.ApiTrangThaiHD_Code),
                                        TinhTrangXuLy_HD = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(apiResponseData.ApiTinhTrangXuLy_Code)
                                    };

                                    if (displayItem.SoThuTuDong == 1)
                                    {
                                        displayItem.TongTienChuaThue_HD = apiResponseData.ApiTongTienChuaThue;
                                        displayItem.TongTienThue_HD = apiResponseData.ApiTongTienThue;
                                        displayItem.TongTienChietKhauTM_HD = apiResponseData.ApiTongTienChietKhauTM;
                                        displayItem.TongTienThanhToan_HD = apiResponseData.ApiTongTienThanhToan;
                                        displayItem.TongTienThanhToanBangChu_HD = apiResponseData.ApiTongTienThanhToanBangChu;
                                    };
                                    System.Windows.Application.Current.Dispatcher.Invoke(() => invoiceDetailList.Add(displayItem));
                                }
                            }
                            else
                            {
                                var displayItemMsg = new InvoiceDisplayItem
                                {
                                    LoaiHoaDon = apiResponseData.ApiLoaiHoaDon,
                                    MauSoHoaDon = apiResponseData.ApiMauSoHoaDon ?? invoiceIdentity.Khmshdon,
                                    KyHieuHoaDon = apiResponseData.ApiKyHieuHoaDon ?? invoiceIdentity.Khhdon,
                                    SoHoaDon = apiResponseData.ApiSoHoaDon ?? invoiceIdentity.Shdon,
                                    NgayLapHoaDon = Helper.FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayLapHoaDonISO ?? invoiceIdentity.Tdlap),
                                    TenNguoiBan = apiResponseData.ApiTenNguoiBan,
                                    TenNguoiMua = apiResponseData.ApiTenNguoiMua,
                                    TenHHDV = "Không có dữ liệu chi tiết hoặc hóa đơn không có dòng hàng hóa.",
                                    TongTienThanhToan_HD = apiResponseData.ApiTongTienThanhToan,
                                    TrangThai_HD = Helper.FormatHelper.GetInvoiceStatusDescription(apiResponseData.ApiTrangThaiHD_Code),
                                    TinhTrangXuLy_HD = Helper.FormatHelper.GetInvoiceProcessingStatusDescription(apiResponseData.ApiTinhTrangXuLy_Code)
                                };
                                System.Windows.Application.Current.Dispatcher.Invoke(() => invoiceDetailList.Add(displayItemMsg));
                            }
                        }
                        else
                        {
                            HandleDetailError($"Lỗi phân tích dữ liệu chi tiết từ API (nội dung: '{(string.IsNullOrEmpty(content) ? "rỗng" : "không hợp lệ")}').", invoiceIdentity, string.IsNullOrEmpty(content));
                        }
                        return;
                    }
                    else if ((int)response.StatusCode == 429)
                    {
                        currentRetry++;
                        if (currentRetry > maxRetries)
                        {
                            Debug.WriteLine($"Đã đạt số lần thử lại tối đa cho HĐ {invoiceIdentity.Shdon} với lỗi 429.");
                            HandleDetailError($"Lỗi 429: Gửi quá nhiều yêu cầu. Đã thử lại {maxRetries} lần.", invoiceIdentity);
                            return;
                        }
                    }
                    else
                    {
                        Debug.WriteLine($"Lỗi API (Details) {response.StatusCode}: {url}");
                        HandleDetailError($"Lỗi API ({response.StatusCode}) khi lấy chi tiết.", invoiceIdentity);
                        return;
                    }
                }
                catch (HttpRequestException httpEx)
                {
                    Debug.WriteLine($"Lỗi HttpRequestException khi lấy chi tiết HĐ {invoiceIdentity.Shdon}: {httpEx.Message}");
                    currentRetry++;
                    if (currentRetry > maxRetries)
                    {
                        HandleDetailError($"Lỗi mạng khi lấy chi tiết. Đã thử lại {maxRetries} lần.", invoiceIdentity);
                        return;
                    }
                }
                catch (JsonException jsonEx)
                {
                    Debug.WriteLine($"Lỗi Deserialize JSON chi tiết HĐ {invoiceIdentity.Shdon}: {jsonEx.Message}");
                    HandleDetailError("Lỗi đọc dữ liệu chi tiết từ API.", invoiceIdentity);
                    return;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi không xác định khi lấy chi tiết HĐ {invoiceIdentity.Shdon}: {ex.Message}");
                    HandleDetailError("Lỗi hệ thống khi lấy chi tiết.", invoiceIdentity);
                    return;
                }
            }

            if (currentRetry > maxRetries)
            {
                Debug.WriteLine($"Không thể lấy chi tiết cho HĐ {invoiceIdentity.Shdon} sau {maxRetries} lần thử.");
            }
        }


        private void HandleDetailError(string baseErrorMessage, InvoiceIdentifier invoiceIdentity, bool isEmptyContent = false)
        {
            string displayMessage;
            if (isEmptyContent)
            {
                // Thông báo cụ thể hơn cho trường hợp nội dung API rỗng
                displayMessage = $"Không có dữ liệu chi tiết cho HĐ: {invoiceIdentity.Nbmst} / {invoiceIdentity.Khhdon} / {invoiceIdentity.Shdon}. (Nội dung API rỗng)";
            }
            else
            {
                displayMessage = $"{baseErrorMessage} (HĐ: {invoiceIdentity.Nbmst} / {invoiceIdentity.Khhdon} / {invoiceIdentity.Shdon})";
            }

            var errorDisplayItem = new InvoiceDisplayItem
            {
                // Điền các thông tin định danh cơ bản của hóa đơn để người dùng biết hóa đơn nào bị lỗi
                MauSoHoaDon = invoiceIdentity.Khmshdon,
                KyHieuHoaDon = invoiceIdentity.Khhdon,
                SoHoaDon = invoiceIdentity.Shdon,
                NgayLapHoaDon = Helper.FormatHelper.FormatInvoiceDate(invoiceIdentity.Tdlap), // Lấy ngày lập từ identity
                MaSoThueNguoiBan = invoiceIdentity.Nbmst,
                // Có thể thêm Tên Người Bán/Mua nếu bạn lưu chúng trong InvoiceIdentifier hoặc có thể truy vấn nhanh
                // Tuy nhiên, để đơn giản, chỉ hiển thị các thông tin có sẵn ngay từ InvoiceIdentifier

                // Trường TenHHDV sẽ chứa thông báo lỗi/trạng thái
                TenHHDV = displayMessage,

                // Các trường số liệu khác để là null hoặc giá trị mặc định (ví dụ 0)
                SoLuong = null,
                DonGia = null,
                ThanhTienChuaThue = null,
                TienThue = null,
                // ... và các trường khác của InvoiceDisplayItem
                // Bạn có thể quyết định điền "N/A" hoặc để trống cho các trường này
                DonViTinh = "Lỗi",
                LoaiThueSuat = "N/A"
            };

            // Thêm dòng lỗi này vào danh sách chi tiết để hiển thị trên DataGrid
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                invoiceDetailList.Add(errorDisplayItem);
            });

            // Ghi log chi tiết hơn vào Debug Output
            Debug.WriteLine($"HandleDetailError: {displayMessage} | MST: {invoiceIdentity.Nbmst}, KHHDon: {invoiceIdentity.Khhdon}, SHDon: {invoiceIdentity.Shdon}");
        }
        private async Task DownloadInvoiceXMLAsync(InvoiceIdentifier invoice, string baseZipSavePath, string specificExtractFolder)
        {
            try
            {
                string baseUrl = invoice.IsScoQuery ?
                    "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-xml?" :
                    "https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-xml?";
                string url = $"{baseUrl}nbmst={invoice.Nbmst}&khhdon={invoice.Khhdon}&shdon={invoice.Shdon}&khmshdon={invoice.Khmshdon}";

                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.Headers.Add("Accept-Encoding", "gzip");

                var response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string zipFileName = $"{invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}.zip";
                    string zipFilePath = Path.Combine(baseZipSavePath, zipFileName);
                    using (var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    ExtractZipFile(zipFilePath, specificExtractFolder);
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError) { Debug.WriteLine($"Không tồn tại hồ sơ gốc của hóa đơn (500): {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}"); }
                else if ((int)response.StatusCode == 429) { System.Windows.MessageBox.Show("Lỗi 429 - Gửi quá nhiều yêu cầu tới máy chủ Web.", "Lỗi tải XML", MessageBoxButton.OK, MessageBoxImage.Warning); }
                else { Debug.WriteLine($"Lỗi khi tải XML/HTML: {response.StatusCode} - {response.ReasonPhrase} cho hóa đơn {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}"); }
            }
            catch (Exception ex) { Debug.WriteLine($"Lỗi khi tải XML/HTML cho HĐ {invoice.Shdon}: {ex.Message}"); }
        }
        private void ExtractZipFile(string zipFilePath, string extractFolder)
        {
            try
            {
                if (!Directory.Exists(extractFolder)) Directory.CreateDirectory(extractFolder);
                ZipFile.ExtractToDirectory(zipFilePath, extractFolder, true);
            }
            catch (IOException ioEx) when (ioEx.Message.Contains("already exists")) { Debug.WriteLine($"Lỗi giải nén (file đã tồn tại và có thể đang được sử dụng): {ioEx.Message} cho {zipFilePath}"); }
            catch (Exception ex) { Debug.WriteLine($"Lỗi giải nén file {zipFilePath}: {ex.Message}"); }
        }
        private void UpdateXMLFileList(string rootFolderPath)
        {
            try
            {
                var rootDir = new DirectoryInfo(rootFolderPath);
                if (!rootDir.Exists) { Debug.WriteLine($"Thư mục gốc không tồn tại để liệt kê file: {rootFolderPath}"); return; }

                var files = rootDir.GetFiles("*.*", SearchOption.AllDirectories)
                                .Where(f => f.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                                            f.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                            f.Extension.Equals(".html", StringComparison.OrdinalIgnoreCase));
                System.Windows.Application.Current.Dispatcher.Invoke(() => {
                    xmlFileList.Clear();
                    foreach (var file in files) xmlFileList.Add(file);
                });
            }
            catch (Exception ex) { Debug.WriteLine($"Lỗi khi cập nhật danh sách file từ {rootFolderPath}: {ex.Message}"); }
        }
        #endregion

        #region Export Functions
        private enum ActiveTabData { TongHop, ChiTiet, XmlFiles, Unknown }

        private ActiveTabData GetCurrentActiveTabData()
        {
            if (MainTabControl.SelectedItem == tabTongHop) return ActiveTabData.TongHop;
            if (MainTabControl.SelectedItem == tabChiTiet) return ActiveTabData.ChiTiet;
            if (MainTabControl.SelectedItem == tabXmlHtml) return ActiveTabData.XmlFiles;
            return ActiveTabData.Unknown;
        }

        private void btnOpenExportMenu_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button btn = sender as System.Windows.Controls.Button;
            if (btn?.ContextMenu != null)
            {
                // Kiểm tra xem có dữ liệu để xuất không, nếu không thì vô hiệu hóa menu items
                ActiveTabData currentTab = GetCurrentActiveTabData();
                bool canExportTongHop = currentTab == ActiveTabData.TongHop && invoiceSummaryList.Any();
                bool canExportChiTiet = currentTab == ActiveTabData.ChiTiet && invoiceDetailList.Any();
                bool canExportXmlList = currentTab == ActiveTabData.XmlFiles && xmlFileList.Any();

                foreach (var item in btn.ContextMenu.Items.OfType<MenuItem>())
                {
                    if (item.Header.ToString().Contains("CSV") || item.Header.ToString().Contains("Excel"))
                    {
                        if (currentTab == ActiveTabData.TongHop) item.IsEnabled = canExportTongHop;
                        else if (currentTab == ActiveTabData.ChiTiet) item.IsEnabled = canExportChiTiet;
                        else if (currentTab == ActiveTabData.XmlFiles) item.IsEnabled = canExportXmlList;
                        else item.IsEnabled = false;
                    }
                }

                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.IsOpen = true;
            }
        }

        // --- CSV Export ---
        private void mnuExportCSV_Click(object sender, RoutedEventArgs e)
        {
            ActiveTabData activeTab = GetCurrentActiveTabData();
            switch (activeTab)
            {
                case ActiveTabData.TongHop: ExportTongHopToCSV(); break;
                case ActiveTabData.ChiTiet: ExportChiTietToCSV(); break;
                case ActiveTabData.XmlFiles: ExportXmlFileListToCSV(); break;
                default: System.Windows.MessageBox.Show("Không xác định được tab dữ liệu để xuất.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning); break;
            }
        }

        private void ExportTongHopToCSV()
        {
            if (!invoiceSummaryList.Any())
            {
                System.Windows.MessageBox.Show("Không có dữ liệu tổng hợp.", "Thông báo");
                return;
            }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV file (*.csv)|*.csv",
                FileName = $"TongHopHD_{DateTime.Now:yyyyMMddHHmmss}.csv"
            };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("STT,Ngày HĐ,MST,Tên đơn vị,Ký hiệu,Số HĐ,Trạng thái,Tổng tiền");
                        foreach (var item in invoiceSummaryList)
                        {
                            writer.WriteLine(string.Join(",",
                                item.STT,
                                EscapeCsvField(item.NgayHD),
                                EscapeCsvField(item.MSTNguoiBan),
                                EscapeCsvField(item.TenNguoiBan),
                                EscapeCsvField(item.KyHieu),
                                EscapeCsvField(item.SoHD),
                                EscapeCsvField(item.TrangThai),
                                EscapeCsvField(item.TongTienChuaThue?.ToString(CultureInfo.InvariantCulture)) // Convert decimal to string using CultureInfo.InvariantCulture  
                            ));
                        }
                    }
                    System.Windows.MessageBox.Show($"Xuất CSV Tổng Hợp thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show($"Lỗi xuất CSV Tổng Hợp: {ex.Message}", "Lỗi");
                }
            }
        }

        private void ExportChiTietToCSV()
        {
            if (!invoiceDetailList.Any()) { System.Windows.MessageBox.Show("Không có dữ liệu chi tiết.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"ChiTietHD_{DateTime.Now:yyyyMMddHHmmss}.csv" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Mẫu Số HĐ,Ký Hiệu HĐ,Số HĐ,Ngày Lập,MST Bán,Tên Người Bán,Địa Chỉ Bán,MST Mua,Tên Người Mua,Địa Chỉ Mua,STT Dòng,Mã HH,Tên HHDV,ĐVT,Số Lượng,Đơn Giá,Tiền CK,Thành Tiền,Loại TS,Tiền Thuế,Ngày Ký,Mã CQT HĐ,ĐV Tiền Tệ,Tỷ Giá,HT Thanh Toán,Tổng Tiền Hàng (HĐ),Tổng Tiền Thuế (HĐ),Tổng CK TM (HĐ),Tổng Thanh Toán (HĐ),Tiền Bằng Chữ (HĐ),Trạng Thái HĐ,Xử Lý HĐ");
                        foreach (var item in invoiceDetailList)
                        {
                            writer.WriteLine(string.Join(",",
                               EscapeCsvField(item.MauSoHoaDon), EscapeCsvField(item.KyHieuHoaDon), EscapeCsvField(item.SoHoaDon), EscapeCsvField(item.NgayLapHoaDon),
                               EscapeCsvField(item.MaSoThueNguoiBan), EscapeCsvField(item.TenNguoiBan), EscapeCsvField(item.DiaChiNguoiBan),
                               EscapeCsvField(item.MaSoThueNguoiMua), EscapeCsvField(item.TenNguoiMua), EscapeCsvField(item.DiaChiNguoiMua),
                               item.SoThuTuDong?.ToString() ?? "", EscapeCsvField(item.MaHHDV), EscapeCsvField(item.TenHHDV), EscapeCsvField(item.DonViTinh),
                               item.SoLuong?.ToString(CultureInfo.InvariantCulture) ?? "", item.DonGia?.ToString(CultureInfo.InvariantCulture) ?? "",
                               item.SoTienChietKhau?.ToString(CultureInfo.InvariantCulture) ?? "", item.ThanhTienChuaThue?.ToString(CultureInfo.InvariantCulture) ?? "",
                               EscapeCsvField(item.LoaiThueSuat), item.TienThue?.ToString(CultureInfo.InvariantCulture) ?? "",
                               EscapeCsvField(item.NgayKy), EscapeCsvField(item.MaCQT), EscapeCsvField(item.DonViTienTe),
                               item.TyGia?.ToString(CultureInfo.InvariantCulture) ?? "", EscapeCsvField(item.HinhThucThanhToan),
                               item.TongTienChuaThue_HD?.ToString(CultureInfo.InvariantCulture) ?? "", item.TongTienThue_HD?.ToString(CultureInfo.InvariantCulture) ?? "",
                               item.TongTienChietKhauTM_HD?.ToString(CultureInfo.InvariantCulture) ?? "", item.TongTienThanhToan_HD?.ToString(CultureInfo.InvariantCulture) ?? "",
                               EscapeCsvField(item.TongTienThanhToanBangChu_HD), EscapeCsvField(item.TrangThai_HD), EscapeCsvField(item.TinhTrangXuLy_HD)
                           ));
                        }
                    }
                    System.Windows.MessageBox.Show($"Xuất CSV Chi Tiết thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất CSV Chi Tiết: {ex.Message}", "Lỗi"); }
            }
        }

        private void ExportXmlFileListToCSV()
        {
            if (!xmlFileList.Any()) { System.Windows.MessageBox.Show("Không có danh sách file XML/HTML.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = $"DS_Files_{DateTime.Now:yyyyMMddHHmmss}.csv" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (var writer = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        writer.WriteLine("Tên file,Kích thước (Bytes),Ngày tạo,Đường dẫn");
                        foreach (var fileInfo in xmlFileList)
                        {
                            writer.WriteLine(string.Join(",",
                                EscapeCsvField(fileInfo.Name), fileInfo.Length,
                                EscapeCsvField(fileInfo.CreationTime.ToString("dd/MM/yyyy HH:mm:ss")),
                                EscapeCsvField(fileInfo.DirectoryName)
                            ));
                        }
                    }
                    System.Windows.MessageBox.Show($"Xuất CSV Danh sách file thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất CSV Danh sách file: {ex.Message}", "Lỗi"); }
            }
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n") || field.Contains("\r"))
            {
                return $"\"{field.Replace("\"", "\"\"")}\"";
            }
            return field;
        }

        // --- Excel Export ---
        private void mnuExportExcel_Click(object sender, RoutedEventArgs e)
        {
            ActiveTabData activeTab = GetCurrentActiveTabData();
            switch (activeTab)
            {
                case ActiveTabData.TongHop: ExportTongHopToExcel(); break;
                case ActiveTabData.ChiTiet: ExportChiTietToExcel(); break;
                case ActiveTabData.XmlFiles: ExportXmlFileListToExcel(); break;
                default: System.Windows.MessageBox.Show("Không xác định được tab dữ liệu để xuất.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning); break;
            }
        }

        private void ExportTongHopToExcel()
        {
            if (!invoiceSummaryList.Any()) { System.Windows.MessageBox.Show("Không có dữ liệu tổng hợp.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"TongHopHD_{DateTime.Now:yyyyMMddHHmmss}.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // For EPPlus 5+
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("TongHop");
                        string[] headers = { "STT", "Ngày HĐ", "MST", "Tên đơn vị", "Ký hiệu", "Số HĐ", "Trạng thái", "Tổng tiền" };
                        for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; StyleHeaderCell(ws.Cells[1, i + 1]); }

                        int row = 2;
                        foreach (var item in invoiceSummaryList)
                        {
                            //ws.Cells[row, 1].Value = item.STT;
                            //ws.Cells[row, 2].Value = item.NgayHD;
                            //ws.Cells[row, 3].Value = item.MST;
                            //ws.Cells[row, 4].Value = item.TenDV;
                            //ws.Cells[row, 5].Value = item.KyHieu;
                            //ws.Cells[row, 6].Value = item.SoHD;
                            //ws.Cells[row, 7].Value = item.TrangThai;
                            //if (decimal.TryParse(item.TongTien?.Replace(".", "").Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal val))
                            //{ ws.Cells[row, 8].Value = val; }
                            //else { ws.Cells[row, 8].Value = item.TongTien; }
                            //ws.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                            //row++;
                        }
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        package.SaveAs(new FileInfo(sfd.FileName));
                    }
                    System.Windows.MessageBox.Show($"Xuất Excel Tổng Hợp thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất Excel Tổng Hợp: {ex.Message}", "Lỗi"); }
            }
        }
        private void ExportChiTietToExcel()
        {
            if (!invoiceDetailList.Any()) { System.Windows.MessageBox.Show("Không có dữ liệu chi tiết.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"ChiTietHD_{DateTime.Now:yyyyMMddHHmmss}.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // For EPPlus 5+
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("ChiTietHoaDon");
                        string[] headers = {
                            "Loại HĐ", "Mẫu Số HĐ", "Ký Hiệu HĐ", "Số HĐ", "Ngày Lập", "Ngày Ký", "Mã CQT HĐ", "MST Bán", "Tên Người Bán", "Địa Chỉ Bán", "MST Mua", "Tên Người Mua", "Địa Chỉ Mua",
                            "STT Dòng", "Mã HH", "Tên HHDV", "ĐVT", "Số Lượng", "Đơn Giá", "Tiền CK", "Thành Tiền", "Loại TS", "Tiền Thuế",
                            "ĐV Tiền Tệ", "Tỷ Giá", "HT Thanh Toán", "Tổng Tiền Hàng (HĐ)", "Tổng Tiền Thuế (HĐ)",
                            "Tổng CK TM (HĐ)", "Tổng Thanh Toán (HĐ)", "Tiền Bằng Chữ (HĐ)", "Trạng Thái HĐ", "Xử Lý HĐ"
                        };
                        for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; StyleHeaderCell(ws.Cells[1, i + 1]); }

                        int rowIdx = 2;
                        foreach (var item in invoiceDetailList)
                        {
                            int colIdx = 1;
                            ws.Cells[rowIdx, colIdx++].Value = item.LoaiHoaDon;
                            ws.Cells[rowIdx, colIdx++].Value = item.MauSoHoaDon; 
                            ws.Cells[rowIdx, colIdx++].Value = item.KyHieuHoaDon; 
                            ws.Cells[rowIdx, colIdx++].Value = item.SoHoaDon;
                            ws.Cells[rowIdx, colIdx++].Value = item.NgayLapHoaDon;
                            ws.Cells[rowIdx, colIdx++].Value = item.NgayKy;
                            ws.Cells[rowIdx, colIdx++].Value = item.MaCQT;
                            ws.Cells[rowIdx, colIdx++].Value = item.MaSoThueNguoiBan; 
                            ws.Cells[rowIdx, colIdx++].Value = item.TenNguoiBan;
                            ws.Cells[rowIdx, colIdx++].Value = item.DiaChiNguoiBan; 
                            ws.Cells[rowIdx, colIdx++].Value = item.MaSoThueNguoiMua; 
                            ws.Cells[rowIdx, colIdx++].Value = item.TenNguoiMua;
                            ws.Cells[rowIdx, colIdx++].Value = item.DiaChiNguoiMua; 
                            ws.Cells[rowIdx, colIdx++].Value = item.SoThuTuDong;
                            ws.Cells[rowIdx, colIdx++].Value = item.MaHHDV; 
                            ws.Cells[rowIdx, colIdx++].Value = item.TenHHDV; 
                            ws.Cells[rowIdx, colIdx++].Value = item.DonViTinh;
                            ws.Cells[rowIdx, colIdx].Value = item.SoLuong; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0.####"; // Cho phép nhiều số lẻ hơn cho số lượng
                            ws.Cells[rowIdx, colIdx].Value = item.DonGia; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0.####";
                            ws.Cells[rowIdx, colIdx].Value = item.SoTienChietKhau; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx].Value = item.ThanhTienChuaThue; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx++].Value = item.LoaiThueSuat;
                            ws.Cells[rowIdx, colIdx].Value = item.TienThue; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                             
                            ws.Cells[rowIdx, colIdx++].Value = item.DonViTienTe;
                            ws.Cells[rowIdx, colIdx].Value = item.TyGia; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0.##";
                            ws.Cells[rowIdx, colIdx++].Value = item.HinhThucThanhToan;
                            ws.Cells[rowIdx, colIdx].Value = item.TongTienChuaThue_HD; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx].Value = item.TongTienThue_HD; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx].Value = item.TongTienChietKhauTM_HD; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx].Value = item.TongTienThanhToan_HD; 
                            ws.Cells[rowIdx, colIdx++].Style.Numberformat.Format = "#,##0";
                            ws.Cells[rowIdx, colIdx++].Value = item.TongTienThanhToanBangChu_HD;
                            ws.Cells[rowIdx, colIdx++].Value = item.TrangThai_HD; 
                            ws.Cells[rowIdx, colIdx++].Value = item.TinhTrangXuLy_HD;
                            rowIdx++;
                        }
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        package.SaveAs(new FileInfo(sfd.FileName));
                    }
                    System.Windows.MessageBox.Show($"Xuất Excel Chi Tiết thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất Excel Chi Tiết: {ex.Message}", "Lỗi"); }
            }
        }
        private void ExportXmlFileListToExcel()
        {
            if (!xmlFileList.Any()) { System.Windows.MessageBox.Show("Không có danh sách file XML/HTML.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "Excel Workbook (*.xlsx)|*.xlsx", FileName = $"DS_Files_{DateTime.Now:yyyyMMddHHmmss}.xlsx" };
            if (sfd.ShowDialog() == true)
            {
                // ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // For EPPlus 5+
                try
                {
                    using (var package = new ExcelPackage())
                    {
                        var ws = package.Workbook.Worksheets.Add("DanhSachFiles");
                        string[] headers = { "Tên file", "Kích thước (Bytes)", "Ngày tạo", "Đường dẫn" };
                        for (int i = 0; i < headers.Length; i++) { ws.Cells[1, i + 1].Value = headers[i]; StyleHeaderCell(ws.Cells[1, i + 1]); }
                        int row = 2;
                        foreach (var fileInfo in xmlFileList)
                        {
                            ws.Cells[row, 1].Value = fileInfo.Name;
                            ws.Cells[row, 2].Value = fileInfo.Length; ws.Cells[row, 2].Style.Numberformat.Format = "#,##0";
                            ws.Cells[row, 3].Value = fileInfo.CreationTime; ws.Cells[row, 3].Style.Numberformat.Format = "dd/MM/yyyy HH:mm:ss";
                            ws.Cells[row, 4].Value = fileInfo.DirectoryName;
                            row++;
                        }
                        ws.Cells[ws.Dimension.Address].AutoFitColumns();
                        package.SaveAs(new FileInfo(sfd.FileName));
                    }
                    System.Windows.MessageBox.Show($"Xuất Excel Danh sách file thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất Excel Danh sách file: {ex.Message}", "Lỗi"); }
            }
        }
        private void StyleHeaderCell(ExcelRange cell)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Fill.PatternType = ExcelFillStyle.Solid;
            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightSteelBlue); // Màu nhạt hơn
            cell.Style.Border.BorderAround(ExcelBorderStyle.Thin);
            cell.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
        }

        // --- (Tùy chọn) XML Export ---
        // private void mnuExportCustomXML_Click(object sender, RoutedEventArgs e) { /* Gọi hàm xuất XML tương ứng */ }
        // Các hàm ExportTongHopToCustomXML, ExportChiTietToCustomXML, ExportXmlFileListToCustomXML tương tự như trên.
        // Ví dụ cho ExportChiTietToCustomXML:
        private void ExportChiTietToCustomXML()
        {
            if (!invoiceDetailList.Any()) { System.Windows.MessageBox.Show("Không có dữ liệu chi tiết.", "Thông báo"); return; }
            Microsoft.Win32.SaveFileDialog sfd = new Microsoft.Win32.SaveFileDialog { Filter = "XML file (*.xml)|*.xml", FileName = $"ChiTietHD_{DateTime.Now:yyyyMMddHHmmss}.xml" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    XElement rootElement = new XElement("DanhSachHoaDonChiTiet");
                    var groupedByInvoice = invoiceDetailList
                       .GroupBy(item => new { item.MauSoHoaDon, item.KyHieuHoaDon, item.SoHoaDon }) // Group theo thông tin định danh HĐ
                       .Select(g => new { InvoiceKey = g.Key, InvoiceHeader = g.First(), Items = g.ToList() });

                    foreach (var group in groupedByInvoice)
                    {
                        var header = group.InvoiceHeader;
                        XElement hoaDonElement = new XElement("HoaDon",
                            new XElement("ThongTinChung",
                                new XElement("MauSo", header.MauSoHoaDon), new XElement("KyHieu", header.KyHieuHoaDon), new XElement("So", header.SoHoaDon),
                                new XElement("NgayLap", header.NgayLapHoaDon), new XElement("NgayKy", header.NgayKy), new XElement("MaCQT", header.MaCQT),
                                new XElement("TienTe", header.DonViTienTe), new XElement("TyGia", header.TyGia?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("HTTT", header.HinhThucThanhToan),
                                new XElement("TongTienHang", header.TongTienChuaThue_HD?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("TongTienThue", header.TongTienThue_HD?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("TongCKTM", header.TongTienChietKhauTM_HD?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("TongThanhToan", header.TongTienThanhToan_HD?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("BangChu", header.TongTienThanhToanBangChu_HD),
                                new XElement("TrangThai", header.TrangThai_HD), new XElement("XuLy", header.TinhTrangXuLy_HD)
                            ),
                            new XElement("NguoiBan", new XElement("Ten", header.TenNguoiBan), new XElement("MST", header.MaSoThueNguoiBan), new XElement("DiaChi", header.DiaChiNguoiBan)),
                            new XElement("NguoiMua", new XElement("Ten", header.TenNguoiMua), new XElement("MST", header.MaSoThueNguoiMua), new XElement("DiaChi", header.DiaChiNguoiMua)),
                            new XElement("DanhSachHHDV")
                        );
                        foreach (var item in group.Items)
                        {
                            hoaDonElement.Element("DanhSachHHDV").Add(new XElement("HHDV",
                                new XElement("STT", item.SoThuTuDong), new XElement("Ma", item.MaHHDV), new XElement("Ten", item.TenHHDV),
                                new XElement("DVT", item.DonViTinh), new XElement("SL", item.SoLuong?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("DG", item.DonGia?.ToString(CultureInfo.InvariantCulture)), new XElement("TienCK", item.SoTienChietKhau?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("ThanhTien", item.ThanhTienChuaThue?.ToString(CultureInfo.InvariantCulture)),
                                new XElement("LoaiTS", item.LoaiThueSuat), new XElement("TienThue", item.TienThue?.ToString(CultureInfo.InvariantCulture))
                            ));
                        }
                        rootElement.Add(hoaDonElement);
                    }
                    XDocument xdoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), rootElement);
                    xdoc.Save(sfd.FileName);
                    System.Windows.MessageBox.Show($"Xuất XML Chi Tiết thành công: {sfd.FileName}", "Hoàn tất");
                }
                catch (Exception ex) { System.Windows.MessageBox.Show($"Lỗi xuất XML Chi Tiết: {ex.Message}", "Lỗi"); }
            }
        }


        #endregion
    }
}
// --- END OF FILE InvoiceWindow.xaml.cs ---