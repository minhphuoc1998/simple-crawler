using Microsoft.Extensions.Options;
using Microsoft.VisualBasic;
using MongoDB.Driver;
using WebApi.Models;

namespace WebApi.Services;

public class ArticleService
{
    private readonly IMongoCollection<Article> _articleCollection;
    public ArticleService(
        IOptions<ArticleDatabaseSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _articleCollection = mongoDatabase.GetCollection<Article>(settings.Value.CollectionName);
    }
    public async Task<List<Article>> GetArticles(ArticleQueryParameters queryParameters)
    {
        var filters = new List<FilterDefinition<Article>>();
        if (queryParameters.Url != null)
        {
            filters.Add(Builders<Article>.Filter.Eq(article => article.Url, queryParameters.Url));
        }
        if (queryParameters.Origin != null)
        {
            filters.Add(Builders<Article>.Filter.Eq(article => article.Origin, queryParameters.Origin));
        }
        var sort = queryParameters.Ascending
            ? Builders<Article>.Sort.Ascending(queryParameters.SortBy)
            : Builders<Article>.Sort.Descending(queryParameters.SortBy);
        var combinedFilter = filters.Count > 0
            ? Builders<Article>.Filter.And(filters)
            : Builders<Article>.Filter.Empty;
        return await _articleCollection
            .Find(combinedFilter)
            .Sort(sort)
            .Skip(((queryParameters.Page > 0 ? queryParameters.Page : 1) - 1) * queryParameters.PageSize)
            .Limit(queryParameters.PageSize)
            .ToListAsync();
    }

    public async Task<Article?> GetOne(ArticleQueryParameters queryParameters)
    {
        var filters = new List<FilterDefinition<Article>>();
        if (queryParameters.Url != null)
        {
            filters.Add(Builders<Article>.Filter.Eq(article => article.Url, queryParameters.Url));
        }
        if (queryParameters.Id != null)
        {
            filters.Add(Builders<Article>.Filter.Eq(article => article.Id, queryParameters.Id));
        }
        var sort = queryParameters.Ascending
            ? Builders<Article>.Sort.Ascending(queryParameters.SortBy)
            : Builders<Article>.Sort.Descending(queryParameters.SortBy);
        
        var combinedFilter = filters.Count > 0
            ? Builders<Article>.Filter.And(filters)
            : Builders<Article>.Filter.Empty;

        return await _articleCollection
            .Find(combinedFilter)
            .Sort(sort)
            .FirstOrDefaultAsync();
    }

    public async Task UpdateArticle(string id, ArticleUpdateParameters parameters)
    {
        var updateDefinitions =  new List<UpdateDefinition<Article>>();
        if (parameters.Title != null)
        {
            updateDefinitions.Add(Builders<Article>.Update.Set(article => article.Title, parameters.Title));
        }
        if (parameters.Description != null)
        {
            updateDefinitions.Add(Builders<Article>.Update.Set(article => article.Description, parameters.Description));
        }
        if (parameters.Thumbnail != null)
        {
            updateDefinitions.Add(Builders<Article>.Update.Set(article => article.Thumbnail, parameters.Thumbnail));
        }
        if (parameters.Like.HasValue)
        {
            updateDefinitions.Add(Builders<Article>.Update.Set(article => article.Like, parameters.Like));
        }
        if (parameters.Origin != null)
        {
            updateDefinitions.Add(Builders<Article>.Update.Set(article => article.Origin, parameters.Origin));
        }
        if (updateDefinitions.Count == 0) return;
        var update = Builders<Article>.Update.Combine(updateDefinitions);
        await _articleCollection.UpdateOneAsync(article => article.Id == id, update);
    }

    public async Task<Article> CreateArticle(Article article)
    {
        await _articleCollection.InsertOneAsync(article);
        return article;
    }
}
