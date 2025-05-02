using System.Collections;

namespace EnterpriseCoder.Marten.ContentRepo.AspNet;

public class ContentRepositoryPathMap : IEnumerable<ContentRepositoryPathMapEntry>
{
    private List<ContentRepositoryPathMapEntry> _mapEntries = new();

    public void AddMapping(ContentRepositoryPathMapEntry newEntry)
    {
        string normalizedUriPathPrefix = newEntry.UriPathPrefix.TrimEnd('/');
        if (normalizedUriPathPrefix.StartsWith("/") is false)
        {
            normalizedUriPathPrefix = $"/{normalizedUriPathPrefix}";
        }
        
        string normalizedContentPathPrefix = newEntry.ContentPathPrefix.TrimEnd('/');
        if (normalizedContentPathPrefix.StartsWith("/") is false)
        {
            normalizedContentPathPrefix = $"/{normalizedContentPathPrefix}";
        }

        _mapEntries.Add(new ContentRepositoryPathMapEntry(normalizedUriPathPrefix, newEntry.BucketName,
            normalizedContentPathPrefix));
    }

    public IEnumerator<ContentRepositoryPathMapEntry> GetEnumerator()
    {
        return _mapEntries.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}