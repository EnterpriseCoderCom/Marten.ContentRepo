using EnterpriseCoder.Marten.ContentRepo.Procedures;

namespace EnterpriseCoder.Marten.ContentRepo;

public partial class ContentRepository : IContentRepository
{
    private const int FileBlockSize = 65535;

    private readonly ContentFileHeaderProcedures _fileHeaderProcedures = new();
    private readonly ContentFileBlockProcedures _fileBlockProcedures = new();
    
}