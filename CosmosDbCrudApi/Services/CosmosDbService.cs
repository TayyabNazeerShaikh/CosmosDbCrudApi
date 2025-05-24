using System.Net;
using CosmosDbCrudApi.Models;
using Microsoft.Azure.Cosmos;

namespace CosmosDbCrudApi.Services;

public class CosmosDbService : ICosmosDbService
{
    private readonly Container _container;
    private readonly IConfiguration _configuration;

    public CosmosDbService(CosmosClient cosmosClient, IConfiguration configuration)
    {
        _configuration = configuration;
        var databaseName = _configuration["CosmosDb:DatabaseName"];
        var containerName = _configuration["CosmosDb:ContainerName"];
        _container = cosmosClient.GetContainer(databaseName, containerName);
    }

    public async Task AddItemAsync(Item item)
    {
        // Ensure Id is set if not already
        if (string.IsNullOrEmpty(item.Id))
        {
            item.Id = Guid.NewGuid().ToString();
        }
        await _container.CreateItemAsync(item, new PartitionKey(item.PartitionKey));
    }

    public async Task DeleteItemAsync(string id, string partitionKey)
    {
        await _container.DeleteItemAsync<Item>(id, new PartitionKey(partitionKey));
    }

    public async Task<Item?> GetItemAsync(string id, string partitionKey)
    {
        try
        {
            ItemResponse<Item> response = await _container.ReadItemAsync<Item>(id, new PartitionKey(partitionKey));
            return response.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    public async Task<IEnumerable<Item>> GetItemsAsync(string queryString)
    {
        var query = _container.GetItemQueryIterator<Item>(new QueryDefinition(queryString));
        var results = new List<Item>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            results.AddRange(response.ToList());
        }
        return results;
    }

    public async Task UpdateItemAsync(string id, Item item)
    {
        // Ensure the ID in the path matches the ID in the body
        if (item.Id != id)
        {
            // Or throw an ArgumentException, depending on desired behavior
            item.Id = id; 
        }
        await _container.UpsertItemAsync(item, new PartitionKey(item.PartitionKey));
    }
}