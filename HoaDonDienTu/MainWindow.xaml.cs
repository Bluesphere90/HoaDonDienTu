using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Newtonsoft.Json;
using System.Net;
using System.Diagnostics;
using Newtonsoft.Json.Linq;
using HoaDonDienTu.Services; // Thêm namespace cho DatabaseService

namespace HoaDonDienTu
{
    public partial class MainWindow : Window
    {
        private List<UserCredential> savedUsers = new List<UserCredential>();
        private string captchaKey = string.Empty;
        private HttpClient client;
        private DatabaseService databaseService; // Thêm DatabaseService

        public MainWindow()
        {
            InitializeComponent();

            // Cấu hình HttpClient
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

            // Khởi tạo DatabaseService
            databaseService = new DatabaseService();

            // Nạp danh sách người dùng đã lưu
            LoadSavedUsers();

            // Lấy Captcha
            GetCaptcha();
        }

        private async void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text) ||
                (string.IsNullOrEmpty(txtPassword.Password) && string.IsNullOrEmpty(txtPasswordVisible.Text)) ||
                string.IsNullOrEmpty(txtCaptcha.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ thông tin đăng nhập!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            lblThanhCong.Visibility = Visibility.Collapsed;
            lblThatBai.Visibility = Visibility.Collapsed;

            try
            {
                // Hiển thị trạng thái đang xử lý
                btnLogin.IsEnabled = false;
                btnLogin.Content = "ĐANG XỬ LÝ...";

                // Lấy mật khẩu từ PasswordBox hoặc TextBox tùy vào trạng thái hiển thị
                string password = txtPassword.Visibility == Visibility.Visible ?
                                txtPassword.Password : txtPasswordVisible.Text;

                // Gọi API đăng nhập
                var loginResult = await LoginAsync(txtUsername.Text, password, txtCaptcha.Text, captchaKey);

                if (loginResult.Success)
                {
                    lblThanhCong.Visibility = Visibility.Visible;
                    lblThatBai.Visibility = Visibility.Collapsed;

                    // Lưu thông tin đăng nhập
                    SaveUserCredentials(txtUsername.Text, password, lblTenDV.Text);

                    Debug.WriteLine("Bắt đầu khởi tạo db " + loginResult.CompanyTaxCode);

                    // Khởi tạo database cho công ty
                    await InitializeDatabaseForCompany(loginResult.CompanyTaxCode);

                    // Chuyển sang cửa sổ quản lý hóa đơn sau 1 giây
                    await Task.Delay(1000);

                    try
                    {
                        // Kiểm tra xem token có giá trị không
                        if (string.IsNullOrEmpty(App.AuthToken))
                        {
                            MessageBox.Show("Token xác thực không hợp lệ", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }

                        // Hiển thị thông tin trước khi mở cửa sổ mới
                        Debug.WriteLine($"Đang mở cửa sổ InvoiceWindow với Token: {App.AuthToken.Substring(0, Math.Min(App.AuthToken.Length, 20))}...");

                        try
                        {
                            // Truyền DatabaseService vào InvoiceWindow
                            InvoiceWindow invoiceWindow = new InvoiceWindow(databaseService, loginResult.CompanyTaxCode);
                            Debug.WriteLine("Đã khởi tạo InvoiceWindow thành công");

                            // Hiển thị cửa sổ
                            invoiceWindow.Show();
                            Debug.WriteLine("Đã hiển thị InvoiceWindow");

                            // Đóng cửa sổ đăng nhập
                            this.Close();
                        }
                        catch (TypeInitializationException typeEx)
                        {
                            Debug.WriteLine($"Lỗi khởi tạo loại: {typeEx.Message}");
                            if (typeEx.InnerException != null)
                            {
                                Debug.WriteLine($"Inner Exception: {typeEx.InnerException.Message}");
                            }
                            MessageBox.Show($"Không thể khởi tạo cửa sổ hóa đơn: {typeEx.Message}\n{typeEx.InnerException?.Message}",
                                           "Lỗi khởi tạo", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (System.IO.FileNotFoundException fileEx)
                        {
                            Debug.WriteLine($"Thiếu tệp: {fileEx.Message}, FileName: {fileEx.FileName}");
                            MessageBox.Show($"Thiếu tệp cần thiết để mở cửa sổ hóa đơn: {fileEx.FileName}",
                                           "Lỗi tệp tin", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Lỗi khi mở InvoiceWindow: {ex.Message}");
                            if (ex.InnerException != null)
                            {
                                Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                            }
                            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
                            MessageBox.Show($"Lỗi khi mở cửa sổ hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi ngoài khi mở InvoiceWindow: {ex.Message}");
                        MessageBox.Show($"Lỗi khi chuyển đến cửa sổ hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    lblThanhCong.Visibility = Visibility.Collapsed;
                    lblThatBai.Visibility = Visibility.Visible;

                    // Lấy captcha mới
                    GetCaptcha();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                lblThatBai.Visibility = Visibility.Visible;
            }
            finally
            {
                btnLogin.IsEnabled = true;
                btnLogin.Content = "ĐĂNG NHẬP";
            }
        }

        // Thêm class để chứa kết quả đăng nhập
        private class LoginResult
        {
            public bool Success { get; set; }
            public string CompanyTaxCode { get; set; }
            public string CompanyName { get; set; }
            public string ErrorMessage { get; set; }
        }

        private async Task<LoginResult> LoginAsync(string username, string password, string captchaValue, string captchaKey)
        {
            try
            {
                string url = "https://hoadondientu.gdt.gov.vn:30000/security-taxpayer/authenticate";

                // Tạo dữ liệu đăng nhập
                var loginData = new
                {
                    username = username,
                    password = password,
                    cvalue = captchaValue,
                    ckey = captchaKey
                };

                // Chuyển đổi thành JSON
                string jsonData = JsonConvert.SerializeObject(loginData);
                Debug.WriteLine($"Dữ liệu đăng nhập: {jsonData}");

                // Tạo HttpContent
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");

                // Gửi request
                var response = await client.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"Kết quả đăng nhập: {responseContent}");

                // Phân tích kết quả
                var result = JsonConvert.DeserializeObject<dynamic>(responseContent);

                if (response.IsSuccessStatusCode && result != null)
                {
                    // Kiểm tra xem token có tồn tại không
                    if (result.token != null)
                    {
                        // Gán token trực tiếp vào biến static của App
                        string token = result.token.ToString();
                        App.AuthToken = token;

                        // Kiểm tra token đã được lưu đúng
                        Debug.WriteLine($"Token lưu thành công: {(App.AuthToken != null && App.AuthToken.Length > 10 ? App.AuthToken.Substring(0, 10) + "..." : "Invalid Token")}");

                        // Cấu hình header cho HttpClient
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AuthToken);

                        // Lấy thông tin công ty
                        var companyInfo = await GetCompanyInfo();

                        return new LoginResult
                        {
                            Success = true,
                            CompanyTaxCode = companyInfo.TaxCode,
                            CompanyName = companyInfo.Name
                        };
                    }
                    else
                    {
                        Debug.WriteLine("Token không tồn tại trong kết quả");
                        return new LoginResult
                        {
                            Success = false,
                            ErrorMessage = "Đăng nhập thành công nhưng không nhận được token"
                        };
                    }
                }
                else
                {
                    string errorMessage = "Đăng nhập thất bại";
                    if (result != null && result.message != null)
                    {
                        errorMessage = result.message.ToString();
                    }
                    MessageBox.Show(errorMessage, "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);

                    return new LoginResult
                    {
                        Success = false,
                        ErrorMessage = errorMessage
                    };
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi đăng nhập: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Debug.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

                return new LoginResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        // Thêm class để chứa thông tin công ty
        private class CompanyInfo
        {
            public string TaxCode { get; set; }
            public string Name { get; set; }
        }

        private async Task<CompanyInfo> GetCompanyInfo()
        {
            try
            {
                // Cấu hình header với token xác thực
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", App.AuthToken);

                // Gọi API lấy thông tin đơn vị
                var response = await client.GetAsync("https://hoadondientu.gdt.gov.vn:30000/security-taxpayer/profile");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    // THÊM DEBUG ĐỂ XEM API TRẢ VỀ GÌ
                    Debug.WriteLine("=== PROFILE API RESPONSE ===");
                    Debug.WriteLine(content);
                    Debug.WriteLine("============================");

                    var profileData = JsonConvert.DeserializeObject<dynamic>(content);

                    if (profileData != null)
                    {
                        string companyName = profileData.name?.ToString() ?? "Không xác định";
                        string taxCode = profileData.id?.ToString() ??
                                profileData.groupId?.ToString() ??
                                profileData.username?.ToString() ??  // Backup: username cũng là MST
                                "Unknown";

                        lblTenDV.Text = companyName;

                        Debug.WriteLine($"Thông tin công ty - Tên: {companyName}, MST: {taxCode}");

                        return new CompanyInfo
                        {
                            TaxCode = taxCode,
                            Name = companyName
                        };
                    }
                }

                // Fallback nếu không lấy được thông tin
                return new CompanyInfo
                {
                    TaxCode = "DEFAULT_MST",
                    Name = "Công ty không xác định"
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy thông tin công ty: {ex.Message}");
                return new CompanyInfo
                {
                    TaxCode = "DEFAULT_MST",
                    Name = "Lỗi lấy thông tin công ty"
                };
            }
        }

        private async Task InitializeDatabaseForCompany(string companyTaxCode)
        {
            try
            {
                Debug.WriteLine($"Đang khởi tạo database cho công ty MST: {companyTaxCode}");

                // Hiển thị trạng thái
                btnLogin.Content = "ĐANG KHỞI TẠO DB...";

                // Khởi tạo database cho công ty này
                await Task.Run(() =>
                {
                    databaseService.InitializeDatabaseForUser(companyTaxCode);
                });

                Debug.WriteLine($"Khởi tạo database thành công cho MST: {companyTaxCode}");

                // Kiểm tra kết nối database
                bool dbConnected = await Task.Run(() =>
                {
                    try
                    {
                        // Test connection bằng cách thử kiểm tra một bảng
                        return databaseService.InvoiceHeaderExists("test_id", true) || true; // Luôn trả về true nếu không có lỗi
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi test connection: {ex.Message}");
                        return false;
                    }
                });

                if (dbConnected)
                {
                    Debug.WriteLine("Kết nối database thành công");
                }
                else
                {
                    throw new Exception("Không thể kết nối đến database");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khởi tạo database: {ex.Message}");
                MessageBox.Show($"Lỗi khởi tạo cơ sở dữ liệu: {ex.Message}", "Lỗi Database", MessageBoxButton.OK, MessageBoxImage.Error);
                throw; // Re-throw để xử lý ở level cao hơn
            }
        }

        // Thêm method để cleanup khi đóng cửa sổ
        protected override void OnClosed(EventArgs e)
        {
            try
            {
                // Đóng kết nối database
                databaseService?.Dispose();
                client?.Dispose();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi cleanup: {ex.Message}");
            }
            base.OnClosed(e);
        }

        // === CÁC PHƯƠNG THỨC KHÁC GIỮ NGUYÊN ===

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            if (txtPassword.Visibility == Visibility.Visible)
            {
                // Chuyển từ ẩn sang hiển thị
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                btnShowPassword.Content = "🔒";
            }
            else
            {
                // Chuyển từ hiển thị sang ẩn
                txtPassword.Password = txtPasswordVisible.Text;
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                btnShowPassword.Content = "👁️";
            }
        }

        private void btnRefreshCaptcha_Click(object sender, RoutedEventArgs e)
        {
            // Set cursor to wait while fetching new captcha
            Cursor = Cursors.Wait;

            // Clear the current captcha text
            txtCaptcha.Text = string.Empty;

            try
            {
                // Get a new captcha
                GetCaptcha();

                // Set focus to the captcha input field
                txtCaptcha.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi làm mới captcha: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                Debug.WriteLine($"Lỗi khi làm mới captcha: {ex.Message}");
            }
            finally
            {
                // Reset cursor back to default
                Cursor = Cursors.Arrow;
            }
        }

        private void txtUsername_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Hiển thị danh sách người dùng đã lưu nếu có nhập liệu
            if (!string.IsNullOrEmpty(txtUsername.Text))
            {
                lsbUsers.Items.Clear();

                foreach (var user in savedUsers)
                {
                    if (user.Username.Contains(txtUsername.Text))
                    {
                        lsbUsers.Items.Add(user);
                    }
                }

                if (lsbUsers.Items.Count > 0)
                {
                    lsbUsers.Visibility = Visibility.Visible;
                    lsbUsers.Height = Math.Min(lsbUsers.Items.Count * 25, 150);
                }
                else
                {
                    lsbUsers.Visibility = Visibility.Collapsed;
                }
            }
            else
            {
                lsbUsers.Visibility = Visibility.Collapsed;
            }
        }

        private void lsbUsers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lsbUsers.SelectedItem != null)
            {
                var selectedUser = (UserCredential)lsbUsers.SelectedItem;
                txtUsername.Text = selectedUser.Username;

                if (txtPassword.Visibility == Visibility.Visible)
                {
                    txtPassword.Password = selectedUser.Password;
                }
                else
                {
                    txtPasswordVisible.Text = selectedUser.Password;
                }

                lblTenDV.Text = selectedUser.CompanyName;
                lsbUsers.Visibility = Visibility.Collapsed;
            }
        }

        private void lsbUsers_MouseLeave(object sender, MouseEventArgs e)
        {
            lsbUsers.Visibility = Visibility.Collapsed;
        }

        private void lblCapNhatMK_Click(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(txtUsername.Text))
            {
                MessageBox.Show("Tên đăng nhập không được để trống.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtUsername.Focus();
                return;
            }

            // TODO: Mở cửa sổ cập nhật mật khẩu
            MessageBox.Show("Chức năng cập nhật mật khẩu sẽ được triển khai sau.", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSavedUsers()
        {
            try
            {
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoaDonDienTu");
                string filePath = Path.Combine(appDataPath, "users.json");

                if (File.Exists(filePath))
                {
                    string jsonData = File.ReadAllText(filePath);
                    savedUsers = JsonConvert.DeserializeObject<List<UserCredential>>(jsonData) ?? new List<UserCredential>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi nạp danh sách người dùng: {ex.Message}");
                savedUsers = new List<UserCredential>();
            }
        }

        private void SaveUserCredentials(string username, string password, string companyName)
        {
            try
            {
                // Kiểm tra xem người dùng đã tồn tại chưa
                var existingUser = savedUsers.Find(u => u.Username == username);
                if (existingUser != null)
                {
                    // Cập nhật thông tin
                    existingUser.Password = password;
                    existingUser.CompanyName = companyName;
                }
                else
                {
                    // Thêm mới
                    savedUsers.Add(new UserCredential
                    {
                        Username = username,
                        Password = password,
                        CompanyName = companyName
                    });
                }

                // Lưu vào file
                string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "HoaDonDienTu");

                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                string filePath = Path.Combine(appDataPath, "users.json");
                string jsonData = JsonConvert.SerializeObject(savedUsers);
                File.WriteAllText(filePath, jsonData);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lưu thông tin đăng nhập: {ex.Message}");
            }
        }

        private async void GetCaptcha()
        {
            try
            {
                // Gọi API lấy captcha
                var response = await client.GetAsync("https://hoadondientu.gdt.gov.vn:30000/captcha");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var captchaData = JsonConvert.DeserializeObject<CaptchaResponse>(content);

                    if (captchaData != null)
                    {
                        captchaKey = captchaData.Key;

                        // Lưu nội dung SVG vào file tạm
                        string tempPath = Path.Combine(Path.GetTempPath(), "captcha.svg");
                        File.WriteAllText(tempPath, captchaData.Content);

                        // Hiển thị captcha
                        wbCaptcha.Navigate(new Uri(tempPath));

                        // Cố gắng tự động phát hiện captcha
                        txtCaptcha.Text = AutoDetectCaptcha(captchaData.Content);
                    }
                }
                else
                {
                    MessageBox.Show($"Lỗi khi lấy captcha: {response.StatusCode}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy captcha: {ex.Message}");
                MessageBox.Show("Không thể kết nối đến máy chủ. Vui lòng kiểm tra kết nối internet và thử lại sau.",
                                "Lỗi kết nối", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string AutoDetectCaptcha(string svgContent)
        {
            try
            {
                // Phương pháp đơn giản để trích xuất text từ SVG
                if (string.IsNullOrEmpty(svgContent))
                    return string.Empty;

                // Tìm text trong SVG
                string result = string.Empty;
                int startIndex = 0;

                while (true)
                {
                    // Tìm thẻ text
                    int textTagIndex = svgContent.IndexOf("<text", startIndex);
                    if (textTagIndex == -1)
                        break;

                    // Tìm đến đóng thẻ mở
                    int closeTagIndex = svgContent.IndexOf(">", textTagIndex);
                    if (closeTagIndex == -1)
                        break;

                    // Tìm đến mở thẻ đóng
                    int closeTextTagIndex = svgContent.IndexOf("</text>", closeTagIndex);
                    if (closeTextTagIndex == -1)
                        break;

                    // Lấy nội dung text
                    string textContent = svgContent.Substring(closeTagIndex + 1, closeTextTagIndex - closeTagIndex - 1);
                    result += textContent.Trim();

                    // Cập nhật vị trí bắt đầu tìm kiếm
                    startIndex = closeTextTagIndex + 7; // 7 là độ dài của "</text>"
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi phát hiện captcha: {ex.Message}");
                return string.Empty;
            }
        }

        private bool CheckRequiredLibraries()
        {
            try
            {
                // Kiểm tra thư viện WindowsAPICodePack
                var type = Type.GetType("Microsoft.WindowsAPICodePack.Shell.CommonOpenFileDialog, Microsoft.WindowsAPICodePack.Shell");
                if (type == null)
                {
                    Debug.WriteLine("Không tìm thấy thư viện Microsoft.WindowsAPICodePack.Shell");
                    MessageBox.Show("Thiếu thư viện cần thiết: Microsoft.WindowsAPICodePack.Shell",
                                   "Lỗi thư viện", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                Debug.WriteLine("Đã tìm thấy tất cả các thư viện cần thiết");
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi kiểm tra thư viện: {ex.Message}");
                MessageBox.Show($"Lỗi khi kiểm tra thư viện: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }
    }
}