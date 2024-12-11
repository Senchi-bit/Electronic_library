namespace Library.Exceptions;

public class NotFoundException : BaseException
{
    public NotFoundException(int statusCode, string? message) : base(statusCode, message) {}
}