namespace EbestTradeBot.Shared.Exceptions
{
    public class InvalidTokenException(string code, string message) : Exception
    {
        public string Code { get; set; } = code;
        public override string Message => message;
    }
}
