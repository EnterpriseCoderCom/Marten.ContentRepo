using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public interface IContentRepository
{
    Task UploadStreamAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath, Stream inStream,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath);
    Task<bool> FileExistsAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath);
    Task DeleteFileAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath);
    Task<ContentRepositoryFileInfo?> GetFileInfoAsync(IDocumentSession documentSession, ContentRepositoryFilePath filePath);
    Task RenameFileAsync(IDocumentSession documentSession, ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false);
    Task CopyFileAsync(IDocumentSession documentSession, ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false);
    Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(IDocumentSession documentSession, ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);
}