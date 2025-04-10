using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public interface IContentRepositoryScoped
{
    IDocumentSession DocumentSession { get; }

    Task CreateBucketAsync(string bucketName);

    Task DeleteBucketAsync(string bucketName, bool force = false);

    Task UploadStreamAsync(string bucketName, ContentRepositoryFilePath filePath,
        Stream inStream, bool autoCreateBucket = true,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);

    Task<Stream?> DownloadStreamAsync(string bucketName,
        ContentRepositoryFilePath filePath);

    Task<bool> FileExistsAsync(string bucketName, ContentRepositoryFilePath filePath);

    Task DeleteFileAsync(string bucketName, ContentRepositoryFilePath filePath);

    Task<ContentRepositoryFileInfo?> GetFileInfoAsync(string bucketName, ContentRepositoryFilePath filePath);

    Task RenameFileAsync(
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false);

    Task CopyFileAsync(
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool autoCreateBucket = true, bool overwriteDestination = false);

    Task<PagedContentRepositoryFileInfo> GetFileListingAsync(
        string bucketName, ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);

    Task<PagedContentRepositoryFileInfo> GetFileListingByUserGuidAsync(string bucketName, Guid userGuid,
        int oneBasedPage, int pageSize);
}