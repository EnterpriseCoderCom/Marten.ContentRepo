using Marten;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public class ContentRepositoryScoped : IContentRepositoryScoped
{
    private readonly IContentRepository _contentRepository;
    private readonly IDocumentSession _documentSession;


    public ContentRepositoryScoped(IDocumentSession documentSession, IContentRepository contentRepository)
    {
        _documentSession = documentSession;
        _contentRepository = contentRepository;
    }

    public async Task CreateBucketAsync(string bucketName)
    {
        await _contentRepository.CreateBucketAsync(_documentSession, bucketName);
    }

    public async Task DeleteBucketAsync(string bucketName, bool force = false)
    {
        await _contentRepository.DeleteBucketAsync(_documentSession, bucketName, force);
    }

    public async Task UploadStreamAsync(string bucketName, ContentRepositoryResourcePath resourcePath, Stream inStream,
        bool autoCreateBucket = true,
        bool overwriteExisting = false,
        Guid? userGuid = null,
        long userValue = 0L)
    {
        await _contentRepository.UploadStreamAsync(_documentSession, bucketName, resourcePath, inStream, autoCreateBucket,
            overwriteExisting, userGuid, userValue);
    }

    public async Task<Stream?> DownloadStreamAsync(string bucketName, ContentRepositoryResourcePath resourcePath)
    {
        return await _contentRepository.DownloadStreamAsync(_documentSession, bucketName, resourcePath);
    }

    public async Task<bool> ResourceExistsAsync(string bucketName, ContentRepositoryResourcePath resourcePath)
    {
        return await _contentRepository.ResourceExistsAsync(_documentSession, bucketName, resourcePath);
    }

    public async Task DeleteResourceAsync(string bucketName, ContentRepositoryResourcePath resourcePath)
    {
        await _contentRepository.DeleteResourceAsync(_documentSession, bucketName, resourcePath);
    }

    public async Task<ContentRepositoryFileInfo?> GetResourceInfoAsync(string bucketName,
        ContentRepositoryResourcePath resourcePath)
    {
        return await _contentRepository.GetResourceInfoAsync(_documentSession, bucketName, resourcePath);
    }

    public async Task RenameResourceAsync(
        string oldBucketName, ContentRepositoryResourcePath oldResourcePath,
        string newBucketName, ContentRepositoryResourcePath newResourcePath,
        bool overwriteDestination = false)
    {
        await _contentRepository.RenameResourceAsync(_documentSession, oldBucketName, oldResourcePath, newBucketName,
            newResourcePath,
            overwriteDestination);
    }

    public async Task CopyResourceAsync(string oldBucket, ContentRepositoryResourcePath oldResourcePath, string newBucket,
        ContentRepositoryResourcePath newResourcePath,
        bool autoCreateBucket = true, bool overwriteDestination = false)
    {
        await _contentRepository.CopyResourceAsync(_documentSession, oldBucket, oldResourcePath, newBucket, newResourcePath,
            autoCreateBucket, overwriteDestination);
    }

    public async Task<PagedContentRepositoryFileInfo> GetResourceListingAsync(string bucketName,
        ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize, bool recursive = false)
    {
        return await _contentRepository.GetResourceListingAsync(_documentSession, bucketName, directory, oneBasedPage,
            pageSize,
            recursive);
    }

    public async Task<PagedContentRepositoryFileInfo> GetResourceListingByUserGuidAsync(string bucketName, Guid userGuid,
        int oneBasedPage, int pageSize)
    {
        return await _contentRepository.GetResourceListingByUserDataGuidAsync(_documentSession, bucketName, userGuid,
            oneBasedPage, pageSize);
    }

    public IDocumentSession DocumentSession => _documentSession;
}