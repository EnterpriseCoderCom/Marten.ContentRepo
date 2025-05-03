namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when a bucket does not exist.
/// </summary>
public class BucketNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BucketNotFoundException"/> class with the name of the missing bucket.
    /// </summary>
    /// <param name="bucketName">The name of the bucket that was not found.</param>
    public BucketNotFoundException(string bucketName) : base(bucketName)
    {
    }
}