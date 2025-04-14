using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public interface IContentRepository
{
    /// <summary>
    /// <para>
    /// <c>CreateBucketAsync</c> creates a new bucket within the system.  A bucket can hold zero to many
    /// content files and serves as a namespace with which to organize content.
    /// </para>
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the content bucket to be created.</param>
    /// <returns></returns>
    Task CreateBucketAsync(IDocumentSession documentSession, string bucketName);

    /// <summary>
    /// <para>
    /// The <c>DeleteBucketAsync</c> method is used to permanently remove a bucket from the database.  The
    /// bucket must be empty in order to delete the bucket unless the <paramref name="force"/> argument is true.  If
    /// <paramref name="force"/> is set to true, all content resources in the bucket will be deleted.
    /// </para> 
    /// </summary>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to update the database.</param>
    /// <param name="bucketName">The name of the content bucket to be created.</param>
    /// <param name="force">Set this value to <c>true</c> to force the destruction of a non-empty bucket.</param>
    /// <exception cref="DeleteFailureException">
    /// <param>If <paramref name="force"/> is <c>false</c> and the bucket is not empty, then a DeleteFailureException will be thrown.</param></exception>
    /// <returns></returns>
    Task DeleteBucketAsync(IDocumentSession documentSession, string bucketName, bool force = false);

    /// <summary>
    /// <para>
    /// The UploadStreamAsync method is used to insert content contained in <paramref name="inStream"/> into the
    /// content repository under the bucket specified in <paramref name="bucketName"/> and the resource path specified
    /// by <paramref name="resourcePath"/>.
    /// </para>
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket in which to place the content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    /// <param name="inStream">A <c>Stream</c> that contains the content to be inserted into the database.</param>
    /// <param name="autoCreateBucket">Whether the bucket specified by <paramref name="bucketName"/> should automatically be created if it is not already present.</param>
    /// <param name="overwriteExisting"><c>true</c> when this call should overwrite any existing resource of the same bucket and file path.</param>
    /// <param name="userGuid">A <c>GUID</c> user specified value.  Defaults to Guid.Empty.  This value is indexed in the database for fast future lookups.  You may want to use this for referencing a user that performed the upload of the content.</param>
    /// <param name="userValue">A <c>long</c> user specified value that can be used to track data related to the content entry.  This value is indexed in the database for fast lookup.  Perhaps a download counter or a primary key value to another document.</param>
    /// <returns></returns>
    /// <exception cref="BucketNotFoundException">If the <paramref name="bucketName"/> is not found and <paramref name="autoCreateBucket"/> is false this exception will be thrown.</exception>
    /// <exception cref="OverwriteNotPermittedException">If the <paramref name="resourcePath"/> is already in the database and <paramref name="overwriteExisting"/> is false, this exception will be thrown./></exception>
    Task UploadStreamAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath resourcePath,
        Stream inStream, bool autoCreateBucket = true, bool overwriteExisting = false, Guid? userGuid = null,
        long userValue = 0L);

    /// <summary>
    /// The DownloadStreamAsync method is used to read content using a standard System.IO.Stream.  The desired content is
    /// addressed through the <paramref name="bucketName"/> and <paramref name="resourcePath"/> arguments.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    /// <returns>A System.IO.Stream that contains the contents of the resource.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket named in the <paramref name="bucketName"/> argument is not found.</exception>
    /// <exception cref="ResourceNotFoundException">Throw when the resource specified in the <paramref name="resourcePath"/> is not found.</exception>
    Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryFilePath resourcePath);

    /// <summary>
    /// The FileExistsAsync method determines if there is a resource at the given <paramref name="bucketName"/> and <paramref name="resourcePath"/> location.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    /// <returns>Returns true if the resource was found.  False if it is not present in the database.</returns>
    Task<bool> FileExistsAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath resourcePath);

    /// <summary>
    /// The DeleteFileAsync method is used to remove content from the repository.  The resource to be removed is
    /// specified by the <paramref name="documentSession"/> and <paramref name="resourcePath"/> arguments.  If the given
    /// resource is not found, then this method returns without error.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    Task DeleteFileAsync(IDocumentSession documentSession, string bucketName, ContentRepositoryFilePath resourcePath);

    /// <summary>
    /// The GetFileInfoAsync method returns information about the resource specified in the <paramref name="bucketName"/>
    /// and <paramref name="resourcePath"/> arguments.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket that holds the desired content.</param>
    /// <param name="resourcePath">A slash separated path to the resource, including filename and extension.  "/myResourcePath/myImage.png"</param>
    /// <returns>A <see cref="ContentRepositoryFileInfo"/> that contains information about the given resource.  This method
    /// may return a null reference if the specified bucket and resource are not found.
    /// </returns>
    Task<ContentRepositoryFileInfo?> GetFileInfoAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryFilePath resourcePath);

    Task RenameFileAsync(IDocumentSession documentSession,
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool replaceDestination = false);

    Task CopyFileAsync(IDocumentSession documentSession,
        string oldBucketName, ContentRepositoryFilePath oldFilePath,
        string newBucketName, ContentRepositoryFilePath newFilePath,
        bool autoCreateBucket = true, bool overwriteDestination = false);

    Task<PagedContentRepositoryFileInfo> GetFileListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);

    Task<PagedContentRepositoryFileInfo> GetFileListingByUserDataGuidAsync(IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize);
}