using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// <para>
    /// The <c>DeleteBucketAsync</c> method is used to permanently remove a bucket from the database.  The
    /// bucket must be empty to delete the bucket unless the <paramref name="force"/> argument is true.  If
    /// <paramref name="force"/> is set to true, all content resources and explicit directories in the bucket will be deleted.
    /// </para>
    /// </summary>
    /// <remarks>
    /// Transaction Control:  This method will delete items within the bucket 100 items at a time.  This is done
    /// using a series of separate database transactions.  The bucket deletion itself is not committed until changes are saved
    /// on the incoming <paramref name="documentSession"/> reference.
    ///
    /// Directory Cleanup:  When <paramref name="force"/> is true, all explicit directories (created via CreateDirectoryAsync)
    /// are deleted along with resources. Empty check verifies both resources and explicit directories exist before deletion.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the content bucket to be deleted.</param>
    /// <param name="force">Set this value to <c>true</c> to force the destruction of a non-empty bucket.</param>
    /// <exception cref="BucketNotEmptyException">
    /// <param>If <paramref name="force"/> is <c>false</c> and the bucket is not empty (contains resources and/or explicit directories), then a BucketNotEmptyException will be thrown.</param></exception>
    /// <returns></returns>
    public async Task DeleteBucketAsync(IDocumentSession documentSession, string bucketName, bool force = false)
    {
        // Lookup the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return;
        }

        // See if there is any content (resources or directories) in the bucket
        var hasContent = await documentSession.Query<ContentResourceHeader>().AnyAsync(x => x.BucketId == targetBucket.Id);
        var hasDirectories = await HasDirectoriesAsync(documentSession, targetBucket.Id);

        if ((hasContent || hasDirectories) && force == false)
        {
            throw new BucketNotEmptyException(bucketName, "*");
        }

        // Get a repeated page listing - delete 100 items at a time until there are none left.
        var contentList = await GetResourceListingAsync(documentSession, bucketName, "/", 1, 100, true);
        while (contentList.Count > 0)
        {
            foreach (var nextContentItem in contentList)
            {
                using (var localSession = documentSession.DocumentStore.LightweightSession())
                {
                    await DeleteResourceAsync(localSession, bucketName, nextContentItem.ResourcePath);
                    await localSession.SaveChangesAsync();
                }
            }

            contentList = await GetResourceListingAsync(documentSession, bucketName, "/", 1, 100, true);
        }

        // Delete all explicit directories in the bucket
        await DeleteAllDirectoriesAsync(documentSession, targetBucket.Id);

        // Delete the bucket entry itself.
        await _contentBucketProcedures.DeleteBucketAsync(documentSession, bucketName);
    }

    /// <summary>
    /// Checks if a bucket contains any explicit directories.
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to query the database.</param>
    /// <param name="bucketId">The unique identifier of the bucket to check.</param>
    /// <returns>True if the bucket contains explicit directories; otherwise, false.</returns>
    private async Task<bool> HasDirectoriesAsync(IDocumentSession documentSession, Guid bucketId)
    {
        return await documentSession.Query<ContentDirectory>()
            .AnyAsync(x => x.BucketId == bucketId);
    }

    /// <summary>
    /// Deletes all explicit directories in a bucket in batches.
    /// </summary>
    /// <remarks>
    /// Transaction Control:  This method deletes directories 100 items at a time using separate database
    /// transactions for memory safety. Each batch is immediately committed.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketId">The unique identifier of the bucket whose directories should be deleted.</param>
    private async Task DeleteAllDirectoriesAsync(IDocumentSession documentSession, Guid bucketId)
    {
        var query = documentSession.Query<ContentDirectory>()
            .Where(x => x.BucketId == bucketId);

        // Delete in batches (same pattern as resources)
        while (true)
        {
            var batch = await query.Take(100).ToListAsync();
            if (batch.Count == 0)
            {
                break;
            }

            foreach (var directory in batch)
            {
                documentSession.Delete(directory);
            }

            await documentSession.SaveChangesAsync();
        }
    }
}