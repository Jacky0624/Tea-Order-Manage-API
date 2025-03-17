namespace TeaAPI.Exceptions
{
    public class CommonException : Exception
    {
        public string Message { get; private set; }
        public CommonException(string message) 
        {
            Message = message;
        }
    }
}
