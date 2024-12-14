using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Models;

namespace WebApi.Services;

public class LoaderTaskService
{
    private readonly IMongoCollection<LoaderTask> _loaderTaskCollection;
    public LoaderTaskService(
        IOptions<LoaderDatabaseSettings> settings)
    {
        var mongoClient = new MongoClient(settings.Value.ConnectionString);
        var mongoDatabase = mongoClient.GetDatabase(settings.Value.DatabaseName);
        _loaderTaskCollection = mongoDatabase.GetCollection<LoaderTask>(settings.Value.CollectionName);
    }
    public async Task<List<LoaderTask>> GetTasks(TaskQueryParameters queryParameters)
    {
        var filter = queryParameters.Status.HasValue
            ? Builders<LoaderTask>.Filter.Eq(task => task.Status, queryParameters.Status)
            : Builders<LoaderTask>.Filter.Empty;
        
        var sort = queryParameters.Ascending
            ? Builders<LoaderTask>.Sort.Ascending(queryParameters.SortBy)
            : Builders<LoaderTask>.Sort.Descending(queryParameters.SortBy);
        return await _loaderTaskCollection
            .Find(filter)
            .Sort(sort)
            .Skip(((queryParameters.Page > 0 ? queryParameters.Page : 1) - 1) * queryParameters.PageSize)
            .Limit(queryParameters.PageSize)
            .ToListAsync();
    }
    public async Task<LoaderTask> GetOne(TaskQueryParameters queryParameters)
    {
        var filters = new List<FilterDefinition<LoaderTask>>();
        if (queryParameters.Status.HasValue)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.Status, queryParameters.Status));
        }
        if (queryParameters.Url != null)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.Url, queryParameters.Url));
        }
        var sort = queryParameters.Ascending
            ? Builders<LoaderTask>.Sort.Ascending(queryParameters.SortBy)
            : Builders<LoaderTask>.Sort.Descending(queryParameters.SortBy);
        
        var combinedFilter = filters.Count > 0
            ? Builders<LoaderTask>.Filter.And(filters)
            : Builders<LoaderTask>.Filter.Empty;

        return await _loaderTaskCollection
            .Find(combinedFilter)
            .Sort(sort)
            .FirstOrDefaultAsync();
    }
    public async Task<LoaderTask> GetTask(string id)
    {
        return await _loaderTaskCollection.Find(task => task.Id == id).FirstOrDefaultAsync();
    }
    public async Task<LoaderTask> CreateTask(LoaderTask task)
    {
        await _loaderTaskCollection.InsertOneAsync(task);
        return task;
    }
    public async Task UpdateTask(string id, TaskUpdateParameters parameters)
    {
        var updateDefinitions = new List<UpdateDefinition<LoaderTask>>();
        if (parameters.Status.HasValue)
        {
            updateDefinitions.Add(Builders<LoaderTask>.Update.Set(task => task.Status, parameters.Status));
        }
        if (updateDefinitions.Count > 0) {
            var update = Builders<LoaderTask>.Update.Combine(updateDefinitions);
            await _loaderTaskCollection.UpdateOneAsync(task => task.Id == id, update);
        }
    }
    public async Task RemoveTask(string id)
    {
        await _loaderTaskCollection.DeleteOneAsync(task => task.Id == id);
    }
}