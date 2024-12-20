using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class T1305Response
    {
        [JsonPropertyName("t1305OutBlock")]
        public T1305OutBlock T1305OutBlock { get; set; } = new();
        [JsonPropertyName("t1305OutBlock1")]
        public List<T1305OutBlock1> T1305OutBlock1 { get; set; } = [];
    }

    public class T1305OutBlock
    {
        [JsonPropertyName("cnt")]
        public int Cnt { get; set; } = int.MinValue;
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;
        [JsonPropertyName("idx")]
        public int Idx { get; set; } = int.MinValue;
    }

    public class T1305OutBlock1
    {
        [JsonPropertyName("date")]
        public string Date { get; set; } = string.Empty;
        [JsonPropertyName("open")]
        public int Open { get; set; } = int.MinValue;
        [JsonPropertyName("high")]
        public int High { get; set; } = int.MinValue;
        [JsonPropertyName("low")]
        public int Low { get; set; } = int.MinValue;
        [JsonPropertyName("close")]
        public int Close { get; set; } = int.MinValue;
        [JsonPropertyName("diff")]
        public string Diff { get; set; } = string.Empty; // 정의서에는 Number인데 왜 문자열로 넘어오니 
    }
}
