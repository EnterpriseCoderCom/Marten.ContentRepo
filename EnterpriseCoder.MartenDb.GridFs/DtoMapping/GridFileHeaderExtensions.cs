using EnterpriseCoder.MartenDb.GridFs.Entities;

namespace EnterpriseCoder.MartenDb.GridFs.DtoMapping;

public static class GridFileHeaderExtensions
{
    public static GridFsFileInfo ToGridFsFileInfoDto(this GridFileHeader inHeader)
    {
        return new GridFsFileInfo()
        {
            FilePath = inHeader.FilePath,
            StoredLength = inHeader.StoredLength,
            UserDataLong = inHeader.UserDataLong,
            UserDataGuid = inHeader.UserDataGuid,
            Sha256 = inHeader.Sha256,
            OriginalLength = inHeader.OriginalLength,
            UpdateDateTime = inHeader.UpdatedDateTime
        };
    }
}