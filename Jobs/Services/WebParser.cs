using System.Text.Json;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Microsoft.Playwright;
using WebApi.Jobs.Config;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Jobs.Services;

class WebParser {
    private readonly LoaderTaskService _loaderTaskService;
    private readonly ArticleService _articleService;
    public WebParser(LoaderTaskService loaderTaskService, ArticleService articleService) {
        _loaderTaskService = loaderTaskService;
        _articleService = articleService;
    }
    
    public static Article ParseGroup(IDocument doc, string url, SelectorNodeConfig selectorNodeConfig) {
        var groupInfo = new Article{
            Url = url,
            Origin = new Uri(url).Host
        };
        Console.WriteLine(JsonSerializer.Serialize(groupInfo));
        foreach (var child in selectorNodeConfig.Children ?? []) {
            switch (child.Type) {
            case ParseType.FIELD:
                string? value = null;
                foreach (var rule in child.Rules) {
                    foreach (var path in rule.Path ?? []) {
                        var element = doc.QuerySelector(path);
                        if (element == null) continue;
                        switch (rule.Type) {
                        case SelectorType.TEXT:
                            value = element.TextContent;
                            break;
                        case SelectorType.ATTRIBUTE:
                            if (rule.AttributeName != null && element.HasAttribute(rule.AttributeName)) {
                                value = element.GetAttribute(rule.AttributeName);
                            }
                            break;
                        case SelectorType.HREF:
                            if (element is IHtmlAnchorElement anchor) {
                                value = anchor.Href;
                            }
                            break;
                        }
                    }
                }
                if (value != null) {
                    switch (child.FieldName)
                    {
                        case "Title":
                            groupInfo.Title = value;
                            break;
                        case "Datetime":
                            if (DateTime.TryParse(value, out DateTime date)) {
                                groupInfo.PublishedAt = date;
                            } else {
                                groupInfo.PublishedAt = DateTime.Now;
                            }
                            break;
                        case "Thumbnail":
                            groupInfo.Thumbnail = value;
                            break;
                        case "Description":
                            groupInfo.Description = value;
                            break;
                    }
                }
                break;
            case ParseType.CUSTOM:
                int likeCount = 0;
                foreach (var rule in child.Rules) {
                    foreach (var path in rule.Path ?? []) {
                        var elements = doc.QuerySelectorAll(path);
                        // Console.WriteLine("Found elements Custom: " + elements.Length);
                        if (elements.Length > 0) {
                            foreach (var element in elements) {
                                // Console.WriteLine("Likes: " + element.TextContent);
                                if (int.TryParse(element.TextContent.Trim(), out int like)) {
                                    likeCount += like;
                                }
                                // Console.WriteLine("Like count: " + likeCount);
                            }
                        }
                    }
                }
                groupInfo.Like = likeCount;
                break;
            }
        }
        return groupInfo;
    }

    public async Task ParseWithDom(
        IDocument document,
        string url,
        SelectorNodeConfig config
    ) {
        switch (config.Type) {
            case ParseType.LINK:
                var rules = config.Rules;
                List<string> hrefs = [];
                foreach (var rule in rules ?? []) {
                    foreach (var path in rule.Path ?? []) {
                        var elements = document.QuerySelectorAll(path);
                        // Console.WriteLine("Found elements: " + elements.Length);
                        foreach (var element in elements) {
                            string? absoluteUrl = null;
                            switch (rule.Type) {
                                case SelectorType.HREF:
                                    if (element is IHtmlAnchorElement anchor) {
                                        absoluteUrl = anchor.Href;
                                    }
                                    break;
                                case SelectorType.TEXT:
                                    absoluteUrl = element.TextContent;
                                    break;
                                case SelectorType.ATTRIBUTE:
                                    if (rule.AttributeName != null && element.HasAttribute(rule.AttributeName)) {
                                        var eurl = element.GetAttribute(rule.AttributeName);
                                        absoluteUrl = new Uri(new Uri(url), eurl!).ToString();
                                    }
                                    break;
                            }
                            if (absoluteUrl != null) {
                                if (config.Ignore?.Length > 0 && config.Ignore.Contains(absoluteUrl)) {
                                    Console.WriteLine("Ignore: " + string.Join("\n", config.Ignore));
                                    continue;
                                } else {
                                    hrefs.Add(absoluteUrl);
                                }
                            }
                        }
                        if (hrefs.Count > 0) break;
                    }
                    if (hrefs.Count > 0) break;
                }
                if (config.Limit != null) {
                    hrefs = hrefs.Take(config.Limit.Value).ToList();
                }
                // Console.WriteLine($"Found links with children {config.Children?.Length}: {string.Join("\n", hrefs)}");
                if (hrefs.Count > 0 && config.Children?.Length > 0) {
                    foreach (var href in hrefs) {
                        await _loaderTaskService.CreateTask(new LoaderTask {
                            Url = href,
                            CallbackType = config.CallbackType
                        });
                    }
                }
                break;
            case ParseType.PAGINATION:
                var paginationConfig = config.PaginationConfig!;
                if (paginationConfig.Type == "fixed-url-with-page") {
                    // Console.WriteLine($"Pagination Params: {paginationConfig.Begin} - {paginationConfig.End}");
                    for (int i = paginationConfig.Begin; i <= paginationConfig.End; i++) {
                        if (paginationConfig.PostFix != null) {
                            var pageUrl = $"{url}{paginationConfig.PostFix.Replace("{_0x0987654321}", i.ToString())}";
                            // Console.WriteLine($"Pagination URL: {pageUrl}");
                            await _loaderTaskService.CreateTask(new LoaderTask {
                                Url = pageUrl,
                                CallbackType = config.CallbackType
                            });
                        }
                    }
                } else {

                }
                break;

            case ParseType.GROUP:
                var data = ParseGroup(document, url, config);
                if (_articleService != null) {
                    var existingArticle = await _articleService.GetOne(new ArticleQueryParameters {
                        Url = url
                    });
                    if (existingArticle == null) {
                        await _articleService.CreateArticle(data);
                    } else {
                        await _articleService.UpdateArticle(existingArticle.Id!, new ArticleUpdateParameters {
                            Title = data.Title,
                            Description = data.Description,
                            Thumbnail = data.Thumbnail,
                            Like = data.Like,
                            PublishedAt = data.PublishedAt
                        });
                    }
                } else {
                    Console.WriteLine($"Group Info: {JsonSerializer.Serialize(data)}");
                }
                break;
        }
    }
}
