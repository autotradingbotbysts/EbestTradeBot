using EbestTradeBot.Shared.Models.Log;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.Log
{
    public interface ILogService
    {
        Task WriteLog(LogModel model);
    }
}
