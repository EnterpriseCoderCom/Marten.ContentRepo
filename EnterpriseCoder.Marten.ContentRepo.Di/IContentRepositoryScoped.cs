using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public interface IContentRepositoryScoped
{
    IDocumentSession DocumentSession { get; }

    Task UploadStreamAsync(ContentRepositoryFilePath filePath, Stream inStream, bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> DownloadStreamAsync(ContentRepositoryFilePath filePath);
    Task<bool> FileExistsAsync(ContentRepositoryFilePath filePath);
    Task DeleteFileAsync(ContentRepositoryFilePath filePath);
    Task<ContentRepositoryFileInfo?> GetFileInfoAsync(ContentRepositoryFilePath filePath);
    Task RenameFileAsync(ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath, bool overwriteDestination = false);
    Task CopyFileAsync(ContentRepositoryFilePath oldFilePath, ContentRepositoryFilePath newFilePath, bool overwriteDestination = false);
    Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(ContentRepositoryDirectory directory, int oneBasedPage, int pageSize, bool recursive = false);
}