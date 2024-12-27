namespace EbestTradeBot.Shared.Models.Log
{
    public class LogModel
    {
        public DateTime Date { get; set; } = DateTime.Now;
        public string StockName { get; set; } = string.Empty;
        public string StockCode { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
    }
}
