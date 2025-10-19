namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when a requested directory cannot be found.
/// </summary>
/// <remarks>
/// This exception is raised when attempting to perform operations (such as deletion or modification)
/// on a directory that does not exist in the specified bucket.
/// </remarks>
public class DirectoryNotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryNotFoundException"/> class.
    /// </summary>
    /// <param name="bucketId">The ID of the bucket where the directory was expected to exist.</param>
    /// <param name="directoryPath">The path of the directory that was not found.</param>
    public DirectoryNotFoundException(Guid bucketId, string directoryPath)
        : base($"Directory not found at path '{directoryPath}' in bucket '{bucketId}'.")
    {
        BucketId = bucketId;
        DirectoryPath = directoryPath;
    }

    /// <summary>
    /// Gets the ID of the bucket where the directory was expected to exist.
    /// </summary>
    public Guid BucketId { get; }

    /// <summary>
    /// Gets the path of the directory that was not found.
    /// </summary>
    public string DirectoryPath { get; }
}
