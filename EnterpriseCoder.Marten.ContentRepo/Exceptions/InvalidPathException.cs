namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class InvalidPathException : Exception
{
    public InvalidPathException(string? message) : base(message)
    {
    }
}