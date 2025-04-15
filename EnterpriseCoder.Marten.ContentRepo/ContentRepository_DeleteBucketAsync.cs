using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// <para>
    /// The <c>DeleteBucketAsync</c> method is used to permanently remove a bucket from the database.  The
    /// bucket must be empty in order to delete the bucket unless the <paramref name="force"/> argument is true.  If
    /// <paramref name="force"/> is set to true, all content resources in the bucket will be deleted.
    /// </para> 
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the content bucket to be created.</param>
    /// <param name="force">Set this value to <c>true</c> to force the destruction of a non-empty bucket.</param>
    /// <exception cref="DeleteFailureException">
    /// <param>If <paramref name="force"/> is <c>false</c> and the bucket is not empty, then a DeleteFailureException will be thrown.</param></exception>
    /// <returns></returns>
    public async Task DeleteBucketAsync(IDocumentSession documentSession, string bucketName, bool force = false)
    {
        // Lookup the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            return;
        }

        // See if there is any content in the bucket
        var hasContent = await documentSession.Query<ContentFileHeader>().AnyAsync(x => x.BucketId == targetBucket.Id);
        if (hasContent && force == false)
        {
            throw new DeleteFailureException(bucketName, "*");
        }

        // Destroy all content in the bucket...
        var contentList = documentSession.Query<ContentFileHeader>()
            .Where(x => x.Directory.StartsWith("/")).ToAsyncEnumerable();

        await foreach (var nextContentItem in contentList)
        {
            await DeleteResourceAsync(documentSession, bucketName, nextContentItem.FilePath);
        }

        // Delete the bucket entry itself.
        await _contentBucketProcedures.DeleteBucketAsync(documentSession, bucketName);
    }
}