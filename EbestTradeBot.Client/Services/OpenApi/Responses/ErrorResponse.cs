using System.Text.Json.Serialization;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class ErrorResponse
    {
        [JsonPropertyName("rsp_cd")]
        public string RspCd { get; set; } = string.Empty;
        [JsonPropertyName("rsp_msg")]
        public string RspMsg { get; set; } = string.Empty;
    }
}
