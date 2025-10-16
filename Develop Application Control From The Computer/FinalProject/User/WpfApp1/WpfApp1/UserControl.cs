using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfApp1
{
    public class UserControl
    {
        public static int ID_Player = -1 ; 
        // ===================== CLASS NGƯỜI CHƠI =====================
        [JsonProperty("name")]
        public string name { get; set; } = string.Empty;
        [JsonProperty("score")]
        public int score { get; set; } = 0;
        [JsonProperty("hasPlayed")]
        public bool hasPlayed { get; set; } = false;
        [JsonProperty("currentAnswer")]
        public string currentAnswer { get; set; } = string.Empty;
        [JsonProperty("lastAnswerTime")]
        public long lastAnswerTime { get; set; } = 0;
    }
}
