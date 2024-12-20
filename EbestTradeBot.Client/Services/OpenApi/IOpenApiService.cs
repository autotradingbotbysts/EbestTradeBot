using EbestTradeBot.Client.Services.OpenApi.Responses;
using EbestTradeBot.Shared.Models.Trade;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi
{
    public interface IOpenApiService
    {
        Task<CSPAT00601Response> BuyStock(string shcode, int count, bool isTest, CancellationToken cancellationToken);
        Task<List<Stock>> GetAccountStocks(CancellationToken cancellationToken);
        Task GetCurrentPrice(List<Stock> stocks, CancellationToken cancellationToken);
        Task<List<Stock>> GetTradingStocks(CancellationToken cancellationToken);
        Task<TokenResponse> InitToken(CancellationToken cancellationToken);
        Task<RevokeResponse> RevokeToken();
        Task<CSPAT00601Response> SellStock(string shcode, int count, bool isTest, CancellationToken cancellationToken);
        Task SetTradingPrice(List<Stock> stocks, CancellationToken cancellationToken);
    }
}
