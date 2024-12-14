using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace WebApi.Models;

public class ArticleDatabaseSettings
{
    public string? ConnectionString { get; set; } = null;
    public string? DatabaseName { get; set; } = null;
    public string? CollectionName { get; set; } = null;
}

public class Article
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public required string Url { get; set; }
    public string? Origin { get; set; }
    public DateTime? PublishedAt { get; set; } = DateTime.Now;
    public int Like { get; set; } = 0;
}

public class ArticleQueryParameters
{
    public string? Url { get; set; }
    public string? Origin { get; set; }
    public string? Id { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string SortBy { get; set; } = "PublishedAt";
    public bool Ascending { get; set; } = true;
}

public class ArticleUpdateParameters
{
    public string? Title { get; set; }
    public string? Origin { get; set; }
    public string? Description { get; set; }
    public string? Thumbnail { get; set; }
    public DateTime? PublishedAt { get; set; }
    public int? Like { get; set; }
}
