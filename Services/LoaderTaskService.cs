using Microsoft.Extensions.Options;
using MongoDB.Driver;
using WebApi.Models;
using TaskStatus = WebApi.Models.TaskStatus;

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
    public async Task<int> GetTaskCount(TaskQueryParameters queryParameters)
    {
        var filters = new List<FilterDefinition<LoaderTask>>();
        if (queryParameters.Status.HasValue)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.Status, queryParameters.Status));
        }
        if (queryParameters.ParentTaskId != null)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.ParentTaskId, queryParameters.ParentTaskId));
        } else if (queryParameters.Root)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.ParentTaskId, null));
        }
        var combinedFilter = filters.Count > 0
            ? Builders<LoaderTask>.Filter.And(filters)
            : Builders<LoaderTask>.Filter.Empty;
        return (int)await _loaderTaskCollection.CountDocumentsAsync(combinedFilter);
    }
    public async Task<List<LoaderTaskDetail>> GetTaskDetails(TaskQueryParameters queryParameters) {
        var tasks = await GetTasks(queryParameters);
        var taskDetailPromises = tasks.Select(async (task) => {
            var taskCounts = await Task.WhenAll(
                GetDescendantCount(task, new TaskQueryParameters {}),
                GetDescendantCount(task, new TaskQueryParameters {
                    Status = TaskStatus.SUCCESS
                }),
                GetDescendantCount(task, new TaskQueryParameters {
                    Status = TaskStatus.ERROR
                }),
                GetDescendantCount(task, new TaskQueryParameters {
                    Status = TaskStatus.PENDING
                }),
                GetDescendantCount(task, new TaskQueryParameters {
                    Status = TaskStatus.RUNNING
                })
            );
            return new LoaderTaskDetail {
                Id = task.Id,
                Url = task.Url,
                Status = task.Status,
                CallbackType = task.CallbackType,
                CreatedAt = task.CreatedAt,
                ErrorMessages = task.ErrorMessages,
                ParentTaskId = task.ParentTaskId,
                PathId = task.PathId,
                TotalDescendant = taskCounts[0],
                TotalDescendantSuccess = taskCounts[1],
                TotalDescendantError = taskCounts[2],
                TotalDescendantPending = taskCounts[3],
                TotalDescendantRunning = taskCounts[4]
            };
        });
        return [.. (await Task.WhenAll(taskDetailPromises))];
    }
    public async Task<List<LoaderTask>> GetTasks(TaskQueryParameters queryParameters)
    {
        var filters = new List<FilterDefinition<LoaderTask>>();
        if (queryParameters.Status.HasValue)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.Status, queryParameters.Status));
        }
        if (queryParameters.ParentTaskId != null)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.ParentTaskId, queryParameters.ParentTaskId));
        } else if (queryParameters.Root)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.ParentTaskId, null));
        }
        var combinedFilter = filters.Count > 0
            ? Builders<LoaderTask>.Filter.And(filters)
            : Builders<LoaderTask>.Filter.Empty;
        
        var sort = queryParameters.Ascending
            ? Builders<LoaderTask>.Sort.Ascending(queryParameters.SortBy)
            : Builders<LoaderTask>.Sort.Descending(queryParameters.SortBy);
        return await _loaderTaskCollection
            .Find(combinedFilter)
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
    public async Task<int> GetDescendantCount(LoaderTask task, TaskQueryParameters queryParameters)
    {
        if (task == null)
        {
            return 0;
        }
        var filters = new List<FilterDefinition<LoaderTask>>();
        if (queryParameters.Status.HasValue)
        {
            filters.Add(Builders<LoaderTask>.Filter.Eq(task => task.Status, queryParameters.Status));
        }
        if (queryParameters.Statuses != null)
        {
            filters.Add(Builders<LoaderTask>.Filter.In(task => task.Status, queryParameters.Statuses));
        }
        filters.Add(Builders<LoaderTask>.Filter.Regex(task => task.PathId, new System.Text.RegularExpressions.Regex($"^{task.PathId + task.Id}")));
        var combinedFilter = Builders<LoaderTask>.Filter.And(filters);
        return (int)await _loaderTaskCollection.CountDocumentsAsync(combinedFilter);
    }
    public async Task UpdateTaskStatus(string id) {
        var task = await _loaderTaskCollection.Find(task => task.Id == id).FirstOrDefaultAsync();
        if (task == null) {
            return;
        }
        var count = await GetDescendantCount(task, new TaskQueryParameters {
            Statuses = [TaskStatus.RUNNING, TaskStatus.PENDING]
        });
        if (count == 0) {
            await UpdateTask(id, new TaskUpdateParameters {
                Status = TaskStatus.SUCCESS
            });
        }
        if (task.ParentTaskId != null) {
            await UpdateTaskStatus(task.ParentTaskId);
        }
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
        if (parameters.ErrorMessages != null)
        {
            updateDefinitions.Add(Builders<LoaderTask>.Update.PushEach(task => task.ErrorMessages, parameters.ErrorMessages));
        }
        if (parameters.NTry > 0)
        {
            updateDefinitions.Add(Builders<LoaderTask>.Update.Set(task => task.NTry, parameters.NTry));
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