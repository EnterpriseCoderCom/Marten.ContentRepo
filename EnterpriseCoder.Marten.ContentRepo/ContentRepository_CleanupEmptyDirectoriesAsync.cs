using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Cleans up empty explicit directories in the hierarchy, starting from the given directory
    /// and cascading up to the root.
    /// </summary>
    /// <remarks>
    /// This method is called after resource deletion to remove empty explicit directories.
    /// It only removes explicit directories (those stored in the database).
    /// Implicit directories (derived from file paths) are automatically removed when no files reference them.
    /// The cleanup stops when:
    /// - A non-empty directory is found
    /// - The root directory ("/") is reached
    /// - An implicit-only directory is encountered
    /// </remarks>
    /// <param name="documentSession">A Marten IDocumentSession for transaction management.</param>
    /// <param name="bucketId">The ID of the bucket containing the directories.</param>
    /// <param name="startingDirectory">The directory to start cleanup from (typically the parent of the deleted resource).</param>
    private async Task CleanupEmptyDirectoriesAsync(
        IDocumentSession documentSession,
        Guid bucketId,
        ContentRepositoryDirectory startingDirectory)
    {
        var directoryToCheck = startingDirectory;

        while (directoryToCheck.Path != "/")
        {
            // 1. Check if directory has files
            var hasFiles = await documentSession.Query<ContentResourceHeader>()
                .AnyAsync(x => x.BucketId == bucketId &&
                              (x.Directory == directoryToCheck.Path ||
                               x.Directory.StartsWith(directoryToCheck.Path + "/")));

            if (hasFiles)
                break;  // Stop, directory has content

            // 2. Check if directory has subdirectories
            var hasSubdirectories = await documentSession.Query<ContentDirectory>()
                .AnyAsync(x => x.BucketId == bucketId &&
                              x.DirectoryPath.StartsWith(directoryToCheck.Path + "/"));

            if (hasSubdirectories)
                break;  // Stop, directory has subdirectories

            // 3. Try to delete the explicit directory if it exists
            var explicitDir = await documentSession.Query<ContentDirectory>()
                .FirstOrDefaultAsync(x => x.BucketId == bucketId &&
                                         x.DirectoryPath == directoryToCheck.Path);

            if (explicitDir != null)
            {
                documentSession.Delete(explicitDir);
            }

            // 4. Move to parent directory
            try
            {
                directoryToCheck = directoryToCheck.Parent;
            }
            catch
            {
                // If Parent throws (shouldn't happen given the while condition), we're done
                break;
            }
        }
    }
}
