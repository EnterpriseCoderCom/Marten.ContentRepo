using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public class ContentRepositoryScoped : IContentRepositoryScoped
{
    private readonly IDocumentSession _documentSession;
    private readonly IContentRepository _contentRepository;

    public ContentRepositoryScoped(IDocumentSession documentSession, IContentRepository contentRepository)
    {
        _documentSession = documentSession;
        _contentRepository = contentRepository;
    }

    public async Task UploadStreamAsync(ContentRepositoryFilePath filePath, Stream inStream, bool overwriteExisting = false,
        Guid? userGuid = null,
        long userValue = 0L)
    {
        await _contentRepository.UploadStreamAsync(_documentSession, filePath, inStream, overwriteExisting, userGuid,
            userValue);
    }

    public async Task<Stream?> DownloadStreamAsync(ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.DownloadStreamAsync(_documentSession, filePath);
    }

    public async Task<bool> FileExistsAsync(ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.FileExistsAsync(_documentSession, filePath);
    }

    public async Task DeleteFileAsync(ContentRepositoryFilePath filePath)
    {
        await _contentRepository.DeleteFileAsync(_documentSession, filePath);
    }

    public async Task<ContentRepositoryFileInfo?> GetFileInfoAsync(ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.GetFileInfoAsync(_documentSession, filePath);
    }

    public async Task RenameFileAsync(ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false)
    {
        await _contentRepository.RenameFileAsync(_documentSession, oldFilePath, newFilePath, overwriteDestination);
    }

    public async Task CopyFileAsync(ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false)
    {
        await _contentRepository.CopyFileAsync(_documentSession, oldFilePath, newFilePath);
    }

    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(ContentRepositoryDirectory directory, int oneBasedPage,
        int pageSize, bool recursive = false)
    {
        return await _contentRepository.GetFileListingAsync(_documentSession, directory, oneBasedPage, pageSize,
            recursive);
    }

    public IDocumentSession DocumentSession => _documentSession;
}