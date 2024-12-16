using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("task")]
public class TaskController : ControllerBase
{
    private readonly LoaderTaskService _taskService;
    public TaskController(LoaderTaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet(Name = "GetTasks")]
    [ServiceFilter(typeof(CustomAuthorizeFilter))]
    public async Task<ActionResult<Pagination<LoaderTaskDetail>>> GetTasks([FromQuery] TaskQueryParameters queryParameters)
    {
        var tasks = await _taskService.GetTaskDetails(queryParameters);
        var count = await _taskService.GetTaskCount(queryParameters);
        var paginatedTasks = new Pagination<LoaderTaskDetail>(
            tasks,
            count,
            queryParameters.Page,
            queryParameters.PageSize
        );
        return Ok(paginatedTasks);
    }
}