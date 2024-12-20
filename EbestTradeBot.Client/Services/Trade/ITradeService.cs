using System;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.Trade
{
    public interface ITradeService
    {
        Task StartTrade();
        Task StopTrade();

        event EventHandler WriteLog;
        event EventHandler StopTradeEvent;
    }
}
