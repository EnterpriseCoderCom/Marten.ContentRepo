using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Retrieves a paginated list of resources from a specified bucket associated with the given user-defined long value.
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that provides the context for querying the database.</param>
    /// <param name="bucketName">The name of the bucket from which resources will be retrieved.</param>
    /// <param name="userLong">A custom user-defined long value associated with the resources to filter the results.</param>
    /// <param name="oneBasedPage">The page number (1-based index) for retrieving the paginated results.</param>
    /// <param name="pageSize">The number of resources to include per page in the paginated results.</param>
    /// <returns>A <c>PagedContentRepositoryResourceInfo</c> object containing the paginated list of resources.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    /// <see cref="PagedContentRepositoryResourceInfo"/>   
    public async Task<PagedContentRepositoryResourceInfo> GetResourceListingByUserDataLongAsync(
        IDocumentSession documentSession, string bucketName, long userLong,
        int oneBasedPage, int pageSize)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        IPagedList<ContentResourceHeader> pageList =
            await _resourceHeaderProcedures.SelectByUserLong(documentSession, targetBucket, userLong, oneBasedPage,
                pageSize);

        return new PagedContentRepositoryResourceInfo(pageList, targetBucket.BucketName);
    }
}