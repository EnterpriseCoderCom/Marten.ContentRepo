using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task DeleteFileAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath)
    {
        // Lookup the target resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, filePath);
        if (targetHeader is null)
        {
            return;
        }

        // Delete all file blocks associated with this header.
        await _fileBlockProcedures.DeleteAsync(documentSession, targetHeader);

        // Delete the header itself.
        await _fileHeaderProcedures.DeleteAsync(documentSession, targetHeader);
    }
}