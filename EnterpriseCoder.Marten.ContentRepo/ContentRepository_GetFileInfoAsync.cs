﻿using EnterpriseCoder.Marten.ContentRepo.DtoMapping;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<ContentRepositoryFileInfo?> GetFileInfoAsync(IDocumentSession documentSession,
        ContentRepositoryFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        return targetHeader?.ToContentFileInfoDto();
    }
}