namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when a bucket cannot be deleted because it is not empty.
/// </summary>
public class BucketNotEmptyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BucketNotEmptyException"/> class with the name of the bucket and the resource within the bucket that prevents deletion.
    /// </summary>
    /// <param name="bucketName">The name of the bucket that cannot be deleted because it is not empty.</param>
    /// <param name="resourceName">The name of the resource within the bucket that cannot be deleted.</param>
    public BucketNotEmptyException(string bucketName, string resourceName) : base($"{bucketName}: {resourceName} cannot be deleted")
    {
    }
}