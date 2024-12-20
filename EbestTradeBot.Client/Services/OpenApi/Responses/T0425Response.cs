using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    public class T0425OutBlock
    {
        [JsonPropertyName("tqty")]
        public int Tqty { get; set; } = int.MinValue; // 총주문수량

        [JsonPropertyName("tcheqty")]
        public int Tcheqty { get; set; } = int.MinValue; // 총체결수량

        [JsonPropertyName("tordrem")]
        public int Tordrem { get; set; } = int.MinValue; // 총미체결수량

        [JsonPropertyName("cmss")]
        public int Cmss { get; set; } = int.MinValue; // 주문수수료

        [JsonPropertyName("tamt")]
        public int Tamt { get; set; } = int.MinValue; // 주문금액

        [JsonPropertyName("tmdamt")]
        public int Tmdamt { get; set; } = int.MinValue; // 총매도체결금액

        [JsonPropertyName("tmsamt")]
        public int Tmsamt { get; set; } = int.MinValue; // 총매수체결금액

        [JsonPropertyName("tax")]
        public int Tax { get; set; } = int.MinValue; // 추정체세금

        [JsonPropertyName("cts_ordno")]
        public string Cts_ordno { get; set; } = string.Empty; // 주문번호
    }

    public class T0425OutBlock1
    {
        [JsonPropertyName("ordno")]
        public int Ordno { get; set; } = int.MinValue; // 주문번호

        [JsonPropertyName("expcode")]
        public string Expcode { get; set; } = string.Empty; // 종목번호

        [JsonPropertyName("medosu")]
        public string Medosu { get; set; } = string.Empty; // 구분

        [JsonPropertyName("qty")]
        public int Qty { get; set; } = int.MinValue; // 주문수량

        [JsonPropertyName("price")]
        public int Price { get; set; } = int.MinValue; // 주문가격

        [JsonPropertyName("cheqty")]
        public int Cheqty { get; set; } = int.MinValue; // 체결수량

        [JsonPropertyName("cheprice")]
        public int Cheprice { get; set; } = int.MinValue; // 체결가격

        [JsonPropertyName("ordrem")]
        public int Ordrem { get; set; } = int.MinValue; // 미체결잔량

        [JsonPropertyName("cfmqty")]
        public int Cfmqty { get; set; } = int.MinValue; // 확인수량

        [JsonPropertyName("status")]
        public string Status { get; set; } = string.Empty; // 상태

        [JsonPropertyName("orgordno")]
        public int Orgordno { get; set; } = int.MinValue; // 원주문번호

        [JsonPropertyName("ordgb")]
        public string Ordbgb { get; set; } = string.Empty; // 유형

        [JsonPropertyName("ordtime")]
        public string Ordtime { get; set; } = string.Empty; // 주문시간

        [JsonPropertyName("ordermtd")]
        public string Ordermtd { get; set; } = string.Empty; // 주문매체

        [JsonPropertyName("sysprocseq")]
        public int Sysprocseq { get; set; } = int.MinValue; // 처리순번

        [JsonPropertyName("hogagb")]
        public string Hogagb { get; set; } = string.Empty; // 호가유형

        [JsonPropertyName("price1")]
        public int Price1 { get; set; } = int.MinValue; // 현재가

        [JsonPropertyName("orggb")]
        public string Orgbb { get; set; } = string.Empty; // 주문구분

        [JsonPropertyName("singb")]
        public string Singb { get; set; } = string.Empty; // 신용구분

        [JsonPropertyName("loandt")]
        public string Loandt { get; set; } = string.Empty; // 대출일자
    }

    public class T0425Response
    {
        [JsonPropertyName("t0425OutBlock")]
        public T0425OutBlock T0425OutBlock { get; set; } = new T0425OutBlock();

        [JsonPropertyName("t0425OutBlock1")]
        public List<T0425OutBlock1> T0425OutBlock1 { get; set; } = new List<T0425OutBlock1>();
    }
}
