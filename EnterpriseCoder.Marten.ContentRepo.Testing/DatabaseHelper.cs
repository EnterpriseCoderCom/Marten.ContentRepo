using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class DatabaseHelper
{
    private readonly IDocumentSession _session;

    public DatabaseHelper(IDocumentSession session)
    {
        _session = session;
    }

    public async Task ClearDatabaseAsync()
    {
        _session.DeleteWhere<ContentFileBlock>(x => true);
        _session.DeleteWhere<ContentFileHeader>(x => true);
        _session.DeleteWhere<ContentBucket>(x => true);
        await _session.SaveChangesAsync();
    }

    public async Task<int> CountHeadersAsync()
    {
        return await _session.Query<ContentFileHeader>().CountAsync();
    }

    public async Task<int> CountBlocksAsync()
    {
        return await _session.Query<ContentFileBlock>().CountAsync();
    }

    public async Task<int> CountBucketsAsync()
    {
        return await _session.Query<ContentBucket>().CountAsync();
    }

    public IDocumentSession Session => _session;
}