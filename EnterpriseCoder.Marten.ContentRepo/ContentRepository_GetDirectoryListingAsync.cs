using EnterpriseCoder.Marten.ContentRepo.Entities;
using Marten;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository
{
    public async Task<IReadOnlyList<ContentRepositoryDirectoryInfo>> GetDirectoryListingAsync(IDocumentSession documentSession,
        string bucketName, ContentRepositoryDirectory baseDirectory)
    {
        var workList = await documentSession.Query<ContentResourceHeader>()
            .Where(x => x.Directory.StartsWith(baseDirectory))
            .Where(x => x.Directory != baseDirectory)
            .ToListAsync();

        List<ContentRepositoryDirectoryInfo> returnList = new();
        HashSet<string> seenDirectories = new();

        foreach (var nextItem in workList)
        {
            if (nextItem.Directory == baseDirectory)
            {
                // don't care about items in the base directory
            }
            
            ContentRepositoryDirectory rightPartial = nextItem.Directory.Substring(baseDirectory.Path.Length);
            var parts = rightPartial.SplitPath();

            if (parts.Length == 0)
            {
                continue;
            }

            string childDirectory = parts[0];

            if (seenDirectories.Contains(childDirectory))
            {
                continue; // we've already seen this directory.
            }
            
            var newDirectory = new ContentRepositoryDirectoryInfo(nextItem.Directory, childDirectory);
            returnList.Add(newDirectory);
            
            seenDirectories.Add(childDirectory);
        }
        
        return returnList;
    }
}