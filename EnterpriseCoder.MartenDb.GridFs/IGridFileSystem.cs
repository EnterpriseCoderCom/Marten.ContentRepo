using Marten;

namespace EnterpriseCoder.MartenDb.GridFs;

public interface IGridFileSystem
{
    IDocumentSession DocumentSession { get; }
    
    Task SaveStreamAsync(GridFsFilePath filePath, Stream inStream, Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> LoadStreamAsync(GridFsFilePath filePath);
    Task<bool> FileExistsAsync(GridFsFilePath filePath);
    Task DeleteFileAsync(GridFsFilePath filePath);
    Task<GridFsFileInfo?> GetFileInfoAsync(GridFsFilePath filePath);
}