using System.Security.Cryptography.X509Certificates;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Quartz;
using WebApi.Models;
using WebApi.Services;
using TaskStatus = WebApi.Models.TaskStatus;

namespace WebApi.Jobs;

public class SiteCrawler : IJob
{
    private readonly LoaderTaskService _loaderTaskService;
    public SiteCrawler(LoaderTaskService loaderTaskService)
    {
        _loaderTaskService = loaderTaskService;
    }
    public async Task Execute(IJobExecutionContext context)
    {
        // Console.WriteLine("SiteCrawler is executing");
        var existingTuoitreTask = await _loaderTaskService.GetOne(new TaskQueryParameters {
            Status = TaskStatus.RUNNING,
            Url = "https://tuoitre.vn/"
        });
        if (existingTuoitreTask != null) {
            // Console.WriteLine("Task already exists");
            return;
        }
        await _loaderTaskService.CreateTask(new LoaderTask {
            Url = "https://tuoitre.vn/",
            CallbackType = "TUOITREVN_ROOT"
        });
        
        var existingVnexpressTask = await _loaderTaskService.GetOne(new TaskQueryParameters {
            Status = TaskStatus.RUNNING,
            Url = "https://vnexpress.net/"
        });
        if (existingVnexpressTask != null) {
            // Console.WriteLine("Task already exists");
            return;
        }
        await _loaderTaskService.CreateTask(new LoaderTask {
            Url = "https://vnexpress.net/",
            CallbackType = "VNEXPRESS_ROOT"
        });
        // Console.WriteLine($"Task created {task.Id}");
    }
}