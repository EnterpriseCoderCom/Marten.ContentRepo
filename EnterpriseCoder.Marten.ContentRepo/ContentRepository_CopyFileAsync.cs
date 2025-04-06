using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task CopyFileAsync(IDocumentSession documentSession, ContentRepositoryFilePath oldFilePath,
        ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false)
    {
        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, oldFilePath);
        if (sourceHeader == null)
        {
            throw new FileNotFoundException(oldFilePath);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, newFilePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new IOException($"File {newFilePath} exists and {nameof(overwriteDestination)} is set to false.");
            }
        }

        // Copy the file
        await using var oldFileStream = await DownloadStreamAsync(documentSession, oldFilePath);
        if (oldFileStream == null)
        {
            throw new ApplicationException($"Unable to load {nameof(oldFilePath)}");
        }

        await UploadStreamAsync(documentSession, newFilePath, oldFileStream, true);
    }
}