namespace Library.Exceptions;

public class IncorrectDataException : BaseException
{
    public IncorrectDataException(int statusCode, string? message) : base(statusCode, message)
    {
        
    }
}