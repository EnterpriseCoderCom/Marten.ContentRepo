namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when an attempt to overwrite a resource is not permitted.
/// </summary>
public class OverwriteNotPermittedException : Exception
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bucketName">The name of the bucket referenced in the attempted overwrite operation.</param>
    /// <param name="resourceName">The name of the resource referenced in the attempted overwrite operation.</param>
    public OverwriteNotPermittedException(string bucketName, string resourceName) : base(
        $"Unable to overwrite resource {bucketName}: {resourceName}")
    {
    }
}