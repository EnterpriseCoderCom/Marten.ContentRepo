using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    /// <summary>
    /// Gets a listing of subdirectories at the specified path, including both explicit directories
    /// and implicit directories (derived from file paths).
    /// </summary>
    /// <remarks>
    /// This method returns a unified view of the directory structure by combining:
    /// 1. Explicitly created directories (stored as ContentDirectory entities)
    /// 2. Implicit directories derived from file paths in ContentResourceHeader
    ///
    /// The listing includes unique directory names at the immediate child level, automatically
    /// deduplicating any directories that exist in both sources.
    /// </remarks>
    /// <param name="documentSession">A Marten <c>IDocumentSession</c> that will be used to query the database.</param>
    /// <param name="bucketName">The name of the bucket to be searched.</param>
    /// <param name="baseDirectory">A slash-separated path to the base directory (e.g., "/myDirectory").</param>
    /// <returns>Returns an <see cref="IReadOnlyList{ContentRepositoryDirectoryInfo}"/> containing information about subdirectories.</returns>
    /// <exception cref="BucketNotFoundException">Thrown when the bucket specified by <paramref name="bucketName"/> is not found.</exception>
    public async Task<IReadOnlyList<ContentRepositoryDirectoryInfo>> GetDirectoryListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory baseDirectory)
    {
        // Get the bucket to retrieve its ID for querying explicit directories
        var targetBucket = await _contentBucketProcedures.SelectBucketAsync(documentSession, bucketName);
        if (targetBucket == null)
        {
            throw new BucketNotFoundException(bucketName);
        }

        // Get implicit directories from file paths (existing logic)
        var implicitDirs = await GetImplicitChildDirectoriesAsync(documentSession, baseDirectory);

        // Get explicit directories from ContentDirectory table
        var explicitDirs = await GetExplicitChildDirectoriesAsync(documentSession, targetBucket.Id, baseDirectory);

        // Merge results - union automatically deduplicates
        var mergedDirs = new HashSet<string>(explicitDirs);
        mergedDirs.UnionWith(implicitDirs);

        // Build return list, sorted by directory name
        List<ContentRepositoryDirectoryInfo> returnList = new();

        foreach (var childName in mergedDirs.OrderBy(x => x))
        {
            ContentRepositoryDirectory childDirectoryFullPath = baseDirectory + "/" + childName;
            var newDirectory = new ContentRepositoryDirectoryInfo(childDirectoryFullPath, childName);
            returnList.Add(newDirectory);
        }

        return returnList;
    }

    /// <summary>
    /// Gets the names of immediate child directories derived from file paths.
    /// </summary>
    private async Task<HashSet<string>> GetImplicitChildDirectoriesAsync(
        IDocumentSession documentSession,
        ContentRepositoryDirectory baseDirectory)
    {
        var workList = await documentSession.Query<ContentResourceHeader>()
            .Where(x => x.Directory.StartsWith(baseDirectory))
            .Where(x => x.Directory != baseDirectory)
            .ToListAsync();

        var childDirectories = new HashSet<string>();

        foreach (var nextItem in workList)
        {
            ContentRepositoryDirectory rightPartial = nextItem.Directory.Substring(baseDirectory.Path.Length);
            var parts = rightPartial.SplitPath();

            if (parts.Length > 0)
            {
                childDirectories.Add(parts[0]);
            }
        }

        return childDirectories;
    }

    /// <summary>
    /// Gets the names of immediate child directories from explicitly created directories.
    /// </summary>
    private async Task<HashSet<string>> GetExplicitChildDirectoriesAsync(
        IDocumentSession documentSession,
        Guid bucketId,
        ContentRepositoryDirectory baseDirectory)
    {
        // Determine the prefix for querying child directories
        var prefix = baseDirectory.Path == "/" ? "/" : baseDirectory.Path + "/";

        var explicitDirs = await documentSession.Query<ContentDirectory>()
            .Where(x => x.BucketId == bucketId)
            .Where(x => x.DirectoryPath.StartsWith(prefix))
            .ToListAsync();

        var childDirectories = new HashSet<string>();

        foreach (var dir in explicitDirs)
        {
            // Extract immediate child name from the full path
            // Special handling for root directory
            string relativePath;
            if (baseDirectory.Path == "/")
            {
                // Remove leading slash for root directory
                relativePath = dir.DirectoryPath.Substring(1);
            }
            else
            {
                // Remove base directory path and the following slash
                relativePath = dir.DirectoryPath.Substring(baseDirectory.Path.Length + 1);
            }

            var parts = relativePath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 0)
            {
                childDirectories.Add(parts[0]);
            }
        }

        return childDirectories;
    }
}