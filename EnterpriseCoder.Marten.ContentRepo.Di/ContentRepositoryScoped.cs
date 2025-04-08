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

    public async Task CreateBucketAsync(string bucketName)
    {
        await _contentRepository.CreateBucketAsync(_documentSession, bucketName);
    }

    public async Task DeleteBucketAsync(string bucketName, bool force = false)
    {
        await _contentRepository.DeleteBucketAsync(_documentSession, bucketName, force);
    }

    public async Task UploadStreamAsync(string bucketName, ContentRepositoryFilePath filePath, Stream inStream,
        bool autoCreateBucket = true,
        bool overwriteExisting = false,
        Guid? userGuid = null,
        long userValue = 0L)
    {
        await _contentRepository.UploadStreamAsync(_documentSession, bucketName, filePath, inStream, autoCreateBucket,
            overwriteExisting, userGuid, userValue);
    }

    public async Task<Stream?> DownloadStreamAsync(string bucketName, ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.DownloadStreamAsync(_documentSession, bucketName, filePath);
    }

    public async Task<bool> FileExistsAsync(string bucketName, ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.FileExistsAsync(_documentSession, bucketName, filePath);
    }

    public async Task DeleteFileAsync(string bucketName, ContentRepositoryFilePath filePath)
    {
        await _contentRepository.DeleteFileAsync(_documentSession, bucketName, filePath);
    }

    public async Task<ContentRepositoryFileInfo?> GetFileInfoAsync(string bucketName,
        ContentRepositoryFilePath filePath)
    {
        return await _contentRepository.GetFileInfoAsync(_documentSession, bucketName, filePath);
    }

    public async Task RenameFileAsync(string bucketName, ContentRepositoryFilePath oldFilePath,
        ContentRepositoryFilePath newFilePath,
        bool overwriteDestination = false)
    {
        await _contentRepository.RenameFileAsync(_documentSession, bucketName, oldFilePath, newFilePath,
            overwriteDestination);
    }

    public async Task CopyFileAsync(string oldBucket, ContentRepositoryFilePath oldFilePath, string newBucket,
        ContentRepositoryFilePath newFilePath,
        bool autoCreateBucket = true, bool overwriteDestination = false)
    {
        await _contentRepository.CopyFileAsync(_documentSession, oldBucket, oldFilePath, newBucket, newFilePath,
            autoCreateBucket, overwriteDestination);
    }

    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingAsync(string bucketName,
        ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize, bool recursive = false)
    {
        return await _contentRepository.GetFileListingAsync(_documentSession, bucketName, directory, oneBasedPage,
            pageSize,
            recursive);
    }

    public async Task<IList<ContentRepositoryFileInfo>> GetFileListingByUserGuidAsync(string bucketName, Guid userGuid,
        int oneBasedPage, int pageSize)
    {
        return await _contentRepository.GetFileListingByUserDataGuidAsync(_documentSession, bucketName, userGuid,
            oneBasedPage, pageSize);
    }

    public IDocumentSession DocumentSession => _documentSession;
}