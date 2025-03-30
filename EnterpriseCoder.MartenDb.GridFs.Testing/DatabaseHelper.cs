﻿using EnterpriseCoder.MartenDb.GridFs.Entities;
using Marten;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class DatabaseHelper
{
    private readonly IDocumentSession _session;
    public DatabaseHelper(IDocumentSession session)
    {
        _session = session;
    }

    public async Task ClearDatabaseAsync()
    {
        _session.DeleteWhere<GridFileBlock>(x => true);
        _session.DeleteWhere<GridFileHeader>(x => true);
        await _session.SaveChangesAsync();
    }

    public async Task<int> CountHeadersAsync()
    {
        return await _session.Query<GridFileHeader>().CountAsync();
    }

    public async Task<int> CountBlocksAsync()
    {
        return await _session.Query<GridFileBlock>().CountAsync();
    }
}