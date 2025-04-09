using EnterpriseCoder.Marten.ContentRepo.Exceptions;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepoDirectoryFilePathTests
{
    [Fact]
    public void ConstructorAndParentTests()
    {
        Assert.Throws<InvalidPathException>(() => new ContentRepositoryDirectory(""));

        var rootDirectory = new ContentRepositoryDirectory("/");
        Assert.Equal("/", rootDirectory.Path);

        var captureRootDirectory = new ContentRepositoryDirectory("/");
        Assert.Throws<InvalidPathException>(() => captureRootDirectory.Parent);

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
        var rootDirectory = new ContentRepositoryDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        Assert.Throws<InvalidPathException>(() => rootDirectory.Parent);

        var childDirectory = rootDirectory.Combine("parentDirectory", "childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);

        childDirectory = rootDirectory.Combine("/parentDirectory\\childDirectory\\");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);

        childDirectory = childDirectory.Combine("..");
        Assert.Equal("/parentDirectory", childDirectory.Path);

        childDirectory = childDirectory.Combine("/.");
        Assert.Equal("/parentDirectory", childDirectory.Path);
    }
}