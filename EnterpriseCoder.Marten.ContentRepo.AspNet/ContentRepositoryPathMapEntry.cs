namespace EnterpriseCoder.Marten.ContentRepo.AspNet;

public class ContentRepositoryPathMapEntry
{
    public ContentRepositoryPathMapEntry(string uriPathPrefix, string bucketName, string contentPathPrefix)
    {
        UriPathPrefix = uriPathPrefix;
        BucketName = bucketName;
        ContentPathPrefix = contentPathPrefix;
    }

    public string UriPathPrefix { get; }
    public string BucketName { get; }
    public string ContentPathPrefix { get; }
}