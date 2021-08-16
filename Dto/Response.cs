namespace FunctionBrowserRemote.Dto
{
    public abstract class CodeResponse
    {
        public const string SUCCESS = "00";
        public const string ERROR = "01";
    }

    public class SuccessResponse
    {
        public string Cod = CodeResponse.SUCCESS;
        public string Timer { get; set; }
        public string Version { get; set; }
        public string Date { get; set; }
        public string Response { get; set; }
    }

    public class BadResponse
    {
        public string Cod = CodeResponse.ERROR;
        public string Mesage { get; set; }
    }
}
