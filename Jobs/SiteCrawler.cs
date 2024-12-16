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
        var existingTuoitreTask = await _loaderTaskService.GetOne(new TaskQueryParameters {
            Statuses = [TaskStatus.RUNNING, TaskStatus.PENDING],
            Url = "https://tuoitre.vn/"
        });
        if (existingTuoitreTask == null) {
            await _loaderTaskService.CreateTask(new LoaderTask {
                Url = "https://tuoitre.vn/",
                CallbackType = "TUOITREVN_ROOT"
            });
        }
        
        var existingVnexpressTask = await _loaderTaskService.GetOne(new TaskQueryParameters {
            Statuses = [TaskStatus.RUNNING, TaskStatus.PENDING],
            Url = "https://vnexpress.net/"
        });
        if (existingVnexpressTask == null) {
            await _loaderTaskService.CreateTask(new LoaderTask {
                Url = "https://vnexpress.net/",
                CallbackType = "VNEXPRESS_ROOT"
            });
        }
    }
}