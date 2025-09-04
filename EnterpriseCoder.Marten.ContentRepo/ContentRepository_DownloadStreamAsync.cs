using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
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
    public async Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, string bucketName,
        ContentRepositoryResourcePath resourcePath)
    {
        Tuple<Stream, ContentRepositoryResourceInfo> result =
            await DownloadStreamWithHeaderAsync(documentSession, bucketName, resourcePath);
        return result.Item1;
    }
}