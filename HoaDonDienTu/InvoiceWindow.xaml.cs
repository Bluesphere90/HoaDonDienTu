using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.IO.Compression;
// using System.Windows.Forms; // Note: Consider if this is truly needed or if System.Windows.Forms.FolderBrowserDialog can be replaced with a WPF alternative.
using HoaDonDienTu.Models;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace HoaDonDienTu
{
    public partial class InvoiceWindow : Window
    {
        private HttpClient client;
        private bool isMuaVao = true;
        private ObservableCollection<InvoiceSummary> invoiceSummaryList = new ObservableCollection<InvoiceSummary>();
        private ObservableCollection<InvoiceDetail> invoiceDetailList = new ObservableCollection<InvoiceDetail>();
        private ObservableCollection<FileInfo> xmlFileList = new ObservableCollection<FileInfo>();

        private List<KeyValuePair<string, string>> invoiceStatusList = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> checkResultList = new List<KeyValuePair<string, string>>();

        public InvoiceWindow()
        {
            InitializeComponent();

            // Cấu hình HttpClient
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AuthToken);

            // Thiết lập DataContext cho DataGrid
            dgTongHop.ItemsSource = invoiceSummaryList;
            dgChiTiet.ItemsSource = invoiceDetailList;
            lvXMLFiles.ItemsSource = xmlFileList;

            // Khởi tạo giá trị mặc định
            dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dpDenNgay.SelectedDate = DateTime.Now;

            // Thư mục lưu XML mặc định
            txtXMLFolderPath.Text = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HoaDonDienTu");

            // Nạp dữ liệu danh sách trạng thái và kết quả kiểm tra
            LoadStatusAndCheckResultLists();

            // Cập nhật giao diện
            UpdateUIState();
        }

        private void LoadStatusAndCheckResultLists()
        {
            // Trạng thái hóa đơn
            invoiceStatusList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("All", "Tất cả"),
                new KeyValuePair<string, string>("0", "Chưa duyệt"),
                new KeyValuePair<string, string>("1", "Đã duyệt"),
                new KeyValuePair<string, string>("2", "Đã hủy"),
                new KeyValuePair<string, string>("3", "Đang duyệt"),
                new KeyValuePair<string, string>("4", "Từ chối duyệt"),
                new KeyValuePair<string, string>("5", "Chờ hủy")
            };

            // Danh sách mặc định - kết quả kiểm tra cho hóa đơn mua vào
            checkResultList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("All", "Tất cả"),
                new KeyValuePair<string, string>("0", "Chưa kiểm tra"),
                new KeyValuePair<string, string>("1", "Hợp lệ"),
                new KeyValuePair<string, string>("2", "Không hợp lệ")
            };

            // Cấu hình ComboBox
            cboTTHD.DisplayMemberPath = "Value";
            cboTTHD.SelectedValuePath = "Key";
            cboTTHD.ItemsSource = invoiceStatusList;
            cboTTHD.SelectedIndex = 0;

            cboKQKT.DisplayMemberPath = "Value";
            cboKQKT.SelectedValuePath = "Key";
            cboKQKT.ItemsSource = checkResultList;
            cboKQKT.SelectedIndex = 0;
        }

        private void UpdateKQKTList()
        {
            if (isMuaVao)
            {
                // Kết quả kiểm tra cho hóa đơn mua vào
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
                // Kết quả kiểm tra cho hóa đơn bán ra
                checkResultList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", "Tất cả"),
                    new KeyValuePair<string, string>("0", "Chưa kiểm tra"),
                    new KeyValuePair<string, string>("1", "Hợp lệ"),
                    new KeyValuePair<string, string>("2", "Sai MST"),
                    new KeyValuePair<string, string>("3", "Sai tên"),
                    new KeyValuePair<string, string>("4", "Sai địa chỉ"),
                    new KeyValuePair<string, string>("5", "Không đủ điều kiện KT"),
                    new KeyValuePair<string, string>("6", "Đang kiểm tra"),
                    new KeyValuePair<string, string>("7", "Sai định dạng CQT"),
                    new KeyValuePair<string, string>("8", "Không tồn tại"),
                    new KeyValuePair<string, string>("9", "Sai CQT")
                };
            }

            // Cập nhật ComboBox
            cboKQKT.ItemsSource = null;
            cboKQKT.ItemsSource = checkResultList;
            cboKQKT.SelectedIndex = 0;
        }

        private void UpdateUIState()
        {
            // Cập nhật trạng thái của các điều khiển dựa trên tùy chọn hiện tại
            txtXMLFolderPath.IsEnabled = chkXmlZip.IsChecked == true;
            btnChonFolder.IsEnabled = chkXmlZip.IsChecked == true;

            // Kiểm tra nếu đường dẫn không tồn tại thì tạo
            if (chkXmlZip.IsChecked == true && !string.IsNullOrEmpty(txtXMLFolderPath.Text) && !Directory.Exists(txtXMLFolderPath.Text))
            {
                try
                {
                    Directory.CreateDirectory(txtXMLFolderPath.Text);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi khi tạo thư mục: {ex.Message}");
                }
            }
        }

        private void optMua_Changed(object sender, RoutedEventArgs e)
        {
            isMuaVao = true;
            UpdateKQKTList();
        }

        private void optBan_Changed(object sender, RoutedEventArgs e)
        {
            isMuaVao = false;
            UpdateKQKTList();
        }

        private void cboTTHD_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể xử lý logic khi thay đổi trạng thái
        }

        private void cboKQKT_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Có thể xử lý logic khi thay đổi kết quả kiểm tra
        }

        private void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateDateRange();
        }

        private void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            ValidateDateRange();
        }

        private void ValidateDateRange()
        {
            if (dpTuNgay.SelectedDate.HasValue && dpDenNgay.SelectedDate.HasValue)
            {
                if (dpTuNgay.SelectedDate.Value > dpDenNgay.SelectedDate.Value)
                {
                    lblSaiNgay.Visibility = Visibility.Visible;
                }
                else
                {
                    lblSaiNgay.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void chkXmlZip_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUIState();
        }

        private void btnChonFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog();
            dialog.Title = "Chọn thư mục để lưu file Zip XML";
            dialog.IsFolderPicker = true;

            // Thiết lập thư mục mặc định nếu đã có
            if (!string.IsNullOrEmpty(txtXMLFolderPath.Text) && Directory.Exists(txtXMLFolderPath.Text))
            {
                dialog.InitialDirectory = txtXMLFolderPath.Text;
                dialog.DefaultDirectory = txtXMLFolderPath.Text;
            }

            // Cấu hình thêm (tùy chọn)
            dialog.AddToMostRecentlyUsedList = false;
            dialog.AllowNonFileSystemItems = false;
            dialog.EnsureFileExists = true;
            dialog.EnsurePathExists = true;
            dialog.EnsureReadOnly = false;
            dialog.EnsureValidNames = true;
            dialog.Multiselect = false;
            dialog.ShowPlacesList = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                txtXMLFolderPath.Text = dialog.FileName;
            }
        }

        private async void btnTaiHoaDon_Click(object sender, RoutedEventArgs e)
        {
            // Kiểm tra ngày hợp lệ
            if (lblSaiNgay.Visibility == Visibility.Visible)
            {
                System.Windows.MessageBox.Show("Khoảng thời gian không hợp lệ. Vui lòng kiểm tra lại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!dpTuNgay.SelectedDate.HasValue || !dpDenNgay.SelectedDate.HasValue)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn khoảng thời gian tìm kiếm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Xác nhận xóa dữ liệu cũ
            bool clearOldData = false;
            if (invoiceSummaryList.Count > 0 || invoiceDetailList.Count > 0)
            {
                var result = System.Windows.MessageBox.Show("Bạn có muốn xóa dữ liệu cũ không?\nChọn [Yes] để xóa hoặc [No] để ghi kế tiếp.",
                                             "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                clearOldData = (result == MessageBoxResult.Yes);
            }

            // Xóa dữ liệu cũ nếu được chọn
            if (clearOldData)
            {
                invoiceSummaryList.Clear();
                invoiceDetailList.Clear();
            }

            // Bắt đầu tải hóa đơn
            await DownloadInvoicesAsync();
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            // Xóa token
            App.AuthToken = null;

            // Mở lại cửa sổ đăng nhập
            var loginWindow = new MainWindow();
            loginWindow.Show();

            // Đóng cửa sổ hiện tại
            this.Close();
        }

        private void btnChiTietXML_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Mở form trích xuất XML
            System.Windows.MessageBox.Show("Chức năng trích xuất XML sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async Task DownloadInvoicesAsync()
        {
            try
            {
                // Hiển thị trạng thái đang xử lý
                lblStatus.Text = "Đang tải dữ liệu...";
                btnTaiHoaDon.IsEnabled = false;

                // Lấy thông tin tìm kiếm
                DateTime tuNgay = dpTuNgay.SelectedDate.Value;
                DateTime denNgay = dpDenNgay.SelectedDate.Value;
                string tthai = (string)cboTTHD.SelectedValue;
                string ttxly = (string)cboKQKT.SelectedValue;

                // Chia khoảng thời gian thành các chu kỳ tháng
                var datePeriods = SplitDateRange(tuNgay, denNgay);
                int totalInvoices = 0; // This variable is declared but its value is never changed or used meaningfully after initialization. Consider its purpose.

                // Danh sách để lưu thông tin chi tiết hóa đơn để tải sau
                List<InvoiceIdentifier> invoiceIdentifiers = new List<InvoiceIdentifier>();

                // Đo thời gian xử lý
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                // Tải dữ liệu cho từng khoảng thời gian
                foreach (var period in datePeriods)
                {
                    // Tạo URL và tham số tìm kiếm cho API
                    string baseUrl = isMuaVao ?
                        "https://hoadondientu.gdt.gov.vn:30000/query/invoices/purchase" :
                        "https://hoadondientu.gdt.gov.vn:30000/query/invoices/sold";

                    string baseUrlSco = isMuaVao ?
                        "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/purchase" :
                        "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold";

                    string search = $"tdlap=ge={period.Item1:dd/MM/yyyy}T00:00:00;tdlap=le={period.Item2:dd/MM/yyyy}T23:59:59";

                    // Thêm điều kiện tìm kiếm
                    if (tthai != "All")
                    {
                        search += $";tthai=={tthai}";
                    }

                    if (ttxly != "All")
                    {
                        search += $";ttxly=={ttxly}";
                    }

                    string sort = "tdlap:asc,khmshdon:asc,shdon:asc";
                    int size = 50;

                    // Tham số ban đầu
                    string url = $"{baseUrl}?sort={sort}&size={size}&search={search}";

                    // Lấy dữ liệu tổng hợp hóa đơn
                    await GetInvoiceSummariesAsync(url, invoiceIdentifiers, invoiceSummaryList.Count); // Pass current count as startIndex

                    // Lấy dữ liệu từ API sco-query
                    url = $"{baseUrlSco}?sort={sort}&size={size}&search={search}";
                    await GetInvoiceSummariesAsync(url, invoiceIdentifiers, invoiceSummaryList.Count, true); // Pass current count as startIndex
                }

                // Tải chi tiết hóa đơn nếu được chọn
                if (chkCT.IsChecked == true && invoiceIdentifiers.Count > 0)
                {
                    lblStatus.Text = "Đang tải chi tiết hóa đơn...";

                    int count = 0;
                    foreach (var invoice in invoiceIdentifiers)
                    {
                        count++;
                        lblStatus.Text = $"Đang tải chi tiết hóa đơn {count}/{invoiceIdentifiers.Count}...";

                        // Cập nhật giao diện
                        await Task.Delay(1); // Cho phép UI update

                        await GetInvoiceDetailsAsync(invoice);
                    }
                }

                // Tải XML/HTML nếu được chọn
                if (chkXmlZip.IsChecked == true && invoiceIdentifiers.Count > 0)
                {
                    lblStatus.Text = "Đang tải file XML/HTML...";

                    // Tạo thư mục lưu trữ với timestamp
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string extractFolder = Path.Combine(txtXMLFolderPath.Text, $"FileGiaiNen_{timestamp}");

                    if (!Directory.Exists(extractFolder))
                    {
                        Directory.CreateDirectory(extractFolder);
                    }

                    int count = 0;
                    foreach (var invoice in invoiceIdentifiers)
                    {
                        count++;
                        lblStatus.Text = $"Đang tải XML/HTML {count}/{invoiceIdentifiers.Count}...";

                        // Cập nhật giao diện
                        await Task.Delay(1); // Cho phép UI update

                        await DownloadInvoiceXMLAsync(invoice, extractFolder);
                    }

                    // Hiển thị danh sách file đã tải
                    UpdateXMLFileList(extractFolder);
                }

                // Dừng đo thời gian
                stopwatch.Stop();

                // Hiển thị thông báo hoàn thành
                TimeSpan elapsed = stopwatch.Elapsed;
                lblStatus.Text = $"Hoàn thành. Thời gian xử lý: {elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";

                System.Windows.MessageBox.Show("Tải hóa đơn hoàn tất!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tải hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                lblStatus.Text = "Đã xảy ra lỗi khi tải hóa đơn";
            }
            finally
            {
                btnTaiHoaDon.IsEnabled = true;
            }
        }

        private List<Tuple<DateTime, DateTime>> SplitDateRange(DateTime startDate, DateTime endDate)
        {
            var result = new List<Tuple<DateTime, DateTime>>();

            // Bắt đầu từ ngày đầu tiên
            DateTime currentStart = startDate;

            while (currentStart <= endDate)
            {
                // Tính ngày cuối của tháng hiện tại
                DateTime currentEnd = new DateTime(currentStart.Year, currentStart.Month, DateTime.DaysInMonth(currentStart.Year, currentStart.Month));

                // Nếu ngày cuối vượt quá endDate, sử dụng endDate
                if (currentEnd > endDate)
                {
                    currentEnd = endDate;
                }

                // Thêm khoảng thời gian vào danh sách
                result.Add(new Tuple<DateTime, DateTime>(currentStart, currentEnd));

                // Chuyển sang tháng tiếp theo
                currentStart = currentEnd.AddDays(1);
            }

            return result;
        }

        private async Task GetInvoiceSummariesAsync(string url, List<InvoiceIdentifier> invoiceIdentifiers, int startIndex, bool isScoQuery = false)
        {
            try
            {
                string currentUrl = url;
                int index = startIndex; // Initialize with the passed startIndex

                while (!string.IsNullOrEmpty(currentUrl))
                {
                    // Gọi API
                    var response = await client.GetAsync(currentUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var result = JsonConvert.DeserializeObject<InvoiceQueryResult>(content);

                        if (result != null && result.Datas != null && result.Datas.Count > 0)
                        {
                            // Xử lý từng hóa đơn
                            foreach (var invoice in result.Datas)
                            {
                                index++;

                                // Tạo đối tượng tổng hợp hóa đơn
                                var summary = new InvoiceSummary
                                {
                                    STT = index,
                                    NgayHD = FormatInvoiceDate(invoice.Tdlap),
                                    MST = invoice.Nbmst,
                                    TenDV = invoice.Nban,
                                    KyHieu = invoice.Khhdon,
                                    SoHD = invoice.Shdon,
                                    TrangThai = GetInvoiceStatus(invoice.Tthai),
                                    TongTien = FormatCurrency(invoice.Tgtttbso)
                                };

                                // Thêm vào danh sách hiển thị
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    invoiceSummaryList.Add(summary);
                                });

                                // Lưu thông tin hóa đơn để tải chi tiết sau
                                invoiceIdentifiers.Add(new InvoiceIdentifier
                                {
                                    Nbmst = invoice.Nbmst,
                                    Khhdon = invoice.Khhdon,
                                    Shdon = invoice.Shdon,
                                    Khmshdon = invoice.Khmshdon,
                                    IsScoQuery = isScoQuery,
                                    Tdlap = invoice.Tdlap
                                });
                            }

                            // Kiểm tra xem có trang tiếp theo không
                            if (result.State != null)
                            {
                                // Tạo URL cho trang tiếp theo
                                int qIndex = url.IndexOf("?"); // Use a different variable name
                                string baseUrl = url.Substring(0, qIndex);
                                string queryParams = url.Substring(qIndex);

                                // Thêm hoặc thay thế state parameter
                                if (queryParams.Contains("state="))
                                {
                                    // Thay thế state parameter
                                    int stateIndex = queryParams.IndexOf("state=");
                                    int nextParamIndex = queryParams.IndexOf("&", stateIndex);

                                    if (nextParamIndex > 0)
                                    {
                                        // Có parameter tiếp theo
                                        string beforeState = queryParams.Substring(0, stateIndex);
                                        string afterState = queryParams.Substring(nextParamIndex);
                                        queryParams = beforeState + "state=" + result.State + afterState;
                                    }
                                    else
                                    {
                                        // State là parameter cuối cùng
                                        string beforeState = queryParams.Substring(0, stateIndex);
                                        queryParams = beforeState + "state=" + result.State;
                                    }
                                }
                                else
                                {
                                    // Thêm state parameter
                                    queryParams += (queryParams.Contains("?") ? "&" : "?") + "state=" + result.State;
                                }
                                currentUrl = baseUrl + queryParams;
                            }
                            else
                            {
                                // Không còn trang tiếp theo
                                currentUrl = string.Empty;
                            }
                        }
                        else
                        {
                            // Không có dữ liệu
                            currentUrl = string.Empty;
                        }
                    }
                    else
                    {
                        throw new Exception($"Lỗi khi gọi API: {response.StatusCode} - {response.ReasonPhrase}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy danh sách hóa đơn: {ex.Message}");
                System.Windows.MessageBox.Show($"Đã xảy ra lỗi khi lấy danh sách hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task GetInvoiceDetailsAsync(InvoiceIdentifier invoice)
        {
            try
            {
                // Tạo URL để lấy chi tiết hóa đơn
                string baseUrl = invoice.IsScoQuery ?
                    "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/detail?" :
                    "https://hoadondientu.gdt.gov.vn:30000/query/invoices/detail?";

                string url = $"{baseUrl}nbmst={invoice.Nbmst}&khhdon={invoice.Khhdon}&shdon={invoice.Shdon}&khmshdon={invoice.Khmshdon}";

                // Gọi API
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<InvoiceDetailResult>(content);

                    if (result != null && result.Hdhhdvu != null && result.Hdhhdvu.Count > 0)
                    {
                        // Xử lý từng hàng hóa dịch vụ
                        int index = 1;
                        foreach (var item in result.Hdhhdvu)
                        {
                            // Tạo đối tượng chi tiết hóa đơn
                            var detail = new InvoiceDetail
                            {
                                STT = index++,
                                NgayHD = FormatInvoiceDate(invoice.Tdlap),
                                MST = invoice.Nbmst,
                                KyHieu = invoice.Khhdon,
                                SoHD = invoice.Shdon,
                                MaHH = item.Thhdv, // Assuming MaHH is Thhdv, might need adjustment based on actual data
                                TenHH = item.Thhdv,
                                DonVi = item.Dvt,
                                SoLuong = item.Sluong,
                                DonGia = FormatCurrency(item.Dgia),
                                ThanhTien = FormatCurrency(item.Thtien)
                            };

                            // Thêm vào danh sách hiển thị
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                invoiceDetailList.Add(detail);
                            });
                        }
                    }
                }
                else
                {
                    Debug.WriteLine($"Lỗi khi gọi API chi tiết hóa đơn: {response.StatusCode} - {response.ReasonPhrase}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy chi tiết hóa đơn: {ex.Message}");
            }
        }

        private async Task DownloadInvoiceXMLAsync(InvoiceIdentifier invoice, string extractFolder)
        {
            try
            {
                // Tạo URL để tải XML/HTML
                string baseUrl = invoice.IsScoQuery ?
                    "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/export-xml?" :
                    "https://hoadondientu.gdt.gov.vn:30000/query/invoices/export-xml?";

                string url = $"{baseUrl}nbmst={invoice.Nbmst}&khhdon={invoice.Khhdon}&shdon={invoice.Shdon}&khmshdon={invoice.Khmshdon}";

                // Cấu hình request
                var request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.Headers.Add("Accept-Encoding", "gzip"); // If server supports gzip, HttpClient handles decompression automatically by default

                // Gọi API
                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    // Lưu file zip
                    string zipFileName = $"{invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}.zip"; // More unique filename
                    string zipFilePath = Path.Combine(txtXMLFolderPath.Text, zipFileName); // Save to the base XML folder, not extractFolder

                    using (var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    // Giải nén file vào thư mục giải nén cụ thể (extractFolder)
                    ExtractZipFile(zipFilePath, extractFolder);
                    // Optionally delete the zip file after extraction if not needed:
                    // File.Delete(zipFilePath); 
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Debug.WriteLine($"Không tồn tại hồ sơ gốc của hóa đơn (500): {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}");
                }
                else if ((int)response.StatusCode == 429)
                {
                    System.Windows.MessageBox.Show("Lỗi 429 - Gửi quá nhiều yêu cầu tới máy chủ Web.", "Lỗi tải XML", MessageBoxButton.OK, MessageBoxImage.Warning);
                    // Consider adding a delay and retry mechanism here
                }
                else
                {
                    Debug.WriteLine($"Lỗi khi tải XML/HTML: {response.StatusCode} - {response.ReasonPhrase} cho hóa đơn {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải XML/HTML cho hóa đơn {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}: {ex.Message}");
            }
        }

        private void ExtractZipFile(string zipFilePath, string extractFolder)
        {
            try
            {
                // Đảm bảo thư mục giải nén tồn tại
                if (!Directory.Exists(extractFolder))
                {
                    Directory.CreateDirectory(extractFolder);
                }
                // Giải nén file ZIP, ghi đè nếu file đã tồn tại
                ZipFile.ExtractToDirectory(zipFilePath, extractFolder, true);
            }
            catch (IOException ioEx) when (ioEx.Message.Contains("already exists"))
            {
                Debug.WriteLine($"Lỗi khi giải nén (file có thể đã tồn tại và đang được sử dụng): {ioEx.Message} cho file {zipFilePath}");
                // Handle specific case if files exist, e.g. by trying to rename or skip.
                // For now, just logging. The 'true' in ExtractToDirectory should overwrite.
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi giải nén file: {ex.Message} cho file {zipFilePath}");
            }
        }

        private void UpdateXMLFileList(string folderPath)
        {
            try
            {
                // Lấy danh sách file trong thư mục giải nén
                var directory = new DirectoryInfo(folderPath);
                if (!directory.Exists)
                {
                    Debug.WriteLine($"Thư mục không tồn tại để liệt kê file: {folderPath}");
                    return;
                }
                var files = directory.GetFiles(); // You might want to filter by .xml or .html here

                // Cập nhật ListView
                Application.Current.Dispatcher.Invoke(() =>
                {
                    xmlFileList.Clear();
                    foreach (var file in files)
                    {
                        xmlFileList.Add(file);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật danh sách file từ {folderPath}: {ex.Message}");
            }
        }

        private string GetInvoiceStatus(string status)
        {
            switch (status)
            {
                case "0": return "Chưa duyệt";
                case "1": return "Đã duyệt";
                case "2": return "Đã hủy";
                case "3": return "Đang duyệt";
                case "4": return "Từ chối duyệt";
                case "5": return "Chờ hủy";
                default: return "Không xác định";
            }
        }

        private string FormatInvoiceDate(string isoDate)
        {
            if (string.IsNullOrEmpty(isoDate)) return string.Empty;

            try
            {
                // Chuyển đổi từ ISO date string (assuming yyyy-MM-ddTHH:mm:ss or similar) sang DateTime
                if (DateTime.TryParse(isoDate, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime date))
                {
                    return date.ToLocalTime().ToString("dd/MM/yyyy");
                }
                return isoDate; // Return original if parsing fails
            }
            catch
            {
                return isoDate; // Return original on any other error
            }
        }

        private string FormatCurrency(string amount)
        {
            if (string.IsNullOrEmpty(amount)) return "0";

            try
            {
                // Assuming amount is a plain number string, potentially with a decimal point
                if (decimal.TryParse(amount, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal value))
                {
                    return string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:N0}", value); // N0 for no decimal places
                    // Use "{0:N2}" for 2 decimal places if needed
                }
                return amount; // Return original if parsing fails
            }
            catch
            {
                return amount; // Return original on any other error
            }
        }
    }
}