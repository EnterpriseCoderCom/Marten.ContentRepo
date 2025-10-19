namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

/// <summary>
/// Represents an exception thrown when attempting to create a directory that already exists.
/// </summary>
/// <remarks>
/// This exception is raised when a directory creation operation fails because an explicit
/// directory already exists at the specified path in the given bucket.
/// </remarks>
public class DirectoryAlreadyExistsException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DirectoryAlreadyExistsException"/> class.
    /// </summary>
    /// <param name="bucketId">The ID of the bucket containing the existing directory.</param>
    /// <param name="directoryPath">The path of the directory that already exists.</param>
    public DirectoryAlreadyExistsException(Guid bucketId, string directoryPath)
        : base($"Directory already exists at path '{directoryPath}' in bucket '{bucketId}'.")
    {
        BucketId = bucketId;
        DirectoryPath = directoryPath;
    }

    /// <summary>
    /// Gets the ID of the bucket containing the existing directory.
    /// </summary>
    public Guid BucketId { get; }

    /// <summary>
    /// Gets the path of the directory that already exists.
    /// </summary>
    public string DirectoryPath { get; }
}
