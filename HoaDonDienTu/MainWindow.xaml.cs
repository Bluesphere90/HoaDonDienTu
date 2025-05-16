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

namespace HoaDonDienTu
{
    public partial class MainWindow : Window
    {
        private List<UserCredential> savedUsers = new List<UserCredential>();
        private string captchaKey = string.Empty;
        private HttpClient client;

        public MainWindow()
        {
            InitializeComponent();

            // Cấu hình HttpClient
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");

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
                bool success = await LoginAsync(txtUsername.Text, password, txtCaptcha.Text, captchaKey);

                if (success)
                {
                    lblThanhCong.Visibility = Visibility.Visible;
                    lblThatBai.Visibility = Visibility.Collapsed;

                    // Lưu thông tin đăng nhập
                    SaveUserCredentials(txtUsername.Text, password, lblTenDV.Text);

                    // Chuyển sang cửa sổ quản lý hóa đơn sau 1 giây
                    await Task.Delay(1000);

                    if (success)
                    {
                        lblThanhCong.Visibility = Visibility.Visible;
                        lblThatBai.Visibility = Visibility.Collapsed;

                        // Lưu thông tin đăng nhập
                        SaveUserCredentials(txtUsername.Text, password, lblTenDV.Text);

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

                            Debug.WriteLine("Đang mở cửa sổ InvoiceWindow");
                            // Mở cửa sổ quản lý hóa đơn
                            InvoiceWindow invoiceWindow = new InvoiceWindow();
                            Debug.WriteLine("Đã khởi tạo InvoiceWindow thành công");
                            invoiceWindow.Show();
                            Debug.WriteLine("Đã hiển thị InvoiceWindow");

                            // Đóng cửa sổ đăng nhập
                            this.Close();
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

        private async Task<bool> LoginAsync(string username, string password, string captchaValue, string captchaKey)
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
                        // Lưu token authentication
                        App.AuthToken = result.token.ToString();
                        Debug.WriteLine($"Token lưu thành công: {App.AuthToken.Substring(0, Math.Min(App.AuthToken.Length, 10))}...");

                        // Lấy tên đơn vị
                        await GetCompanyName();

                        return true;
                    }
                    else
                    {
                        Debug.WriteLine("Token không tồn tại trong kết quả");
                        MessageBox.Show("Đăng nhập thành công nhưng không nhận được token", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return false;
                    }
                }
                else
                {
                    if (result != null && result.message != null)
                    {
                        MessageBox.Show(result.message.ToString(), "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    return false;
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
                MessageBox.Show($"Lỗi khi đăng nhập: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        private async Task GetCompanyName()
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
                    var profileData = JsonConvert.DeserializeObject<dynamic>(content);

                    if (profileData != null && profileData.name != null)
                    {
                        lblTenDV.Text = profileData.name.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi lấy thông tin công ty: {ex.Message}");
            }
        }
    }
}