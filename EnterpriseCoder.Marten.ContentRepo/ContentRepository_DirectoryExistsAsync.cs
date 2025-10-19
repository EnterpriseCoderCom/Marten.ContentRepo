using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Determines if an explicit directory exists at the specified path.
    /// </summary>
    /// <remarks>
    /// This method checks for explicitly created directories. Implicit directories (derived from file paths)
    /// are not checked by this method.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to check the database.</param>
    /// <param name="bucketName">The name of the bucket containing the directory.</param>
    /// <param name="directoryPath">A slash-separated path to the directory.</param>
    /// <returns>Returns true if an explicit directory exists at the specified path. Otherwise, returns false.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    public async Task<bool> DirectoryExistsAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryDirectory directoryPath)
    {
        // Validate bucket exists
        var bucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (bucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Check for directory existence
        var exists = await _contentDirectoryProcedures.DirectoryExistsAsync(
            documentSession, bucket.Id, directoryPath);

        return exists;
    }
}
