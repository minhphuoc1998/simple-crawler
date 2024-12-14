using AngleSharp;
using Quartz;
using WebApi.Jobs.Config;
using WebApi.Jobs.Services;
using WebApi.Models;
using WebApi.Services;
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
            Status = TaskStatus.RUNNING
        });
        // Console.WriteLine($"Callback type: {task.CallbackType}");
        var config = ParseConfig.GetConfig(task.CallbackType);
        if (config == null || task.Url == null) {
            // No call back
            return;
        }
        var response = await WebLoader.Load(task.Url, config.LoadConfig);
        if (response == null) {
            return;
        }
        if (config.Children != null) {
            var contextConfig = Configuration.Default;
            var browsingContext = BrowsingContext.New(contextConfig);
            var document = await browsingContext.OpenAsync(req => req.Content(response.Html).Address(response.RedirectUrl));

            // await File.WriteAllTextAsync($"{task.CallbackType}.html", response.Html);
            var parseTasks = config.Children.Select(child => _webParser.ParseWithDom(document, response.Url, child));
            await Task.WhenAll(parseTasks);
        }
        await _loaderTaskService.UpdateTask(task.Id!, new TaskUpdateParameters {
            Status = TaskStatus.SUCCESS
        });
    }
}
