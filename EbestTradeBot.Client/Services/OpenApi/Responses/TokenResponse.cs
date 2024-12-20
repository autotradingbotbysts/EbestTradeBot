using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
        [JsonPropertyName("expire_in")]
        public string ExpireIn { get; set; } = string.Empty;
        [JsonPropertyName("scope")]
        public string Scope { get; set; } = string.Empty;
        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}
