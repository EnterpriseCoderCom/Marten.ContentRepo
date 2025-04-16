using EnterpriseCoder.Marten.ContentRepo.CompiledQueries;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;
using Marten.Pagination;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentFileHeaderProcedures
{
    public async Task<ContentFileHeader?> SelectAsync(IDocumentSession documentSession, ContentBucket targetBucket,
        ContentRepositoryResourcePath resourcePath)
    {
        var targetHeader = await documentSession.QueryAsync(new QuerySelectContentFileHeader
        {
            BucketId = targetBucket.Id,
            FilePath = resourcePath
        });

        return targetHeader;
    }

    public async Task<IPagedList<ContentFileHeader>> SelectByUserGuid(IDocumentSession documentSession,
        ContentBucket targetBucket, Guid userGuid, int pageNumber, int pageSize)
    {
        IPagedList<ContentFileHeader> pageList = await documentSession.Query<ContentFileHeader>()
            .Where(x => x.UserDataGuid == userGuid && x.BucketId == targetBucket.Id)
            .ToPagedListAsync(pageNumber, pageSize);

        return pageList;
    }

    public Task DeleteAsync(IDocumentSession documentSession, ContentFileHeader targetHeader)
    {
        documentSession.Delete<ContentFileHeader>(targetHeader.Id);
        return Task.CompletedTask;
    }

    public Task UpsertAsync(IDocumentSession documentSession, ContentFileHeader targetHeader)
    {
        documentSession.Store(targetHeader);
        return Task.CompletedTask;
    }
}