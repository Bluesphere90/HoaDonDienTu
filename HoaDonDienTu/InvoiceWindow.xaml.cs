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
using Newtonsoft.Json;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;
using System.IO.Compression;
// using System.Windows.Forms; // Đã có using Microsoft.WindowsAPICodePack.Dialogs
using HoaDonDienTu.Models; // Quan trọng: Namespace của Models
// using Microsoft.WindowsAPICodePack.Dialogs; // Bị comment nếu đã có using System.Windows.Forms cho FolderBrowserDialog
using System.Windows.Forms; // Sử dụng cho FolderBrowserDialog


namespace HoaDonDienTu
{
    public partial class InvoiceWindow : Window
    {
        private HttpClient client;
        private bool isMuaVao = true;
        private ObservableCollection<InvoiceSummary> invoiceSummaryList = new ObservableCollection<InvoiceSummary>();
        // Đổi kiểu của invoiceDetailList
        private ObservableCollection<InvoiceDisplayItem> invoiceDetailList = new ObservableCollection<InvoiceDisplayItem>();
        private ObservableCollection<FileInfo> xmlFileList = new ObservableCollection<FileInfo>();

        private List<KeyValuePair<string, string>> invoiceStatusList = new List<KeyValuePair<string, string>>();
        private List<KeyValuePair<string, string>> checkResultList = new List<KeyValuePair<string, string>>();

