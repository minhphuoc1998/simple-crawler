using AngleSharp;
using Quartz;
using WebApi.Jobs.Config;
using WebApi.Jobs.Services;
using WebApi.Models;
using WebApi.Services;
using static WebApi.Jobs.Services.WebLoader;
using TaskStatus = WebApi.Models.TaskStatus;

namespace WebApi.Jobs;

public class TaskHandler: IJob
{
    private readonly LoaderTaskService _loaderTaskService;
    private readonly int _batchSize = 5;
    private readonly WebParser _webParser;
    public TaskHandler(LoaderTaskService loaderTaskService, ArticleService articleService)
    {
        _loaderTaskService = loaderTaskService;
        _webParser = new WebParser(loaderTaskService, articleService);
    }
    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine("Task handler is executing");
        var tasks = await _loaderTaskService.GetTasks(new TaskQueryParameters{
            Status = TaskStatus.CREATED,
            SortBy = "CreatedAt",
            Ascending = true,
            PageSize = _batchSize
        });
        
        var taskExecutions = tasks.Select(ExecuteSingleTask).ToArray();
        await Task.WhenAll(taskExecutions);
        // Console.WriteLine($"TASK_HANDLER: Done {tasks.Count}");
    }
    public async Task ExecuteSingleTask(LoaderTask task) {
        await _loaderTaskService.UpdateTask(task.Id!, new TaskUpdateParameters {
            Status = TaskStatus.RUNNING,
            NTry = task.NTry + 1
        });
        // Console.WriteLine($"Callback type: {task.CallbackType}");
        var config = ParseConfig.GetConfig(task.CallbackType);
        if (config == null || task.Url == null) {
            // No call back
            return;
        }
        LoaderResponse? response;
        try {
            response = await Load(task.Url, config.LoadConfig);
        } catch (Exception ex) {
            await _loaderTaskService.UpdateTask(task.Id!, new TaskUpdateParameters {
                Status = TaskStatus.ERROR,
                ErrorMessages = [ex.Message]
            });
            if (task.ParentTaskId != null) {
                await _loaderTaskService.UpdateTaskStatus(task.ParentTaskId);
            }
            return;
        }
        if (response == null) {
            return;
        }
        var status = TaskStatus.SUCCESS;
        string[]? childrenTaskIds = null;
        if (config.Children != null) {
            var contextConfig = Configuration.Default;
            var browsingContext = BrowsingContext.New(contextConfig);
            var document = await browsingContext.OpenAsync(req => req.Content(response.Html).Address(response.RedirectUrl));

            // create children tasks
            var parseTasks = config.Children.Select(child => _webParser.ParseWithDom(document, response.Url, child, task.Id!, task.PathId));
            var results = await Task.WhenAll(parseTasks);
            childrenTaskIds = results
                .Where(result => result != null)
                .SelectMany(result => result!)
                .Where(str => str != null)
                .ToArray();
            status = childrenTaskIds.Length > 0 ? TaskStatus.PENDING : TaskStatus.SUCCESS;
        }
        await _loaderTaskService.UpdateTask(task.Id!, new TaskUpdateParameters {
            Status = status,
            // ChildrenTaskIds = childrenTaskIds
        });
        if (status == TaskStatus.SUCCESS && task.ParentTaskId != null) {
            await _loaderTaskService.UpdateTaskStatus(task.ParentTaskId);
        }
    }
}
