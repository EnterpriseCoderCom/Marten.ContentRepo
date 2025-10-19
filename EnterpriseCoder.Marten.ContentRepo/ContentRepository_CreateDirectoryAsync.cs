using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Creates an empty directory in the specified bucket.
    /// </summary>
    /// <remarks>
    /// Transaction Control: This method adds the directory to the session specified by
    /// <paramref name="documentSession"/>, but does not Save the session.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the bucket in which to create the directory.</param>
    /// <param name="directoryPath">A slash-separated path to the directory (e.g., "/myDirectory/subDirectory").</param>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    /// <exception cref="DirectoryAlreadyExistsException">Thrown when a directory already exists at the specified path.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateDirectoryAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryDirectory directoryPath)
    {
        // Look up the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Create the directory using procedures
        await _contentDirectoryProcedures.CreateDirectoryAsync(documentSession, targetBucket.Id, directoryPath);
    }

    /// <summary>
    /// Creates an empty directory in the specified bucket, with auto-creation of the bucket if needed.
    /// </summary>
    /// <remarks>
    /// Transaction Control: If <paramref name="autoCreateBucket"/> is true, the bucket is created using a separate
    /// session. The directory is added to the session specified by <paramref name="documentSession"/>,
    /// but does not Save the session.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the bucket in which to create the directory.</param>
    /// <param name="directoryPath">A slash-separated path to the directory.</param>
    /// <param name="autoCreateBucket">Default: true. Whether to create the bucket if it does not exist.</param>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket is not found and <paramref name="autoCreateBucket"/> is false.</exception>
    /// <exception cref="DirectoryAlreadyExistsException">Thrown when a directory already exists at the specified path.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task CreateDirectoryAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryDirectory directoryPath, bool autoCreateBucket)
    {
        if (autoCreateBucket)
        {
            // Ensure bucket exists
            await CreateBucketAsync(documentSession, bucketName);
        }

        // Create the directory (without auto-create, so it will throw if bucket doesn't exist)
        await CreateDirectoryAsync(documentSession, bucketName, directoryPath);
    }
}
