using System;
using Newtonsoft.Json;

namespace HoaDonDienTu
{
    public class CaptchaResponse
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }
    }
}
