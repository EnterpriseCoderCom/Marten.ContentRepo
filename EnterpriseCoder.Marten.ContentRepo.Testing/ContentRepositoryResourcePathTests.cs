using EnterpriseCoder.Marten.ContentRepo.Exceptions;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepositoryResourcePathTests
{
    [Fact]
    public void AllTests()
    {
        Assert.Throws<InvalidPathException>(() => new ContentRepositoryResourcePath(""));
        Assert.Throws<InvalidPathException>(() => new ContentRepositoryResourcePath("/"));

        var path = new ContentRepositoryResourcePath("test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new ContentRepositoryResourcePath("/test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new ContentRepositoryResourcePath("/parent/test.dat");
        Assert.Equal("/parent", path.Directory);
        Assert.Equal("/parent/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);
    }
}