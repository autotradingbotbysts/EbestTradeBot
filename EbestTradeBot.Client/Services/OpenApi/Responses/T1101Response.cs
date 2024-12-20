using System.Text.Json.Serialization;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class T1101Response
    {
        [JsonPropertyName("t1101OutBlock")]
        public T1101OutBlock T1101OutBlock = new();
        [JsonPropertyName("rsp_cd")]
        public string RspCd { get; set; } = string.Empty;
        [JsonPropertyName("rsp_msg")]
        public string RspMsg { get; set; } = string.Empty;
    }
    public class T1101OutBlock
    {
        [JsonPropertyName("hname")]
        public string Hname { get; set; } = string.Empty;
        [JsonPropertyName("price")]
        public int Price { get; set; } = int.MinValue;
        [JsonPropertyName("sign")]
        public string Sign { get; set; } = string.Empty;
        [JsonPropertyName("diff")]
        public string Diff { get; set; } = string.Empty;
        [JsonPropertyName("volume")]
        public int Volume { get; set; } = int.MinValue;
    }
}
