using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The GetResourceListingByUserDataGuidAsync method is used to retrieve a paged listing of resources that were saved
    /// with a specific "user data" guid.  This can be used as a type of foreign key to a user table in order to
    /// track quotas if desired.  The guid field associated with the user data is indexed for fast lookup.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket to be searched.</param>
    /// <param name="userGuid">The guid identifier that will be used in the search.</param>
    /// <param name="oneBasedPage">The one-based page number to be queried.</param>
    /// <param name="pageSize">The desired size for each page of resource information.</param>
    /// <returns>Returns a <see cref="PagedContentRepositoryFileInfo"/> that contains the items for the page as well as paging information.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    public async Task<PagedContentRepositoryFileInfo> GetResourceListingByUserDataGuidAsync(
        IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize)
    {
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        IPagedList<ContentFileHeader> pageList =
            await _fileHeaderProcedures.SelectByUserGuid(documentSession, targetBucket, userGuid, oneBasedPage,
                pageSize);

        return new PagedContentRepositoryFileInfo(pageList);
    }
}