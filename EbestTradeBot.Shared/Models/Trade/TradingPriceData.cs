using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Shared.Models.Trade
{
    public class TradingPriceData
    {
        public string Date { get; set; } = string.Empty;
        public string Hname { get; set; } = string.Empty;
        public string Shcode { get; set; } = string.Empty;
        public int 익절가 { get; set; } = int.MinValue;
        public int 손절가 { get; set; } = int.MinValue;
        public int 매수가_1차 { get; set; } = int.MinValue;
        public int 매수가_2차 { get; set; } = int.MinValue;
    }
    public class TradingPriceDataMap : ClassMap<TradingPriceData>
    {
        public TradingPriceDataMap()
        {
            Map(m => m.Date).Name("Date");
            Map(m => m.Hname).Name("Hname");
            Map(m => m.Shcode).Name("Shcode");
            Map(m => m.익절가).Name("익절가");
            Map(m => m.손절가).Name("손절가");
            Map(m => m.매수가_1차).Name("매수가_1차");
            Map(m => m.매수가_2차).Name("매수가_2차");
        }
    }

}
