using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public interface IContentRepositoryScoped
{
    IDocumentSession DocumentSession { get; }

    Task CreateBucketAsync(string bucketName);

    Task DeleteBucketAsync(string bucketName, bool force = false);

    Task UploadStreamAsync(string bucketName, ContentRepositoryResourcePath resourcePath,
        Stream inStream, bool autoCreateBucket = true,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);

    Task<Stream?> DownloadStreamAsync(string bucketName,
        ContentRepositoryResourcePath resourcePath);

    Task<bool> ResourceExistsAsync(string bucketName, ContentRepositoryResourcePath resourcePath);

    Task DeleteResourceAsync(string bucketName, ContentRepositoryResourcePath resourcePath);

    Task<ContentRepositoryFileInfo?> GetResourceInfoAsync(string bucketName, ContentRepositoryResourcePath resourcePath);

    Task RenameResourceAsync(
        string oldBucketName, ContentRepositoryResourcePath oldResourcePath,
        string newBucketName, ContentRepositoryResourcePath newResourcePath,
        bool overwriteDestination = false);

    Task CopyResourceAsync(
        string oldBucketName, ContentRepositoryResourcePath oldResourcePath,
        string newBucketName, ContentRepositoryResourcePath newResourcePath,
        bool autoCreateBucket = true, bool overwriteDestination = false);

    Task<PagedContentRepositoryFileInfo> GetResourceListingAsync(
        string bucketName, ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);

    Task<PagedContentRepositoryFileInfo> GetResourceListingByUserGuidAsync(string bucketName, Guid userGuid,
        int oneBasedPage, int pageSize);
}