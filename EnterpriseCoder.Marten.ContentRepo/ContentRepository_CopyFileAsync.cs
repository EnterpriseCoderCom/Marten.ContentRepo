using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task CopyFileAsync(IDocumentSession documentSession,
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool autoCreateNewBucket = true, bool overwriteDestination = false)
    {
        // Get the old bucket
        ContentBucket? oldBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, oldBucketName);
        if (oldBucket is null)
        {
            throw new BucketNotFoundException(oldBucketName);
        }

        // Lookup the old resource
        var sourceHeader = await _fileHeaderProcedures.SelectAsync(documentSession, oldBucket, oldFilePath);
        if (sourceHeader == null)
        {
            throw new ResourceNotFoundException(oldBucketName, oldFilePath);
        }

        // Lookup and possibly create the new bucket.
        ContentBucket? newBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, newBucketName);
        if (newBucket is null && autoCreateNewBucket)
        {
            newBucket = await _contentBucketProcedures.CreateBucketAsync(documentSession, newBucketName);
        }

        if (newBucket is null)
        {
            throw new BucketNotFoundException(newBucketName);
        }

        // Lookup the new resource
        var targetHeader = await _fileHeaderProcedures.SelectAsync(documentSession, newBucket, newFilePath);
        if (targetHeader != null)
        {
            if (!overwriteDestination)
            {
                throw new OverwriteNotPermittedException(newBucketName, newFilePath);
            }
        }

        // Copy the file
        await using var oldFileStream = await DownloadStreamAsync(documentSession, oldBucketName, oldFilePath);
        if (oldFileStream == null)
        {
            throw new ApplicationException($"Unable to load {nameof(oldFilePath)}");
        }

        await UploadStreamAsync(documentSession, newBucketName, newFilePath, oldFileStream, autoCreateNewBucket,
            overwriteDestination, sourceHeader.UserDataGuid, sourceHeader.UserDataLong);
    }
}