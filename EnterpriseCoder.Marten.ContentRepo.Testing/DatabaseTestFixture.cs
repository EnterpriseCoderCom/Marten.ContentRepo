using EnterpriseCoder.Marten.ContentRepo.Di;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Weasel.Core;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

// ReSharper disable once ClassNeverInstantiated.Global
public class DatabaseTestFixture : IDisposable
{
    public DatabaseTestFixture()
    {
        var services = new ServiceCollection();

        var connectionBuilder = new NpgsqlConnectionStringBuilder
        {
            Host = "localhost",
            Database = "postgres",
            Username = "postgres",
            Password = "3nterp4is3C0de4",
            Port = 29123,
            Pooling = true,
            MinPoolSize = 1,
            MaxPoolSize = 10,
            SearchPath = "unittesting"
        };

        services.AddMarten(options =>
        {
            options.DatabaseSchemaName = "unittesting";
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.Connection(connectionBuilder.ToString());
        });
        services.AddMartenContentRepo();
        services.AddSingleton<DatabaseHelper>();

        ServiceProvider = services.BuildServiceProvider();
    }

    public IServiceProvider ServiceProvider { get; private set; }


    public void Dispose()
    {
        // Force release all connections.
        NpgsqlConnection.ClearAllPools();
    }
}