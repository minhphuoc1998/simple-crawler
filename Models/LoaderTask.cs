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
    PENDING,
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
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public string[] ErrorMessages { get; set; } = [];
    public string? ParentTaskId { get; set; }
    public string PathId { get; set; } = "";
    public int NTry { get; set; } = 0;
}

public class LoaderTaskDetail : LoaderTask {
    public int TotalDescendant { get; set; } = 0;
    public int TotalDescendantSuccess { get; set; } = 0;
    public int TotalDescendantError { get; set; } = 0;
    public int TotalDescendantPending { get; set; } = 0;
    public int TotalDescendantRunning { get; set; } = 0;
}

public class TaskQueryParameters
{
    public string? Url { get; set; }
    public TaskStatus? Status { get; set; }
    public string? CallbackType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "CreatedAt";
    public bool Ascending { get; set; } = true;
    public TaskStatus[]? Statuses { get; set; }
    public string? PathId { get; set; }
    public string? ParentTaskId { get; set; }
    public bool Root { get; set; } = false;
}

public class TaskUpdateParameters
{
    public TaskStatus ? Status { get; set; }
    public string[]? ErrorMessages { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.Now;
    public int? NTry { get; set; }
}
