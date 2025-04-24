namespace EnterpriseCoder.Marten.ContentRepo.AspNet;

public class ContentRepositoryPathMapBuilder
{
    private ContentRepositoryPathMap _map = new();

    public ContentRepositoryPathMapBuilder AddMapping(string uriPathPrefix, string bucketName, string contentPathPrefix)
    {
        _map.AddMapping(new ContentRepositoryPathMapEntry(uriPathPrefix, bucketName, contentPathPrefix));

        return this;
    }

    internal ContentRepositoryPathMap Build() => _map;
}