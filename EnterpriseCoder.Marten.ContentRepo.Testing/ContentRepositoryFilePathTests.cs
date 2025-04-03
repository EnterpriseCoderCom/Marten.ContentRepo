namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepositoryFilePathTests
{
    [Fact]
    public void AllTests()
    {
        Assert.Throws<ArgumentException>(() => new ContentRepositoryFilePath(""));
        Assert.Throws<ArgumentException>(() => new ContentRepositoryFilePath("/"));

        ContentRepositoryFilePath path = new ContentRepositoryFilePath("test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new ContentRepositoryFilePath("/test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new ContentRepositoryFilePath("/parent/test.dat");
        Assert.Equal("/parent", path.Directory);
        Assert.Equal("/parent/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);
    }    
}