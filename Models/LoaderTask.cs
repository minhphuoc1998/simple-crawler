using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApi.Models;

public class LoaderDatabaseSettings
{
    public string? ConnectionString { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
    public string? CollectionName { get; set; } = null;
}

public enum LoadType
{
    HTTP,
    BROWSER
}

public enum TaskStatus
{
    CREATED,
    RUNNING,
    SUCCESS,
    ERROR
}

public class LoaderTask
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    
    public string? Url { get; set; }
    [BsonRepresentation(BsonType.String)]
    public TaskStatus Status { get; set; } = TaskStatus.CREATED;
    public string CallbackType { get; set; } = "default";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}

public class TaskQueryParameters
{
    public string? Url { get; set; }
    public LoadType? LoadType { get; set; }
    public TaskStatus? Status { get; set; }
    public string? CallbackType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool Ascending { get; set; } = true;
}

public class TaskUpdateParameters
{
    public TaskStatus ? Status { get; set; }
}
