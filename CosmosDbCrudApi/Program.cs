using CosmosDbCrudApi.Services;
using Microsoft.Azure.Cosmos;
using Scalar.AspNetCore;

namespace CosmosDbCrudApi;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        // Configure Cosmos DB Client and Service
        builder.Services.AddSingleton<ICosmosDbService>(serviceProvider =>
        {
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var endpointUri = configuration["CosmosDb:EndpointUri"];
            var primaryKey = configuration["CosmosDb:PrimaryKey"];
            var databaseName = configuration["CosmosDb:DatabaseName"];
            var containerName = configuration["CosmosDb:ContainerName"];

            if (string.IsNullOrEmpty(endpointUri) || string.IsNullOrEmpty(primaryKey) || string.IsNullOrEmpty(databaseName) || string.IsNullOrEmpty(containerName))
            {
                throw new InvalidOperationException("Cosmos DB configuration is missing or incomplete.");
            }
    
            // For emulator, allow insecure connections
            var cosmosClientOptions = new CosmosClientOptions
            {
                HttpClientFactory = () =>
                {
                    HttpMessageHandler httpMessageHandler = new HttpClientHandler()
                    {
                        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                    };
                    return new HttpClient(httpMessageHandler);
                },
                ConnectionMode = ConnectionMode.Gateway // Gateway mode is often simpler for local dev with emulator
            };

            var cosmosClient = new CosmosClient(endpointUri, primaryKey, cosmosClientOptions);
    
            // Ensure Database and Container exist (run this once during startup)
            // This is a blocking call, consider moving to a hosted service for production
            // or ensure it's handled gracefully.
            Database database = cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName).GetAwaiter().GetResult();
            database.CreateContainerIfNotExistsAsync(
                containerName, 
                "/partitionKey", // Define the partition key path
                throughput: 400   // Minimum throughput
            ).GetAwaiter().GetResult();

            return new CosmosDbService(cosmosClient, configuration);
        });
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}