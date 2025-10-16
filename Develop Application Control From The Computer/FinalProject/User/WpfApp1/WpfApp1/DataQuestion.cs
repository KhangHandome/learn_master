using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class DataQuestion
    {
        // ===== CLASS MÔ TẢ LỰA CHỌN =====
        public class LuaChon
        {
            /*Use this to matching with json file on firebase */
            [JsonProperty("A")]
            public string A { get; set; } = string.Empty;

            [JsonProperty("B")]
            public string B { get; set; } = string.Empty;

            [JsonProperty("C")]
            public string C { get; set; } = string.Empty;

            [JsonProperty("D")]
            public string D { get; set; } = string.Empty;
        }

        // ===== CLASS MÔ TẢ CÂU HỎI =====
        public class CauHoi
        {
            [JsonProperty("answer")]
            public string DapAn { get; set; } = string.Empty;

            [JsonProperty("question")]
            public string NoiDung { get; set; } = string.Empty;

            [JsonProperty("options")]
            public LuaChon LuaChon { get; set; } = new LuaChon();
        }
    }
}
