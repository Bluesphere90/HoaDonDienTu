using System;

namespace HoaDonDienTu
{
    public class UserCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }

        public override string ToString()
        {
            return Username;
        }
    }
}