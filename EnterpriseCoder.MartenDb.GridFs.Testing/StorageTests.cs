using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class StorageTests : IClassFixture<DatabaseTestFixture>
{
    private readonly IDocumentSession _documentSession;
    private readonly IGridFileSystem _gridFileSystem;
    
    public StorageTests(DatabaseTestFixture databaseFixture)
    {
        _documentSession =
            databaseFixture.ServiceProvider.GetRequiredService<IDocumentSession>();
        
        _gridFileSystem = databaseFixture.ServiceProvider.GetRequiredService<IGridFileSystem>();
    }

    [Fact]
    public void StoreFileAndRetrieve()
    {
    }
}