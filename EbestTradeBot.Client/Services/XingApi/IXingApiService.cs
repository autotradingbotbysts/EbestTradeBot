using EbestTradeBot.Shared.Models.Trade;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.XingApi
{
    public interface IXingApiService
    {
        Task<List<Stock>> GetSearchShcodes(bool isTestTrade);
    }
}
