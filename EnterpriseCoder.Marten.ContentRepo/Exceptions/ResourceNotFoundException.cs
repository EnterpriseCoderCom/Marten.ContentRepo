namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when a requested resource cannot be found.
/// </summary>
/// <remarks>
/// This exception is specifically tailored for use in scenarios where a resource such as a file or data item is not located within its designated bucket.
/// It provides contextual information about the bucket and the path to the resource that was sought but not found, aiding in debugging and error handling.
/// </remarks>
public class ResourceNotFoundException : Exception
{
    /// <summary>
    /// Constructor method used to create the exception.
    /// </summary>
    /// <param name="bucketName">The name of the bucket that caused the exception.</param>
    /// <param name="resourcePath">The name of the resource path that was not found.</param>
    public ResourceNotFoundException(string bucketName, string resourcePath) : base(
        $"{bucketName}: {resourcePath} was not found")
    {
    }
}