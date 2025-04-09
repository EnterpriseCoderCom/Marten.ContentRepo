using EnterpriseCoder.Marten.ContentRepo.Exceptions;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepositoryFilePathTests
{
    [Fact]
    public void AllTests()
    {
        Assert.Throws<InvalidPathException>(() => new ContentRepositoryFilePath(""));
        Assert.Throws<InvalidPathException>(() => new ContentRepositoryFilePath("/"));

        var path = new ContentRepositoryFilePath("test.dat");
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