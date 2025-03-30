﻿namespace EnterpriseCoder.MartenDb.GridFs;

public class GridFsFileInfo
{
    public GridFsFilePath FilePath { get; set; } = null!;
    public byte[] Sha256 { get; set; } = Array.Empty<byte>();
    public DateTimeOffset UpdateDateTime { get; set; }
    public long OriginalLength { get; set; }
    public long StoredLength { get; set; }
    public Guid UserDataGuid { get; set; } = Guid.Empty;
    public long UserDataLong { get; set; }
}
