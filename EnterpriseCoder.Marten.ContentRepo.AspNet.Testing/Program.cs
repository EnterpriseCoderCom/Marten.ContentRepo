using System.Diagnostics;
using System.Reflection;
using EnterpriseCoder.Marten.ContentRepo.AspNet;
using EnterpriseCoder.Marten.ContentRepo.Di;
using Marten;
using Npgsql;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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

builder.Services.AddMarten(options =>
    {
        options.DatabaseSchemaName = "unittesting";
        options.AutoCreateSchemaObjects = AutoCreate.All;
        options.Connection(connectionBuilder.ToString());
    })
    .UseLightweightSessions();

builder.Services.AddMartenContentRepo();
builder.Services.MapContentRepository(config =>
{
    config
        .AddMapping("/images", "images", "/userImages")
        .AddMapping("/documents", "documents", "/userDocuments");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Insert the middleware to handle content repository requests
app.UseContentRepository();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Make sure the test buckets and resources are in the database

using (var scope = app.Services.CreateScope())
{
    var contentRepo = scope.ServiceProvider.GetRequiredService<IContentRepositoryScoped>();
    if (await contentRepo.BucketExistsAsync("images") is false)
    {
        await contentRepo.CreateBucketAsync("images");
    }

    if (await contentRepo.BucketExistsAsync("documents") is false)
    {
        await contentRepo.CreateBucketAsync("documents");
    }

    await contentRepo.DocumentSession.SaveChangesAsync();

    // Insert the test documents
    string runDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!;
    string testFilePath = Path.Combine(runDirectory, "TestFiles", "angrybird.png");
    Debug.Assert(File.Exists(testFilePath));

    // Insert /userImages/angrybird.png
    if (await contentRepo.ResourceExistsAsync("images", "/userImages/angrybird.png") is false)
    {
        await using (var stream = File.OpenRead(testFilePath))
        {
            await contentRepo.UploadStreamAsync("images", "/userImages/angrybird.png", stream);
        }
        await contentRepo.DocumentSession.SaveChangesAsync();
    }

    // Insert /userDocuments/angrybird.png
    if (await contentRepo.ResourceExistsAsync("documents", "/userDocuments/angrybird.png") is false)
    {
        await using (var stream = File.OpenRead(testFilePath))
        {
            await contentRepo.UploadStreamAsync("documents", "/userDocuments/angrybird.png", stream);
        }
        await contentRepo.DocumentSession.SaveChangesAsync();
    }
    
    // Insert /userDocuments/images/angrybird.png
    if (await contentRepo.ResourceExistsAsync("documents", "/userDocuments/images/angrybird.png") is false)
    {
        await using (var stream = File.OpenRead(testFilePath))
        {
            await contentRepo.UploadStreamAsync("documents", "/userDocuments/images/angrybird.png", stream);
        }
        await contentRepo.DocumentSession.SaveChangesAsync();
    }
}


app.Run();