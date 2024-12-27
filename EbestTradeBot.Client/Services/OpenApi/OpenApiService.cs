using EbestTradeBot.Client.Services.OpenApi.Responses;
using EbestTradeBot.Shared.Exceptions;
using EbestTradeBot.Shared.Models.Trade;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi
{
    public class OpenApiService(
        IOptionsMonitor<OpenApiOptions> openApiOptionsMonitor
        ) : IOpenApiService
    {
        private readonly OpenApiOptions _openApiOptions = openApiOptionsMonitor.CurrentValue;

        private readonly string _url = $"https://openapi.ls-sec.co.kr:8080";
        private readonly string _tokenRevokePath = "/oauth2/revoke";
        private readonly string _tokenPath = "/oauth2/token";
        private readonly string _orderPath = "/stock/order";
        private readonly string _accnoPath = "/stock/accno";
        private readonly string _marketDataPath = "/stock/market-data";

        private string _token = string.Empty;

        private static readonly SemaphoreSlim _t1101Semaphore = new(1, 1);
        private static readonly SemaphoreSlim _t1305Semaphore = new(1, 1);
        private static readonly SemaphoreSlim _CSPAT00601Semaphore = new(1, 1);

        private static readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        };

        public async Task<List<Stock>> GetAccountStocks(CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "t0424"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            var jsonString = JsonSerializer.Serialize(new
            {
                t0424InBlock = new
                {
                    prcgb = "",
                    chegb = "",
                    dangb = "",
                    charge = "",
                    cts_expcode = "",
                }
            });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_url}{_accnoPath}", content, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return [];

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                ThrowErrorByMessage(errorMessage);
            }
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return [];

            var responseData = JsonSerializer.Deserialize<T0424Response>(responseString) ?? throw new Exception($"{responseString}");
            content.Dispose();
            response.Dispose();
            var stocks = new List<Stock>();
            foreach (var data in responseData.T0424OutBlock1)
            {
                stocks.Add(new Stock
                {
                    Shcode = data.Expcode,
                    Hname = data.Hname,
                    보유량 = data.Mdposqt,
                });
            }

            return stocks;
        }

        public async Task GetCurrentPrice(List<Stock> stocks, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "t1101"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            foreach (var stock in stocks)
            {
                try
                {
                    await _t1101Semaphore.WaitAsync();
                    if (cancellationToken.IsCancellationRequested) return;

                    var jsonString = JsonSerializer.Serialize(new
                    {
                        t1101InBlock = new
                        {
                            shcode = stock.Shcode
                        }
                    });
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

                    var response = await client.PostAsync($"{_url}{_marketDataPath}", content, cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                        ThrowErrorByMessage(errorMessage);
                    }

                    string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;

                    var currentPrice = JsonSerializer.Deserialize<T1101Response>(responseString) ?? throw new Exception($"{responseString}");
                    var node = JsonNode.Parse(responseString);
                    stock.현재가 = node?["t1101OutBlock"]?["price"]?.GetValue<int>() ?? int.MinValue;

                    content.Dispose();
                    response.Dispose();
                }
                finally
                {
                    await Task.Delay(400);
                    _t1101Semaphore.Release();
                }
            }
        }

        public async Task SetTradingPrice(List<Stock> stocks, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "t1305"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            foreach (var stock in stocks)
            {
                try
                {
                    await _t1305Semaphore.WaitAsync();
                    var jsonString = JsonSerializer.Serialize(new
                    {
                        t1305InBlock = new
                        {
                            shcode = stock.Shcode,
                            dwmcode = 1,
                            date = "",
                            idx = 0,
                            cnt = _openApiOptions.DayCount
                        }
                    });
                    var content = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync($"{_url}{_marketDataPath}", content, cancellationToken);
                    if(cancellationToken.IsCancellationRequested) return;

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                        ThrowErrorByMessage(errorMessage);
                    }

                    var responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (cancellationToken.IsCancellationRequested) return;
                    var t1305Response = JsonSerializer.Deserialize<T1305Response>(responseString) ?? throw new Exception($"{responseString}");
                    
                    CalcPrice(stock, t1305Response.T1305OutBlock1);
                    
                }
                finally
                {
                    await Task.Delay(1100);
                    _t1305Semaphore.Release();
                }
            }
        }

        public async Task<CSPAT00601Response> BuyStock(string shcode, int count, bool isTest, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "CSPAT00601"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            if(isTest)
            {
                shcode = $"A{shcode}";
            }

            var jsonString = JsonSerializer.Serialize(new
            {
                CSPAT00601InBlock1 = new
                {
                    IsuNo = shcode, // 종목번호
                    OrdQty = count, // 주문수량
                    OrdPrc = 0, // 주문가
                    BnsTpCode = "2", // 매매구분 (2: 매수)
                    OrdprcPtnCode = "03", // 호가유형코드
                    MgntrnCode = "000", // 신용거래코드
                    LoanDt = "", // 대출일
                    OrdCndiTpCode = "0" // 주문조건구분
                }
            });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            try
            {
                await _CSPAT00601Semaphore.WaitAsync();
                if (cancellationToken.IsCancellationRequested) return new();

                var response = await client.PostAsync($"{_url}{_orderPath}", content, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return new();

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                    ThrowErrorByMessage(errorMessage);
                }

                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return new();

                var orderResponseData = JsonSerializer.Deserialize<CSPAT00601Response>(responseString) ?? throw new Exception($"{responseString}");

                content.Dispose();
                response.Dispose();

                return orderResponseData;
            }
            finally
            {
                await Task.Delay(200);
                _CSPAT00601Semaphore.Release();
            }
        }

        public async Task<CSPAT00601Response> SellStock(string shcode, int count, bool isTest, CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "CSPAT00601"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            if (isTest)
            {
                shcode = $"A{shcode}";
            }

            var jsonString = JsonSerializer.Serialize(new
            {
                CSPAT00601InBlock1 = new
                {
                    IsuNo = shcode, // 종목번호
                    OrdQty = count, // 주문수량
                    OrdPrc = 0, // 주문가
                    BnsTpCode = "1", // 매매구분 (1: 매도)
                    OrdprcPtnCode = "03", // 호가유형코드
                    MgntrnCode = "000", // 신용거래코드
                    LoanDt = "", // 대출일
                    OrdCndiTpCode = "0" // 주문조건구분
                }
            });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            try
            {
                await _CSPAT00601Semaphore.WaitAsync();
                if (cancellationToken.IsCancellationRequested) return new();

                var response = await client.PostAsync($"{_url}{_orderPath}", content, cancellationToken);
                if (cancellationToken.IsCancellationRequested) return new();

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                    ThrowErrorByMessage(errorMessage);
                }

                string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
                if (cancellationToken.IsCancellationRequested) return new();

                var orderResponseData = JsonSerializer.Deserialize<CSPAT00601Response>(responseString) ?? throw new Exception($"{responseString}");

                content.Dispose();
                response.Dispose();

                return orderResponseData;
            }
            finally
            {
                await Task.Delay(200);
                _CSPAT00601Semaphore.Release();
            }
        }

        public async Task<TokenResponse> InitToken(CancellationToken cancellationToken)
        {
            var parameters = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "appkey", _openApiOptions.AppKey },
                { "appsecretkey", _openApiOptions.SecretKey },
                { "scope", "oob" },
            };

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

            var content = new FormUrlEncodedContent(parameters);

            var response = await client.PostAsync($"{_url}{_tokenPath}", content, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return new();

            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return new();

            var responseData = JsonSerializer.Deserialize<TokenResponse>(responseString) ?? throw new Exception($"{responseString}");

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                ThrowErrorByMessage(errorMessage);
            }

            _token = responseData.AccessToken;

            return responseData;
        }

        public async Task<RevokeResponse> RevokeToken()
        {
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "appkey", _openApiOptions.AppKey },
                    { "appsecretkey", _openApiOptions.SecretKey },
                    { "token_type_hint", "access_token" },
                    { "token", _token }
                };

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));

                var content = new FormUrlEncodedContent(parameters);

                var response = await client.PostAsync($"{_url}{_tokenRevokePath}", content);

                string responseString = await response.Content.ReadAsStringAsync();
                var responseData = JsonSerializer.Deserialize<RevokeResponse>(responseString) ?? throw new Exception($"{responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    ThrowErrorByMessage(errorMessage);
                }

                return responseData;
            }
            finally
            {
                _token = string.Empty;
            }
        }

        public async Task<List<Stock>> GetTradingStocks(CancellationToken cancellationToken)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.AcceptCharset.Add(new StringWithQualityHeaderValue("utf-8"));
            client.DefaultRequestHeaders.Add("authorization", $"Bearer {_token}"); // OAuth 토큰
            client.DefaultRequestHeaders.Add("tr_cd", "t0425"); // 거래 코드
            client.DefaultRequestHeaders.Add("tr_cont", "N"); // 연속 거래 여부
            client.DefaultRequestHeaders.Add("tr_cont_key", ""); // 연속키
            client.DefaultRequestHeaders.Add("mac_address", ""); // MAC 주소

            var jsonString = JsonSerializer.Serialize(new
            {
                t0425InBlock = new
                {
                    expcode = "",
                    chegb = "2",
                    medosu = "0",
                    sortgb = "1",
                    cts_ordno = "",
                }
            });
            var content = new StringContent(jsonString, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{_url}{_accnoPath}", content, cancellationToken);
            if (cancellationToken.IsCancellationRequested) return [];

            if (!response.IsSuccessStatusCode)
            {
                var errorMessage = await response.Content.ReadAsStringAsync(cancellationToken);
                ThrowErrorByMessage(errorMessage);
            }
            string responseString = await response.Content.ReadAsStringAsync(cancellationToken);
            if (cancellationToken.IsCancellationRequested) return [];

            var responseData = JsonSerializer.Deserialize<T0425Response>(responseString) ?? throw new Exception($"{responseString}");
            content.Dispose();
            response.Dispose();
            var stocks = new List<Stock>();
            foreach (var data in responseData.T0425OutBlock1)
            {
                stocks.Add(new Stock
                {
                    Shcode = data.Expcode,
                });
            }

            return stocks;
        }

        private static void ThrowErrorByMessage(string errorMsg)
        {
            var error = JsonSerializer.Deserialize<ErrorResponse>(errorMsg, _options) ?? throw new Exception("Failed to deserialize error message.");
            var code = error.RspCd;
            var message = error.RspMsg;

            throw code switch
            {
                "IGW00121" => new InvalidTokenException(code, message), // 유효하지 않은 token 입니다.
                "IGW00105" => new ArgumentException($"[{code}] {message}"), // 유효하지 않은 AppSecret입니다.
                _ => new Exception($"[{code}] {message}"),
            };
        }

        private static void CalcPrice(Stock stock, List<T1305OutBlock1> t1305s)
        {
            // 기준봉 구하기
            int count = t1305s.Count;

            stock.익절가 = int.MinValue;
            stock.손절가 = int.MinValue;
            stock.매수가_1차 = int.MinValue;
            stock.매수가_2차 = int.MinValue;

            for (int i = 0; i < t1305s.Count; i++)
            {
                if (double.Parse(t1305s[i].Diff) < 15) continue;

                stock.손절가 = Get손절가(t1305s, i);
                stock.익절가 = Get익절가(t1305s, i);

                break;
            }
            stock.매수가_1차 = (stock.손절가 + stock.익절가) / 2;
            stock.매수가_2차 = (stock.매수가_1차 + stock.손절가) / 2;
            if (stock.매수가_1차 == int.MinValue || stock.익절가 == int.MinValue || stock.손절가 == int.MinValue || stock.매수가_2차 == int.MinValue)
            {
                stock.매수가_1차 = int.MinValue;
                stock.매수가_2차 = int.MinValue;
                stock.익절가 = int.MinValue;
                stock.손절가 = int.MinValue;

                return;
            }
        }

        private static int Get익절가(List<T1305OutBlock1> t1305s, int index)
        {
            int nextIndex = index - 1;
            if(nextIndex < 0)
            {
                return t1305s[index].Close;
            }

            if (double.Parse(t1305s[nextIndex].Diff) > 0)
            {
                if (t1305s[nextIndex].Close > t1305s[index].Close)
                {
                    return Get익절가(t1305s, nextIndex);
                }
                else
                {
                    return t1305s[index].Close;
                }
            }
            else
            {
                if (t1305s[nextIndex].Open > t1305s[index].Close)
                {
                    return Get익절가(t1305s, nextIndex);
                }
                else
                {
                    return t1305s[index].Close;
                }
            }
        }

        private static int Get손절가(List<T1305OutBlock1> t1305s, int index)
        {
            int preIndex = index + 1;
            if (preIndex >= t1305s.Count)
            {
                return t1305s[index].Close;
            }

            if (double.Parse(t1305s[preIndex].Diff) > 0)
            {
                if (t1305s[preIndex].Open < t1305s[index].Open)
                {
                    return Get손절가(t1305s, preIndex);
                }
                else
                {
                    return t1305s[index].Open;
                }
            }
            else
            {
                if (t1305s[preIndex].Close < t1305s[index].Open)
                {
                    return Get손절가(t1305s, preIndex);
                }
                else
                {
                    return t1305s[index].Open;
                }
            }
        }
    }
}
