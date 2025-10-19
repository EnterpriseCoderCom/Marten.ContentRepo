using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The DeleteResourceAsync method is used to remove content from the repository.  The resource to be removed is
    /// specified by the <paramref name="documentSession"/> and <paramref name="resourcePath"/> arguments.  If the given
    /// resource is not found, then this method returns without error.
    /// </summary>
    /// <remarks>
    /// When <paramref name="autoCleanupEmptyDirectories"/> is true, explicit directories that become empty after
    /// resource deletion are automatically removed. This cleanup cascades up the directory tree until a non-empty
    /// directory is found or the root directory is reached. Implicit directories (those created only by file paths)
    /// are not affected by this cleanup.
    /// </remarks>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.
    /// "/myResourcePath/myImage.png"</param>
    /// <param name="autoCleanupEmptyDirectories">Default: true. When true, empty parent directories are automatically
    /// removed after the resource is deleted. Set to false to skip cleanup (useful for performance when deleting many files).</param>
    public async Task DeleteResourceAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryResourcePath resourcePath, bool autoCleanupEmptyDirectories = true)
    {
        // Lookup the target bucket.
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return;
        }

        // Lookup the target resource
        var targetHeader = await _resourceHeaderProcedures.SelectAsync(documentSession, targetBucket, resourcePath);
        if (targetHeader is null)
        {
            return;
        }

        // Delete all file blocks associated with this header.
        await _resourceBlockProcedures.DeleteAsync(documentSession, targetHeader);

        // Delete the header itself.
        await _resourceHeaderProcedures.DeleteAsync(documentSession, targetHeader);

        // Clean up empty parent directories if requested
        if (autoCleanupEmptyDirectories)
        {
            var parentDirectory = resourcePath.Directory;
            await CleanupEmptyDirectoriesAsync(documentSession, targetBucket.Id, parentDirectory);
        }
    }
}