        public InvoiceWindow()
        {
            try
            {
                InitializeComponent();

                // Ensure collections are initialized first
                invoiceSummaryList = new ObservableCollection<InvoiceSummary>();
                invoiceDetailList = new ObservableCollection<InvoiceDisplayItem>(); // Đổi kiểu
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
                dgChiTiet.ItemsSource = invoiceDetailList; // Đã đổi kiểu
                lvXMLFiles.ItemsSource = xmlFileList;

                dpTuNgay.SelectedDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                dpDenNgay.SelectedDate = DateTime.Now;

                string defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "HoaDonDienTu");
                if (!Directory.Exists(defaultPath))
                {
                    try
                    {
                        Directory.CreateDirectory(defaultPath);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Không thể tạo thư mục mặc định: {ex.Message}");
                        defaultPath = Path.GetTempPath();
                    }
                }
                txtXMLFolderPath.Text = defaultPath;

                LoadStatusAndCheckResultLists();
                UpdateUIState();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo InvoiceWindow: {ex.Message}");
                if (ex.InnerException != null) Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                System.Windows.MessageBox.Show($"Lỗi khởi tạo cửa sổ hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);

                var loginWindow = new MainWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void LoadStatusAndCheckResultLists()
        {
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

            // Mặc định là KQKT cho hóa đơn mua vào
            checkResultList = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("All", "Tất cả"),
                new KeyValuePair<string, string>("0", "Chưa kiểm tra"),
                new KeyValuePair<string, string>("1", "Hợp lệ"),
                new KeyValuePair<string, string>("2", "Không hợp lệ")
            };
            // Gọi UpdateKQKTList để thiết lập đúng danh sách ban đầu dựa trên isMuaVao
            UpdateKQKTList();


            cboTTHD.DisplayMemberPath = "Value";
            cboTTHD.SelectedValuePath = "Key";
            cboTTHD.ItemsSource = invoiceStatusList;
            cboTTHD.SelectedIndex = 0;

            // cboKQKT sẽ được cấu hình trong UpdateKQKTList
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
            else // Hóa đơn bán ra
            {
                checkResultList = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("All", "Tất cả"),
                    new KeyValuePair<string, string>("0", "Chưa kiểm tra CQT"), // Diễn giải rõ hơn
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
            cboKQKT.SelectedIndex = 0;
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
            if (optMua.IsChecked == true) // Đảm bảo chỉ chạy khi radio button này được chọn
            {
                isMuaVao = true;
                UpdateKQKTList();
            }
        }

        private void optBan_Changed(object sender, RoutedEventArgs e)
        {
            if (optBan.IsChecked == true) // Đảm bảo chỉ chạy khi radio button này được chọn
            {
                isMuaVao = false;
                UpdateKQKTList();
            }
        }

        private void cboTTHD_SelectionChanged(object sender, SelectionChangedEventArgs e) { /* Xử lý nếu cần */ }
        private void cboKQKT_SelectionChanged(object sender, SelectionChangedEventArgs e) { /* Xử lý nếu cần */ }
        private void dpTuNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ValidateDateRange();
        private void dpDenNgay_SelectedDateChanged(object sender, SelectionChangedEventArgs e) => ValidateDateRange();

        private void ValidateDateRange()
        {
            if (dpTuNgay.SelectedDate.HasValue && dpDenNgay.SelectedDate.HasValue)
            {
                lblSaiNgay.Visibility = dpTuNgay.SelectedDate.Value > dpDenNgay.SelectedDate.Value ?
                                        Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void chkXmlZip_Checked(object sender, RoutedEventArgs e) => UpdateUIState();
        private void chkXmlZip_Unchecked(object sender, RoutedEventArgs e) => UpdateUIState();


        private void btnChonFolder_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog()) // Sử dụng System.Windows.Forms
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

            bool clearOldData = false;
            if (invoiceSummaryList.Any() || invoiceDetailList.Any())
            {
                var result = System.Windows.MessageBox.Show("Bạn có muốn xóa dữ liệu cũ không?\nChọn [Yes] để xóa hoặc [No] để ghi kế tiếp.",
                                             "Thông báo", MessageBoxButton.YesNo, MessageBoxImage.Question);
                clearOldData = (result == MessageBoxResult.Yes);
            }

            if (clearOldData)
            {
                invoiceSummaryList.Clear();
                invoiceDetailList.Clear();
                xmlFileList.Clear();
            }
            await DownloadInvoicesAsync();
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

        private async Task DownloadInvoicesAsync()
        {
            try
            {
                lblStatus.Text = "Đang tải dữ liệu...";
                btnTaiHoaDon.IsEnabled = false;

                DateTime tuNgay = dpTuNgay.SelectedDate.Value;
                DateTime denNgay = dpDenNgay.SelectedDate.Value;
                string tthai = (string)cboTTHD.SelectedValue;
                string ttxly = (string)cboKQKT.SelectedValue;

                var datePeriods = SplitDateRange(tuNgay, denNgay);
                List<InvoiceIdentifier> invoiceIdentifiers = new List<InvoiceIdentifier>();
                var stopwatch = Stopwatch.StartNew();

                foreach (var period in datePeriods)
                {
                    string baseUrl = isMuaVao ? "https://hoadondientu.gdt.gov.vn:30000/query/invoices/purchase"
                                              : "https://hoadondientu.gdt.gov.vn:30000/query/invoices/sold";
                    string baseUrlSco = isMuaVao ? "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/purchase"
                                                 : "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/sold";

                    string search = $"tdlap=ge={period.Item1:dd/MM/yyyy}T00:00:00;tdlap=le={period.Item2:dd/MM/yyyy}T23:59:59";
                    if (tthai != "All") search += $";tthai=={tthai}";
                    if (ttxly != "All") search += $";ttxly=={ttxly}";
                    string sort = "tdlap:asc,khmshdon:asc,shdon:asc";
                    int size = 50; // Số lượng item mỗi trang, có thể tăng nếu API cho phép

                    string urlQuery = $"{baseUrl}?sort={sort}&size={size}&search={search}";
                    await GetInvoiceSummariesAsync(urlQuery, invoiceIdentifiers, invoiceSummaryList.Count);

                    string urlScoQuery = $"{baseUrlSco}?sort={sort}&size={size}&search={search}";
                    await GetInvoiceSummariesAsync(urlScoQuery, invoiceIdentifiers, invoiceSummaryList.Count, true);
                }

                if (chkCT.IsChecked == true && invoiceIdentifiers.Any())
                {
                    lblStatus.Text = "Đang tải chi tiết hóa đơn...";
                    for (int i = 0; i < invoiceIdentifiers.Count; i++)
                    {
                        lblStatus.Text = $"Đang tải chi tiết hóa đơn {i + 1}/{invoiceIdentifiers.Count}...";
                        await Task.Delay(1); // Cho phép UI update
                        await GetInvoiceDetailsAsync(invoiceIdentifiers[i]);
                    }
                }

                if (chkXmlZip.IsChecked == true && invoiceIdentifiers.Any())
                {
                    lblStatus.Text = "Đang tải file XML/HTML...";
                    string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                    string baseExtractFolder = Path.Combine(txtXMLFolderPath.Text, $"HoaDon_{timestamp}"); // Một thư mục chung cho lần tải này

                    if (!Directory.Exists(baseExtractFolder)) Directory.CreateDirectory(baseExtractFolder);

                    for (int i = 0; i < invoiceIdentifiers.Count; i++)
                    {
                        lblStatus.Text = $"Đang tải XML/HTML {i + 1}/{invoiceIdentifiers.Count}...";
                        await Task.Delay(1);
                        // Mỗi hóa đơn giải nén vào thư mục con riêng để tránh ghi đè file cùng tên (như file output.xml)
                        string invoiceSpecificExtractFolder = Path.Combine(baseExtractFolder, $"{invoiceIdentifiers[i].Nbmst}_{invoiceIdentifiers[i].Khhdon}_{invoiceIdentifiers[i].Shdon}");
                        if (!Directory.Exists(invoiceSpecificExtractFolder)) Directory.CreateDirectory(invoiceSpecificExtractFolder);

                        await DownloadInvoiceXMLAsync(invoiceIdentifiers[i], txtXMLFolderPath.Text, invoiceSpecificExtractFolder); // Truyền cả thư mục gốc và thư mục giải nén cụ thể
                    }
                    UpdateXMLFileList(baseExtractFolder); // Liệt kê file từ thư mục cha chứa các thư mục con đã giải nén
                }

                stopwatch.Stop();
                TimeSpan elapsed = stopwatch.Elapsed;
                lblStatus.Text = $"Hoàn thành. Thời gian xử lý: {elapsed:hh\\:mm\\:ss}";
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

        private async Task GetInvoiceSummariesAsync(string initialUrl, List<InvoiceIdentifier> invoiceIdentifiers, int startIndex, bool isScoQuery = false)
        {
            string currentUrl = initialUrl;
            int currentIndex = startIndex;

            try
            {
                while (!string.IsNullOrEmpty(currentUrl))
                {
                    var response = await client.GetAsync(currentUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        Debug.WriteLine($"Lỗi API (Summaries) {response.StatusCode}: {currentUrl}");
                        // Có thể throw exception hoặc break tùy theo chiến lược xử lý lỗi
                        break;
                    }

                    var content = await response.Content.ReadAsStringAsync();
                    var result = JsonConvert.DeserializeObject<InvoiceQueryResult>(content);

                    if (result?.Datas != null && result.Datas.Any())
                    {
                        foreach (var invoiceData in result.Datas)
                        {
                            currentIndex++;
                            var summary = new InvoiceSummary
                            {
                                STT = currentIndex,
                                NgayHD = FormatHelper.FormatInvoiceDate(invoiceData.Tdlap),
                                MST = invoiceData.Nbmst,
                                TenDV = invoiceData.Nban,
                                KyHieu = invoiceData.Khhdon,
                                SoHD = invoiceData.Shdon,
                                TrangThai = FormatHelper.GetInvoiceStatusDescription(invoiceData.Tthai),
                                TongTien = FormatHelper.FormatCurrencyFromString(invoiceData.Tgtttbso)
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(() => invoiceSummaryList.Add(summary));

                            invoiceIdentifiers.Add(new InvoiceIdentifier
                            {
                                Nbmst = invoiceData.Nbmst,
                                Khhdon = invoiceData.Khhdon,
                                Shdon = invoiceData.Shdon,
                                Khmshdon = invoiceData.Khmshdon,
                                IsScoQuery = isScoQuery,
                                Tdlap = invoiceData.Tdlap
                            });
                        }

                        if (!string.IsNullOrEmpty(result.State))
                        {
                            // Xây dựng URL cho trang tiếp theo
                            Uri baseUri = new Uri(currentUrl.Contains("?") ? currentUrl.Substring(0, currentUrl.IndexOf('?')) : currentUrl);
                            var queryParams = System.Web.HttpUtility.ParseQueryString(new Uri(currentUrl).Query);
                            queryParams["state"] = result.State;
                            currentUrl = $"{baseUri}?{queryParams}";
                        }
                        else currentUrl = string.Empty;
                    }
                    else currentUrl = string.Empty;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy danh sách hóa đơn từ {initialUrl}: {ex.Message}");
                // Không nên hiển thị MessageBox trong vòng lặp, có thể ghi log hoặc thông báo một lần ở cuối
            }
        }

        private async Task GetInvoiceDetailsAsync(InvoiceIdentifier invoiceIdentity)
        {
            try
            {
                string baseUrl = invoiceIdentity.IsScoQuery ?
                    "https://hoadondientu.gdt.gov.vn:30000/sco-query/invoices/detail?" :
                    "https://hoadondientu.gdt.gov.vn:30000/query/invoices/detail?";
                string url = $"{baseUrl}nbmst={invoiceIdentity.Nbmst}&khhdon={invoiceIdentity.Khhdon}&shdon={invoiceIdentity.Shdon}&khmshdon={invoiceIdentity.Khmshdon}";

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var apiResponseData = JsonConvert.DeserializeObject<InvoiceDetailApiResponse>(content);

                    if (apiResponseData != null)
                    {
                        // Xác định Hình thức thanh toán (ưu tiên ApiHinhThucThanhToanAlt nếu ApiHinhThucThanhToan null)
                        string hinhThucTT = !string.IsNullOrEmpty(apiResponseData.ApiHinhThucThanhToan) ?
                                            apiResponseData.ApiHinhThucThanhToan :
                                            apiResponseData.ApiHinhThucThanhToanAlt;


                        if (apiResponseData.Hdhhdvu != null && apiResponseData.Hdhhdvu.Any())
                        {
                            foreach (var rawItem in apiResponseData.Hdhhdvu)
                            {
                                var displayItem = new InvoiceDisplayItem
                                {
                                    MauSoHoaDon = apiResponseData.ApiMauSoHoaDon,
                                    KyHieuHoaDon = apiResponseData.ApiKyHieuHoaDon,
                                    SoHoaDon = apiResponseData.ApiSoHoaDon,
                                    NgayLapHoaDon = FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayLapHoaDonISO),
                                    NgayKy = FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayKyISO),
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

                                    TongTienChuaThue_HD = apiResponseData.ApiTongTienChuaThue,
                                    TongTienThue_HD = apiResponseData.ApiTongTienThue,
                                    TongTienChietKhauTM_HD = apiResponseData.ApiTongTienChietKhauTM,
                                    TongTienThanhToan_HD = apiResponseData.ApiTongTienThanhToan,
                                    TongTienThanhToanBangChu_HD = apiResponseData.ApiTongTienThanhToanBangChu,
                                    TrangThai_HD = FormatHelper.GetInvoiceStatusDescription(apiResponseData.ApiTrangThaiHD_Code),
                                    TinhTrangXuLy_HD = FormatHelper.GetInvoiceProcessingStatusDescription(apiResponseData.ApiTinhTrangXuLy_Code)
                                };
                                System.Windows.Application.Current.Dispatcher.Invoke(() => invoiceDetailList.Add(displayItem));
                            }
                        }
                        else // Không có dòng chi tiết
                        {
                            var displayItemMsg = new InvoiceDisplayItem
                            {
                                MauSoHoaDon = apiResponseData.ApiMauSoHoaDon ?? invoiceIdentity.Khmshdon,
                                KyHieuHoaDon = apiResponseData.ApiKyHieuHoaDon ?? invoiceIdentity.Khhdon,
                                SoHoaDon = apiResponseData.ApiSoHoaDon ?? invoiceIdentity.Shdon,
                                NgayLapHoaDon = FormatHelper.FormatInvoiceDate(apiResponseData.ApiNgayLapHoaDonISO ?? invoiceIdentity.Tdlap),
                                TenNguoiBan = apiResponseData.ApiTenNguoiBan,
                                TenNguoiMua = apiResponseData.ApiTenNguoiMua,
                                TenHHDV = "Không có dữ liệu chi tiết hoặc hóa đơn không có dòng hàng hóa.",
                                TongTienThanhToan_HD = apiResponseData.ApiTongTienThanhToan,
                                TrangThai_HD = FormatHelper.GetInvoiceStatusDescription(apiResponseData.ApiTrangThaiHD_Code),
                                TinhTrangXuLy_HD = FormatHelper.GetInvoiceProcessingStatusDescription(apiResponseData.ApiTinhTrangXuLy_Code)
                            };
                            System.Windows.Application.Current.Dispatcher.Invoke(() => invoiceDetailList.Add(displayItemMsg));
                        }
                    }
                    else // Deserialize thất bại
                    {
                        HandleDetailError($"Lỗi phân tích dữ liệu chi tiết từ API.", invoiceIdentity);
                    }
                }
                else // Lỗi API
                {
                    Debug.WriteLine($"Lỗi API (Details) {response.StatusCode}: {url}");
                    HandleDetailError($"Lỗi API ({response.StatusCode}) khi lấy chi tiết.", invoiceIdentity);
                }
            }
            catch (JsonException jsonEx)
            {
                Debug.WriteLine($"Lỗi Deserialize JSON chi tiết HĐ {invoiceIdentity.Shdon}: {jsonEx.Message}");
                HandleDetailError("Lỗi đọc dữ liệu chi tiết từ API.", invoiceIdentity);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy chi tiết HĐ {invoiceIdentity.Shdon}: {ex.Message}");
                HandleDetailError("Lỗi hệ thống khi lấy chi tiết.", invoiceIdentity);
            }
        }

        private void HandleDetailError(string errorMessage, InvoiceIdentifier invoiceIdentity)
        {
            var errorDisplayItem = new InvoiceDisplayItem
            {
                MauSoHoaDon = invoiceIdentity.Khmshdon,
                KyHieuHoaDon = invoiceIdentity.Khhdon,
                SoHoaDon = invoiceIdentity.Shdon,
                MaSoThueNguoiBan = invoiceIdentity.Nbmst, // Hoặc người mua tùy isMuaVao
                TenHHDV = $"{errorMessage} (HĐ: {invoiceIdentity.Shdon})",
            };
            System.Windows.Application.Current.Dispatcher.Invoke(() => { invoiceDetailList.Add(errorDisplayItem); });
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
                // ... (các header như cũ)
                request.Headers.Add("User-Agent", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/128.0.0.0 Safari/537.36");
                request.Headers.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                request.Headers.Add("Accept-Encoding", "gzip");


                var response = await client.SendAsync(request);

                if (response.IsSuccessStatusCode)
                {
                    string zipFileName = $"{invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}.zip";
                    string zipFilePath = Path.Combine(baseZipSavePath, zipFileName); // Lưu file zip vào thư mục gốc được chọn

                    using (var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }
                    ExtractZipFile(zipFilePath, specificExtractFolder); // Giải nén vào thư mục con riêng
                    // File.Delete(zipFilePath); // Cân nhắc xóa file zip sau khi giải nén nếu không cần nữa
                }
                // ... (xử lý lỗi 500, 429 như cũ) ...
                else if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
                {
                    Debug.WriteLine($"Không tồn tại hồ sơ gốc của hóa đơn (500): {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}");
                }
                else if ((int)response.StatusCode == 429)
                {
                    System.Windows.MessageBox.Show("Lỗi 429 - Gửi quá nhiều yêu cầu tới máy chủ Web.", "Lỗi tải XML", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Debug.WriteLine($"Lỗi khi tải XML/HTML: {response.StatusCode} - {response.ReasonPhrase} cho hóa đơn {invoice.Nbmst}_{invoice.Khhdon}_{invoice.Shdon}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi tải XML/HTML cho HĐ {invoice.Shdon}: {ex.Message}");
            }
        }

        private void ExtractZipFile(string zipFilePath, string extractFolder)
        {
            try
            {
                if (!Directory.Exists(extractFolder)) Directory.CreateDirectory(extractFolder);
                ZipFile.ExtractToDirectory(zipFilePath, extractFolder, true); // true để ghi đè
            }
            catch (IOException ioEx) when (ioEx.Message.Contains("already exists"))
            {
                Debug.WriteLine($"Lỗi giải nén (file đã tồn tại và có thể đang được sử dụng): {ioEx.Message} cho {zipFilePath}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi giải nén file {zipFilePath}: {ex.Message}");
            }
        }

        private void UpdateXMLFileList(string rootFolderPath) // Root folder chứa các thư mục con đã giải nén
        {
            try
            {
                var rootDir = new DirectoryInfo(rootFolderPath);
                if (!rootDir.Exists)
                {
                    Debug.WriteLine($"Thư mục gốc không tồn tại để liệt kê file: {rootFolderPath}");
                    return;
                }

                // Lấy tất cả file .xml và .pdf trong tất cả thư mục con của rootFolderPath
                var files = rootDir.GetFiles("*.*", SearchOption.AllDirectories)
                                .Where(f => f.Extension.Equals(".xml", StringComparison.OrdinalIgnoreCase) ||
                                            f.Extension.Equals(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                            f.Extension.Equals(".html", StringComparison.OrdinalIgnoreCase));


                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    xmlFileList.Clear(); // Xóa danh sách cũ trước khi thêm mới
                    foreach (var file in files)
                    {
                        xmlFileList.Add(file);
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cập nhật danh sách file từ {rootFolderPath}: {ex.Message}");
            }
        }

        // Các hàm FormatCurrency, FormatInvoiceDate, GetInvoiceStatus đã được chuyển vào FormatHelper
        // nên có thể xóa khỏi đây nếu không còn sử dụng trực tiếp ở đâu khác trong class này.
        // Tuy nhiên, GetInvoiceSummariesAsync vẫn đang dùng FormatInvoiceDate và GetInvoiceStatus cục bộ,
        // bạn nên đổi chúng thành FormatHelper.FormatInvoiceDate và FormatHelper.GetInvoiceStatusDescription.
        // Hàm FormatCurrency(string) vẫn đang được GetInvoiceSummariesAsync sử dụng.
        // Nếu muốn, có thể tạo FormatHelper.FormatCurrencyFromString(string)
    }
}
// --- END OF FILE InvoiceWindow.xaml.cs ---