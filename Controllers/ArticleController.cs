
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers;

[ApiController]
[Route("article")]
public class ArticlesController : ControllerBase
{
    private readonly ArticleService _articleService;
    public ArticlesController(ArticleService articleService)
    {
        _articleService = articleService;
    }

    [HttpGet(Name = "GetArticles")]
    public async Task<ActionResult<List<Article>>> GetArticles([FromQuery] ArticleQueryParameters queryParameters)
    {
        // Console.WriteLine($"Url: {queryParameters.Url}, Origin: {queryParameters.Origin}, Id: {queryParameters.Id}, Page: {queryParameters.Page}, PageSize: {queryParameters.PageSize}, SortBy: {queryParameters.SortBy}, Ascending: {queryParameters.Ascending}");
        var articles = await _articleService.GetArticles(queryParameters);
        return Ok(articles);
    }
}