using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Procedures;

public class ContentFileHeaderProcedures
{
    public async Task<ContentFileHeader?> SelectAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath)
    {
        ContentFileHeader? targetHeader = await documentSession.Query<ContentFileHeader>()
            .SingleOrDefaultAsync(x => x.FilePath == filePath.Path);
        
        return targetHeader;
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