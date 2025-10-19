using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using EnterpriseCoder.Marten.ContentRepo.Utility;
using Marten;
using DirectoryNotFoundException = EnterpriseCoder.Marten.ContentRepo.Exceptions.DirectoryNotFoundException;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Deletes an empty directory from the specified bucket.
    /// </summary>
    /// <remarks>
    /// Transaction Control: This method marks the directory for deletion in the session specified by
    /// <paramref name="documentSession"/>, but does not Save the session.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the bucket containing the directory.</param>
    /// <param name="directoryPath">A slash-separated path to the directory to be deleted.</param>
    /// <param name="force">Default: false. When true, deletes the directory even if it contains subdirectories or implicit files.</param>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown when the directory is not found.</exception>
    /// <exception cref="DirectoryNotEmptyException">Thrown when the directory contains files or subdirectories and <paramref name="force"/> is false.</exception>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task DeleteDirectoryAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryDirectory directoryPath, bool force = false)
    {
        // Look up the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Look up the directory
        var directory = await _contentDirectoryProcedures.SelectDirectoryByPathAsync(
            documentSession, targetBucket.Id, directoryPath);
        if (directory == null)
        {
            throw new DirectoryNotFoundException(targetBucket.Id, directoryPath.Path);
        }

        // Check if directory is empty (both files and subdirectories)
        var isEmpty = await DirectoryValidator.IsDirectoryEmptyAsync(documentSession, targetBucket.Id, directoryPath);

        if (!isEmpty && !force)
        {
            throw new DirectoryNotEmptyException(directoryPath.Path);
        }

        // If force is true and directory is not empty, delete all contents recursively
        if (force && !isEmpty)
        {
            await DeleteDirectoryContentsRecursivelyAsync(documentSession, targetBucket.Id, directoryPath);
        }

        // Delete the directory entity itself
        documentSession.Delete(directory);
    }

    /// <summary>
    /// Recursively deletes all contents (files and subdirectories) within a directory.
    /// </summary>
    private async Task DeleteDirectoryContentsRecursivelyAsync(
        IDocumentSession documentSession,
        Guid bucketId,
        ContentRepositoryDirectory directoryPath)
    {
        var path = directoryPath.Path;
        var prefix = path == "/" ? "/" : path + "/";

        // Get the bucket to obtain its name for DeleteResourceAsync
        var bucket = await documentSession.Query<ContentBucket>()
            .FirstOrDefaultAsync(b => b.Id == bucketId);
        if (bucket == null)
        {
            // Bucket should exist, but safeguard against deletion race conditions
            return;
        }

        // Get all subdirectories recursively
        var subdirectories = await documentSession.Query<ContentDirectory>()
            .Where(d => d.BucketId == bucketId && d.DirectoryPath.StartsWith(prefix))
            .ToListAsync();

        // Sort by path depth (deepest first) to delete from bottom-up
        var sortedSubdirectories = subdirectories
            .OrderByDescending(d => d.DirectoryPath.Count(c => c == '/'))
            .ToList();

        // Delete all subdirectories
        foreach (var subdir in sortedSubdirectories)
        {
            documentSession.Delete(subdir);
        }

        // Get all files in this directory and subdirectories
        var files = await documentSession.Query<ContentResourceHeader>()
            .Where(f =>
                f.BucketId == bucketId &&
                (f.Directory == path || f.Directory.StartsWith(prefix)))
            .ToListAsync();

        // Delete all files and their blocks
        foreach (var file in files)
        {
            // Delete the file using the existing DeleteResourceAsync method
            // Construct the resource path from the file's ResourcePath field
            var resourcePath = new ContentRepositoryResourcePath(file.ResourcePath);
            await DeleteResourceAsync(documentSession, bucket.BucketName, resourcePath);
        }
    }
}
