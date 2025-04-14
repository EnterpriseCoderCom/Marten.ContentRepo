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

    /// <summary>
    /// The RenameFileAsync method is used to rename a resource from one name to another.  This includes moving a resource
    /// between buckets.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="sourceBucketName">The name of the source bucket.</param>
    /// <param name="sourceResourcePath">The name of the resource to be renamed.</param>
    /// <param name="destinationBucketName">The name of the destination bucket.</param>
    /// <param name="destinationResourcePath">The new name for the resource within the <paramref name="destinationBucketName"/></param>
    /// <param name="replaceDestination">A boolean that indicates if an error should be thrown if there is already a resource in the destination location.</param>
    /// <exception cref="BucketNotFoundException">Thrown when either the <paramref name="sourceBucketName"/> or <paramref name="destinationBucketName"/> is not found.</exception>
    /// <exception cref="ResourceNotFoundException">Throw when there isn't a resource at the location specified by <paramref name="sourceBucketName"/> and <paramref name="sourceResourcePath"/>.</exception>
    /// <exception cref="OverwriteNotPermittedException">Throw when there an existing resource at the specified destination and <paramref name="replaceDestination"/> is false.</exception>
    Task RenameFileAsync(IDocumentSession documentSession,
        string sourceBucketName, ContentRepositoryFilePath sourceResourcePath,
        string destinationBucketName, ContentRepositoryFilePath destinationResourcePath,
        bool replaceDestination = false);

    /// <summary>
    /// The CopyFileAsync method is used to make a copy of an existing resource.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="sourceBucketName">The name of the source bucket.</param>
    /// <param name="sourceResourcePath">The name of the resource to be renamed.</param>
    /// <param name="destinationBucketName">The name of the destination bucket.</param>
    /// <param name="destinationResourcePath">The new name for the resource within the <paramref name="destinationBucketName"/></param>
    /// <param name="autoCreateBucket">Set to true in order to create the destination bucket automatically.</param>
    /// <param name="overwriteDestination">Set to true to overwrite any existing resource at the specified destination location.</param>
    /// <exception cref="BucketNotFoundException">Thrown when either the <paramref name="sourceBucketName"/> or when <paramref name="destinationBucketName"/> is not found and <paramref name="autoCreateBucket"/> is false.</exception>
    /// <exception cref="ResourceNotFoundException">Thrown when there isn't a resource at the location specified by <paramref name="sourceBucketName"/> and <paramref name="sourceResourcePath"/>.</exception>
    /// <exception cref="OverwriteNotPermittedException">Thrown when <paramref name="overwriteDestination"/> is false and there's an existing resource at the given destination location.</exception>
    Task CopyFileAsync(IDocumentSession documentSession,
        string sourceBucketName, ContentRepositoryFilePath sourceResourcePath,
        string destinationBucketName, ContentRepositoryFilePath destinationResourcePath,
        bool autoCreateBucket = true, bool overwriteDestination = false);

    /// <summary>
    /// The GetFileListingAsync method is used to obtain a paged listing of all resources from the bucket specified by
    /// <paramref name="bucketName"/> for all resources that start with <paramref name="resourcePrefix"/>.  The
    /// returned <see cref="PagedContentRepositoryFileInfo"/> contains paging information so that large repositories
    /// listings can be handled in a memory-safe way.
    /// </summary>
    /// <param name="documentSession">A Marten documentSession that will be used to communicate with the database.</param>
    /// <param name="bucketName">The name of the bucket to be searched.</param>
    /// <param name="resourcePrefix">The resource prefix to be searched.  For example: /images to return information about all resources that start with "/images".</param>
    /// <param name="oneBasedPage">The one-based page to be returned by this call.</param>
    /// <param name="pageSize">The desired size for the returned page of information.</param>
    /// <param name="recursive">Set to true to return all resources under <paramref name="resourcePrefix"/>.  Set to false in order to return only resources that are directly in the given prefix pseudo-directory.</param>
    /// <returns>Returns a <see cref="PagedContentRepositoryFileInfo"/> object that contains the items for the requested page as
    /// well as information about the total number of pages.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    Task<PagedContentRepositoryFileInfo> GetFileListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory resourcePrefix,
        int oneBasedPage, int pageSize,
        bool recursive = false);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="documentSession"></param>
    /// <param name="bucketName"></param>
    /// <param name="userGuid"></param>
    /// <param name="oneBasedPage"></param>
    /// <param name="pageSize"></param>
    /// <returns></returns>
    Task<PagedContentRepositoryFileInfo> GetFileListingByUserDataGuidAsync(IDocumentSession documentSession,
        string bucketName, Guid userGuid, int oneBasedPage, int pageSize);
}