using System.Text.Json.Serialization;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class RevokeResponse
    {
        [JsonPropertyName("code")]
        public int Code { get; set; } = int.MinValue;
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;
    }
}
