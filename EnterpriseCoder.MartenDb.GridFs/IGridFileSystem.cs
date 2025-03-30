using Marten;

namespace EnterpriseCoder.MartenDb.GridFs;

public interface IGridFileSystem
{
    IDocumentSession DocumentSession { get; }
    
    Task UploadStreamAsync(GridFsFilePath filePath, Stream inStream, bool overwriteExisting = false, Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> DownLoadStreamAsync(GridFsFilePath filePath);
    Task<bool> FileExistsAsync(GridFsFilePath filePath);
    Task DeleteFileAsync(GridFsFilePath filePath);
    Task<GridFsFileInfo?> GetFileInfoAsync(GridFsFilePath filePath);
    Task RenameFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath, bool overwriteDestination = false);
    Task CopyFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath, bool overwriteDestination = false);
}