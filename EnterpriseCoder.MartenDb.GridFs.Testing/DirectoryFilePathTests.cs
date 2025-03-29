namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class DirectoryFilePathTests
{
    [Fact]
    public void ConstructorAndParentTests()
    {
        Assert.Throws<ArgumentException>(() => new GridFsDirectory(""));
        
        GridFsDirectory rootDirectory = new GridFsDirectory("/");
        Assert.Equal("/", rootDirectory.Path);
        Assert.Throws<ArgumentException>(() => rootDirectory.Parent);
        
        rootDirectory = new GridFsDirectory("/childDirectory");
        Assert.Equal("/childDirectory", rootDirectory.Path);
        Assert.Equal("/", rootDirectory.Parent);
 
        rootDirectory = new GridFsDirectory("/parentDirectory/childDirectory");
        Assert.Equal("/parentDirectory/childDirectory", rootDirectory.Path);
        Assert.Equal("/parentDirectory", rootDirectory.Parent);
        
        Assert.Throws<ArgumentException>(() => new GridFsDirectory("parentDirectory/childDirectory/"));

    }
}