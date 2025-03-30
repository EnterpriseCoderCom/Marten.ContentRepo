using EnterpriseCoder.MartenDb.GridFs.Entities;
using Marten;

namespace EnterpriseCoder.MartenDb.GridFs.Procedures;

public class GridFileHeaderProcedures
{
    public async Task<GridFileHeader?> SelectAsync(IDocumentSession documentSession, GridFsFilePath filePath)
    {
        GridFileHeader? targetHeader = await documentSession.Query<GridFileHeader>()
            .SingleOrDefaultAsync(x => x.FilePath == filePath.Path);
        
        return targetHeader;
    }

    public Task DeleteAsync(IDocumentSession documentSession, GridFileHeader targetHeader)
    {
        documentSession.Delete<GridFileHeader>(targetHeader.Id);
        return Task.CompletedTask;
    }

    public Task UpsertAsync(IDocumentSession documentSession, GridFileHeader targetHeader)
    {
        documentSession.Store(targetHeader);
        return Task.CompletedTask;
    }
}