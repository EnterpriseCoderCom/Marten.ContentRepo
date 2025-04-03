using Marten;

namespace EnterpriseCoder.MartenDb.GridFs.Di;

public interface IGridFileSystemScoped
{
    IDocumentSession DocumentSession { get; }

    Task UploadStreamAsync(GridFsFilePath filePath, Stream inStream, bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> DownloadStreamAsync(GridFsFilePath filePath);
    Task<bool> FileExistsAsync(GridFsFilePath filePath);
    Task DeleteFileAsync(GridFsFilePath filePath);
    Task<GridFsFileInfo?> GetFileInfoAsync(GridFsFilePath filePath);
    Task RenameFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath, bool overwriteDestination = false);
    Task CopyFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath, bool overwriteDestination = false);
    Task<IList<GridFsFileInfo>> GetFileListingAsync(GridFsDirectory directory, int oneBasedPage, int pageSize, bool recursive = false);
}