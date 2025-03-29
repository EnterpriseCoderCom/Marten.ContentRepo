﻿using EnterpriseCoder.MartenDb.GridFs.Di;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Weasel.Core;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class DatabaseTestFixture : IDisposable
{
    public IServiceProvider ServiceProvider { get; private set; }
    
    public DatabaseTestFixture()
    {
        var services = new ServiceCollection();

        services.AddMarten(options =>
        {
            options.DatabaseSchemaName = "unittesting";
            options.AutoCreateSchemaObjects = AutoCreate.All;

            NpgsqlConnectionStringBuilder connectionBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = "localhost",
                Database = "postgres",
                Username = "postgres",
                Password = "3nterp4is3C0de4",
                Port = 29123,
                Pooling = true,
                MinPoolSize = 1,
                MaxPoolSize = 10
            };

            options.Connection(connectionBuilder.ToString());
        });
        services.AddMartenDbGridFs();

        ServiceProvider = services.BuildServiceProvider();
    }
    
    
    public void Dispose()
    {
        // Force release all connections.
        NpgsqlConnection.ClearAllPools();
    }
}