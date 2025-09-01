namespace EnterpriseCoder.Marten.ContentRepo;

public class ContentRepositoryDirectoryInfo
{
    public ContentRepositoryDirectory Directory { get; init; }
    public string ChildDirectoryName { get; init; }
    
    public ContentRepositoryDirectoryInfo(ContentRepositoryDirectory directory, string childDirectoryName)
    {
        Directory = directory;
        ChildDirectoryName = childDirectoryName;
    }

    public override string ToString()
    {
        return $"{nameof(ChildDirectoryName)}: {ChildDirectoryName}";
    }
}