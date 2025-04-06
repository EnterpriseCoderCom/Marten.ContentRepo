using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(IDocumentSession documentSession,
        ContentRepositoryDirectory directory, int oneBasedPage, int pageSize,
        bool recursive = false)
    {
        // Convert the incoming directory to a string
        string directoryString = directory;
        
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
        return new List<ContentRepositoryFileInfo>(pagedList.Select(x => x.ToContentFileInfoDto()));
    }
}