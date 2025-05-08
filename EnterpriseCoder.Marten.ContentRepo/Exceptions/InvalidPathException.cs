namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when a path is invalid or not found.
/// </summary>
public class InvalidPathException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidPathException"/> class with the specific message indicating that the path is invalid.
    /// </summary>
    /// <param name="message">The error message that explains why the path is invalid.</param>
    public InvalidPathException(string message) : base(message)
    {
    }
}