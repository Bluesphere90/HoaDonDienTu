using System;
using System.Windows;

namespace HoaDonDienTu
{
    public partial class App : Application
    {
        // Token xác thực được sử dụng xuyên suốt ứng dụng
        public static string AuthToken { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Xử lý ngoại lệ không được bắt trong ứng dụng
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Exception ex = (Exception)args.ExceptionObject;
                MessageBox.Show($"Đã xảy ra lỗi không mong muốn: {ex.Message}\r\n{ex.StackTrace}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Xử lý ngoại lệ không được bắt trong luồng UI
            this.DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Đã xảy ra lỗi: {args.Exception.Message}",
                                "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}