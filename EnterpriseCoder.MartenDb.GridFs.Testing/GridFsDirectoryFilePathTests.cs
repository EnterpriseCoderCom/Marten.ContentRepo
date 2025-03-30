namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class GridFsDirectoryFilePathTests
{
    [Fact]
    public void ConstructorAndParentTests()
    {
        Assert.Throws<ArgumentException>(() => new GridFsDirectory(""));
        
        GridFsDirectory rootDirectory = new GridFsDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        
        GridFsDirectory captureRootDirectory = new GridFsDirectory("/");
        Assert.Throws<ArgumentException>(() => captureRootDirectory.Parent);
        
        rootDirectory = new GridFsDirectory("/childDirectory");
        Assert.Equal("/childDirectory", rootDirectory.Path);
        Assert.Equal("/", rootDirectory.Parent);
 
        rootDirectory = new GridFsDirectory("/parentDirectory/childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", rootDirectory.Path);
        Assert.Equal("/parentDirectory", rootDirectory.Parent);

        rootDirectory = new GridFsDirectory("/parentDirectory/childDirectory/./..");
        Assert.Equal("/parentDirectory", rootDirectory.Path);
        Assert.Equal("/", rootDirectory.Parent);

        
    }

    [Fact]
    public void RelativePathTests()
    {
        GridFsDirectory rootDirectory = new GridFsDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        Assert.Throws<ArgumentException>(() => rootDirectory.Parent);

        GridFsDirectory childDirectory = rootDirectory.Combine("parentDirectory", "childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);
        
        childDirectory = rootDirectory.Combine("/parentDirectory\\childDirectory\\");
        Assert.Equal("/parentDirectory/childDirectory", childDirectory.Path);

        childDirectory = childDirectory.Combine("..");
        Assert.Equal("/parentDirectory", childDirectory.Path);
        
        childDirectory = childDirectory.Combine("/.");
        Assert.Equal("/parentDirectory", childDirectory.Path);
    }
}