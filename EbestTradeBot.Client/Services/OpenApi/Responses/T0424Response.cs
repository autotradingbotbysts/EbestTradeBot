using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class T0424Response
    {
        [JsonPropertyName("t0424OutBlock")]
        public T0424OutBlock T0424OutBlock { get; set; } = new();
        [JsonPropertyName("t0424OutBlock1")]
        public List<T0424OutBlock1> T0424OutBlock1 { get; set; } = [];
    }

    public class T0424OutBlock
    {

    }

    public class T0424OutBlock1
    {
        [JsonPropertyName("expcode")]
        public string Expcode { get; set; } = string.Empty;
        [JsonPropertyName("mdposqt")]
        public int Mdposqt { get; set; } = int.MinValue;
        [JsonPropertyName("hname")]
        public string Hname { get; set; } = string.Empty;
    }
}
