namespace EbestTradeBot.Client.Services.OpenApi.Responses
{
    using System.Text.Json.Serialization;

    public class CSPAT00601OutBlock2
    {
        [JsonPropertyName("OrdAmt")]
        public int OrdAmt { get; set; } = int.MinValue;

        [JsonPropertyName("CvrgSeqno")]
        public int CvrgSeqno { get; set; } = int.MinValue;

        [JsonPropertyName("RuseOrdQty")]
        public int RuseOrdQty { get; set; } = int.MinValue;

        [JsonPropertyName("MgempNo")]
        public string MgempNo { get; set; } = string.Empty;

        [JsonPropertyName("SpareOrdNo")]
        public int SpareOrdNo { get; set; } = int.MinValue;

        [JsonPropertyName("OrdNo")]
        public int OrdNo { get; set; } = int.MinValue;

        [JsonPropertyName("SpotOrdQty")]
        public int SpotOrdQty { get; set; } = int.MinValue;

        [JsonPropertyName("SubstOrdAmt")]
        public int SubstOrdAmt { get; set; } = int.MinValue;

        [JsonPropertyName("OrdMktCode")]
        public string OrdMktCode { get; set; } = string.Empty;

        [JsonPropertyName("IsuNm")]
        public string IsuNm { get; set; } = string.Empty;

        [JsonPropertyName("ShtnIsuNo")]
        public string ShtnIsuNo { get; set; } = string.Empty;

        [JsonPropertyName("RuseOrdAmt")]
        public int RuseOrdAmt { get; set; } = int.MinValue;

        [JsonPropertyName("RecCnt")]
        public int RecCnt { get; set; } = int.MinValue;

        [JsonPropertyName("OrdTime")]
        public string OrdTime { get; set; } = string.Empty;

        [JsonPropertyName("MnyOrdAmt")]
        public int MnyOrdAmt { get; set; } = int.MinValue;

        [JsonPropertyName("AcntNm")]
        public string AcntNm { get; set; } = string.Empty;

        [JsonPropertyName("RsvOrdNo")]
        public int RsvOrdNo { get; set; } = int.MinValue;

        [JsonPropertyName("OrdPtnCode")]
        public string OrdPtnCode { get; set; } = string.Empty;
    }

    public class CSPAT00601OutBlock1
    {
        [JsonPropertyName("BnsTpCode")]
        public string BnsTpCode { get; set; } = string.Empty;

        [JsonPropertyName("InptPwd")]
        public string InptPwd { get; set; } = string.Empty;

        [JsonPropertyName("CommdaCode")]
        public string CommdaCode { get; set; } = string.Empty;

        [JsonPropertyName("StslAbleYn")]
        public string StslAbleYn { get; set; } = string.Empty;

        [JsonPropertyName("MbrNo")]
        public string MbrNo { get; set; } = string.Empty;

        [JsonPropertyName("OrdSeqNo")]
        public int OrdSeqNo { get; set; } = int.MinValue;

        [JsonPropertyName("StslOrdprcTpCode")]
        public string StslOrdprcTpCode { get; set; } = string.Empty;

        [JsonPropertyName("PtflNo")]
        public int PtflNo { get; set; } = int.MinValue;

        [JsonPropertyName("GrpId")]
        public string GrpId { get; set; } = string.Empty;

        [JsonPropertyName("MgntrnCode")]
        public string MgntrnCode { get; set; } = string.Empty;

        [JsonPropertyName("OrdPrc")]
        public string OrdPrc { get; set; } = string.Empty;

        [JsonPropertyName("TrchNo")]
        public int TrchNo { get; set; } = int.MinValue;

        [JsonPropertyName("PrgmOrdprcPtnCode")]
        public string PrgmOrdprcPtnCode { get; set; } = string.Empty;

        [JsonPropertyName("BskNo")]
        public int BskNo { get; set; } = int.MinValue;

        [JsonPropertyName("StrtgCode")]
        public string StrtgCode { get; set; } = string.Empty;

        [JsonPropertyName("OrdQty")]
        public int OrdQty { get; set; } = int.MinValue;

        [JsonPropertyName("RecCnt")]
        public int RecCnt { get; set; } = int.MinValue;

        [JsonPropertyName("OrdprcPtnCode")]
        public string OrdprcPtnCode { get; set; } = string.Empty;

        [JsonPropertyName("IsuNo")]
        public string IsuNo { get; set; } = string.Empty;

        [JsonPropertyName("ItemNo")]
        public int ItemNo { get; set; } = int.MinValue;

        [JsonPropertyName("OpDrtnNo")]
        public string OpDrtnNo { get; set; } = string.Empty;

        [JsonPropertyName("AcntNo")]
        public string AcntNo { get; set; } = string.Empty;

        [JsonPropertyName("LoanDt")]
        public string LoanDt { get; set; } = string.Empty;

        [JsonPropertyName("OrdCndiTpCode")]
        public string OrdCndiTpCode { get; set; } = string.Empty;

        [JsonPropertyName("CvrgTpCode")]
        public string CvrgTpCode { get; set; } = string.Empty;

        [JsonPropertyName("LpYn")]
        public string LpYn { get; set; } = string.Empty;
    }

    public class CSPAT00601Response
    {
        [JsonPropertyName("rsp_cd")]
        public string RspCd { get; set; } = string.Empty;

        [JsonPropertyName("rsp_msg")]
        public string RspMsg { get; set; } = string.Empty;

        [JsonPropertyName("CSPAT00601OutBlock2")]
        public CSPAT00601OutBlock2 CSPAT00601OutBlock2 { get; set; } = new();

        [JsonPropertyName("CSPAT00601OutBlock1")]
        public CSPAT00601OutBlock1 CSPAT00601OutBlock1 { get; set; } = new();
    }

}
