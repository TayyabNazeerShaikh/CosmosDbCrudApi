using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace CosmosDbCrudApi.Models;

public class Item
{
    [JsonPropertyName("id")]
    [JsonProperty("id")] 
    public string Id { get; set; } = Guid.NewGuid().ToString(); // Auto-generate ID

    [JsonPropertyName("partitionKey")]
    [JsonProperty("partitionKey")] 
    public string PartitionKey { get; set; } = "Default"; // Example: Use a category, tenantId, etc.

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsComplete { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
}