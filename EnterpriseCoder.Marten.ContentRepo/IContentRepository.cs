using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public interface IContentRepository
{
    Task CreateBucketAsync(IDocumentSession session, string bucketName);

    Task DeleteBucketAsync(IDocumentSession session, string bucketName, bool force = false);

    Task UploadStreamAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath filePath,
        Stream inStream, bool autoCreateBucket = true,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);

    Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryFilePath filePath);

    Task<bool> FileExistsAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath filePath);

    Task DeleteFileAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath filePath);

    Task<ContentRepositoryFileInfo?> GetFileInfoAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryFilePath filePath);

    Task RenameFileAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryFilePath oldFilePath,
        ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false);

    Task CopyFileAsync(IDocumentSession documentSession,
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool autoCreateBucket = true, bool overwriteDestination = false);

    Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);

    Task<IList<ContentRepositoryFileInfo>> GetFileListingByUserDataGuidAsync(IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize);
}