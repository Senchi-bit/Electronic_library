namespace Library.Exceptions;

public class BaseException : Exception
{
    public int StatusCode { get; private set; }
    public BaseException(int statusCode, string? message) : base(message)
    {
        this.StatusCode = statusCode;
    }
}