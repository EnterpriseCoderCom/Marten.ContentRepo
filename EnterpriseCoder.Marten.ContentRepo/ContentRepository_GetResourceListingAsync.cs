using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// The GetResourceListingAsync method is used to get a paged listing of all resources from the bucket specified by
    /// <paramref name="bucketName"/> for all resources that start with <paramref name="resourcePrefix"/>.  The
    /// returned <see cref="PagedContentRepositoryFileInfo"/> contains paging information so that large repositories
    /// listings can be handled in a memory-safe way.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket to be searched.</param>
    /// <param name="resourcePrefix">The resource prefix to be searched.  For example, "/images" to return information about all resources that start with "/images".</param>
    /// <param name="oneBasedPage">The one-based page to be returned by this call.</param>
    /// <param name="pageSize">The desired size for the returned page of information.</param>
    /// <param name="recursive">Default: false.  Set to true to return all resources under <paramref name="resourcePrefix"/>.  Set too false to return only resources that are directly in the given prefix pseudo-directory.</param>
    /// <returns>Returns a <see cref="PagedContentRepositoryFileInfo"/> object that contains the items for the requested page as
    /// well as information about the total number of pages.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    /// <see cref="PagedContentRepositoryFileInfo"/>
    public async Task<PagedContentRepositoryFileInfo> GetResourceListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory resourcePrefix, int oneBasedPage, int pageSize,
        bool recursive = false)
    {
        // Convert the incoming directory to a string
        string directoryString = resourcePrefix;

        // Lookup the bucket
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket is null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        IQueryable<ContentFileHeader> baseQuery = documentSession.Query<ContentFileHeader>();
        if (recursive)
        {
            // StartsWith so we get the Directory plus anything under it.
            baseQuery = baseQuery.Where(x => x.Directory.StartsWith(directoryString));
        }
        else
        {
            // Direct equality of the Directory field.
            baseQuery = baseQuery.Where(x => x.Directory == directoryString);
        }

        // Ask Marten for a paged result.
        var pagedList = await baseQuery.ToPagedListAsync(oneBasedPage, pageSize);

        // Convert to a list and return.
        return new PagedContentRepositoryFileInfo(pagedList);
    }
}