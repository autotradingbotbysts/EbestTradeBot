using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EbestTradeBot.Shared.Models.Trade
{
    public class Stock
    {
        public string Shcode { get; set; } = string.Empty;
        public string Hname { get; set; } = string.Empty;
        public int 손절가 { get; set; } = int.MinValue;
        public int 익절가 { get; set; } = int.MinValue;
        public int 매수가_1차 { get; set; } = int.MinValue;
        public int 매수가_2차 { get; set; } = int.MinValue;
        public int 보유량 { get; set; } = int.MinValue;
        public int 현재가 { get; set; } = int.MinValue;

        public Stock()
        {
        }

        public Stock(Stock other)
        {
            ArgumentNullException.ThrowIfNull(other);

            Shcode = other.Shcode;
            Hname = other.Hname;
            손절가 = other.손절가;
            익절가 = other.익절가;
            매수가_1차 = other.매수가_1차;
            매수가_2차 = other.매수가_2차;
            보유량 = other.보유량;
            현재가 = other.현재가;
        }

        // 깊은 복사를 위한 Clone 메서드
        public Stock Clone()
        {
            return new Stock(this);
        }
    }
}
