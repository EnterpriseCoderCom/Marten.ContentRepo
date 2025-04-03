namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepoDirectoryFilePathTests
{
    [Fact]
    public void ConstructorAndParentTests()
    {
        Assert.Throws<ArgumentException>(() => new ContentRepositoryDirectory(""));
        
        ContentRepositoryDirectory rootDirectory = new ContentRepositoryDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        
        ContentRepositoryDirectory captureRootDirectory = new ContentRepositoryDirectory("/");
        Assert.Throws<ArgumentException>(() => captureRootDirectory.Parent);
        
        rootDirectory = new ContentRepositoryDirectory("/childDirectory");
        Assert.Equal("/childDirectory", rootDirectory.Path);
        Assert.Equal("/", rootDirectory.Parent);
 
        rootDirectory = new ContentRepositoryDirectory("/parentDirectory/childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", rootDirectory.Path);
        Assert.Equal("/parentDirectory", rootDirectory.Parent);

        rootDirectory = new ContentRepositoryDirectory("/parentDirectory/childDirectory/./..");
        Assert.Equal("/parentDirectory", rootDirectory.Path);
        Assert.Equal("/", rootDirectory.Parent);
    }

    [Fact]
    public void RelativePathTests()
    {
        ContentRepositoryDirectory rootDirectory = new ContentRepositoryDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        Assert.Throws<ArgumentException>(() => rootDirectory.Parent);

        ContentRepositoryDirectory childDirectory = rootDirectory.Combine("parentDirectory", "childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);
        
        childDirectory = rootDirectory.Combine("/parentDirectory\\childDirectory\\");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);

        childDirectory = childDirectory.Combine("..");
        Assert.Equal("/parentDirectory", childDirectory.Path);
        
        childDirectory = childDirectory.Combine("/.");
        Assert.Equal("/parentDirectory", childDirectory.Path);
    }
}