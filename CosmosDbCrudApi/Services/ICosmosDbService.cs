using CosmosDbCrudApi.Models;

namespace CosmosDbCrudApi.Services;

public interface ICosmosDbService
{
    Task<IEnumerable<Item>> GetItemsAsync(string queryString);
    Task<Item?> GetItemAsync(string id, string partitionKey);
    Task AddItemAsync(Item item);
    Task UpdateItemAsync(string id, Item item);
    Task DeleteItemAsync(string id, string partitionKey);
}