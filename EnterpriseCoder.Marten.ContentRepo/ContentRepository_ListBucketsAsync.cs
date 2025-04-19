using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The ListBucketsAsync method returns a paged listing of all buckets in the repository.  The returned
    /// <see cref="PagedBucketNameListing"/> contains paging information so that large repositories listings can be handled
    /// in a memory-safe way.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="oneBasedPageNumber">The one-based page number to be returned by this call.</param>
    /// <param name="pageSize">The desired size for the returned page of information.</param>
    /// <returns>Returns a <see cref="PagedBucketNameListing"/> object that contains the items for the requested page.</returns>
    public async Task<PagedBucketNameListing> ListBucketsAsync(IDocumentSession documentSession,
        int oneBasedPageNumber, int pageSize)
    {
        IPagedList<ContentBucket> bucketList = await documentSession.Query<ContentBucket>().ToPagedListAsync(oneBasedPageNumber, pageSize);
        return new PagedBucketNameListing(bucketList);
    }
}