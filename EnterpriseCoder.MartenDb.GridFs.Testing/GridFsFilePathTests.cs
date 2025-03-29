namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class GridFsFilePathTests
{
    [Fact]
    public void AllTests()
    {
        Assert.Throws<ArgumentException>(() => new GridFsFilePath(""));
        Assert.Throws<ArgumentException>(() => new GridFsFilePath("/"));

        GridFsFilePath path = new GridFsFilePath("test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new GridFsFilePath("/test.dat");
        Assert.Equal("/", path.Directory);
        Assert.Equal("/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);

        path = new GridFsFilePath("/parent/test.dat");
        Assert.Equal("/parent", path.Directory);
        Assert.Equal("/parent/test.dat", path.Path);
        Assert.Equal("test.dat", path.Filename);
        Assert.Equal("test", path.FilenameNoExtension);
        Assert.Equal(".dat", path.FileExtension);
    }    
}