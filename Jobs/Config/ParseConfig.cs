using WebApi.Models;

namespace WebApi.Jobs.Config;

public static class ParseConfig
{
    public static SelectorNodeConfig? GetConfig(string configName) {
        var articleTitleSelector = new SelectorNodeConfig{
            Type = ParseType.FIELD,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.ATTRIBUTE,
                    Path = [
                        "meta[property=\"og:title\"][content]",
                        "meta[name=\"twitter:title\"][content]",
                    ],
                    AttributeName = "content"
                },
                new SelectorRule{
                    Type = SelectorType.TEXT,
                    Path = [
                        "head title"
                    ]
                }
            ],
            FieldName = "Title"
        };
        var articleDescriptionSelector = new SelectorNodeConfig{
            Type = ParseType.FIELD,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.ATTRIBUTE,
                    Path = [
                        "meta[name=\"twitter:description\"][content]",
                        "meta[property=\"og:description\"][content]"
                    ],
                    AttributeName = "content"
                }
            ],
            FieldName = "Description"
        };
        var articleThumbnailSelector = new SelectorNodeConfig{
            Type = ParseType.FIELD,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.ATTRIBUTE,
                    Path = [
                        "meta[property=\"og:image\"][content]",
                    ],
                    AttributeName = "content"
                }
            ],
            FieldName = "Thumbnail"
        };
        var articleDatetimeSelector = new SelectorNodeConfig{
            Type = ParseType.FIELD,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.ATTRIBUTE,
                    Path = [
                        "meta[name=\"pubdate\"][content]",
                    ],
                    AttributeName = "content"
                }
            ],
            FieldName = "Datetime"
        };

        var articleCustomSelector = new SelectorNodeConfig{
            Type = ParseType.CUSTOM,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.CUSTOM,
                    Path = [
                        "div.reactions-total > a.number",
                        "div.commentpopupwrap div.totalreact span.total"
                    ],
                    AttributeName = "content"
                }
            ],
            FieldName = "Like"
        };

        var articleGroupSelector = new SelectorNodeConfig{
            Type = ParseType.GROUP,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.ELEMENT,
                    Path = ["html"]
                }
            ],
            Children = [
                articleTitleSelector,
                articleDescriptionSelector,
                articleThumbnailSelector,
                articleDatetimeSelector,
                articleCustomSelector
            ]
        };

        var articleLinkSelector = new SelectorNodeConfig{
            Type = ParseType.LINK,
            LoadConfig = new LoaderConfig{
                Type = LoadType.BROWSER,
                Actions = [
                    new ScrollAction{
                        ScrollTimes = 5,
                        ScrollDistance = 1000,
                        WaitTime = 500
                    },
                    new ScrollAction{
                        ScrollTimes = 5,
                        ScrollDistance = -1000,
                        WaitTime = 500
                    },
                    new ClickAction{
                        Selector = "#show_more_coment",
                        ClickCount = 10,
                        Timeout = 5000
                    }
                ]
            },
            Rules = [
                new SelectorRule{
                    Type = SelectorType.HREF,
                    Path = ["article h3.title-news a[href][title], article h1.title-news a[href][title]"]
                },
            ],
            Multiple = true,
            CallbackType = "VNEXPRESS_ARTICLE_LINK",
            Children = [
                articleGroupSelector
            ]
        };

        var categoryPaginationSelector = new SelectorNodeConfig{
            Type = ParseType.PAGINATION,
            Rules = [],
            PaginationConfig = new PaginationConfig{
                Begin = 1,
                End = 20,
                Type = "fixed-url-with-page",
                PostFix = "-p{_0x0987654321}"
            },
            CallbackType = "VNEXPRESS_CATEGORY_PAGINATION",
            Children = [
                articleLinkSelector
            ]
        };
        var categoryLinkSelector = new SelectorNodeConfig{
            Type = ParseType.LINK,
            Rules = [
                new SelectorRule{
                    Type = SelectorType.HREF,
                    Path = ["nav.main-nav ul.parent li[data-id] a[href]"]
                },
            ],
            Ignore = [
                // "https://vnexpress.net/goc-nhin",
                // "https://vnexpress.net/the-gioi",
                // "https://vnexpress.net/kinh-doanh",
                // "https://vnexpress.net/bat-dong-san",
                // "https://vnexpress.net/khoa-hoc",
                // "https://vnexpress.net/giai-tri",
                // "https://vnexpress.net/the-thao",
                // "https://vnexpress.net/phap-luat",
                // "https://vnexpress.net/giao-duc",
                // "https://vnexpress.net/suc-khoe",
                // "https://vnexpress.net/doi-song",
                // "https://vnexpress.net/du-lich",
                // "https://vnexpress.net/so-hoa",
                // "https://vnexpress.net/oto-xe-may",
                // "https://vnexpress.net/y-kien",
                "https://video.vnexpress.net/",
                "https://vnexpress.net/podcast",
                "https://vnexpress.net/tam-su",
                "https://vnexpress.net/"
            ],
            Multiple = true,
            Children = [
                categoryPaginationSelector
            ],
            CallbackType = "VNEXPRESS_CATEGORY"
        };

        var rootSelector = new SelectorNodeConfig{
            Type = ParseType.ROOT,
            RootUrl = "https://vnexpress.net/",
            Children = [
                categoryLinkSelector
            ],
            CallbackType = "VNEXPRESS_ROOT"
        };

        var tuoitreArticleLinkSelector = new SelectorNodeConfig{
            Type = ParseType.LINK,
            LoadConfig = new LoaderConfig{
                Type = LoadType.BROWSER,
                Actions = [
                    new ScrollAction{
                        ScrollTimes = 10,
                        ScrollDistance = 1000,
                        WaitTime = 500
                    },
                    new ClickAction{
                        Selector = "div.cmtpopupboot button.commentpopupall",
                        ClickCount = 1,
                        Timeout = 2000
                    },
                    new WaitAction{
                        WaitForSelectors = ["div.commentpopupwrap"],
                        Timeout = 2000
                    },
                    new MouseMoveAction{
                        
                        Timeout = 500
                    },
                    new ScrollAction{
                        ScrollTimes = 10,
                        ScrollDistance = 1000,
                        WaitTime = 500
                    }
                ]
            },
            Rules = [
                new SelectorRule{
                    Type = SelectorType.HREF,
                    Path = ["div.box-content-title a[title][href]"]
                }
            ],
            Multiple = true,
            Children = [
                articleGroupSelector
            ],
            CallbackType = "TUOITREVN_ARTICLE_LINK"
        };

        var tuoitrePaginationSelector = new SelectorNodeConfig{
            Type = ParseType.PAGINATION,
            Rules = [],
            PaginationConfig = new PaginationConfig{
                Begin = 1,
                End = 20,
                Type = "fixed-url-with-page",
                PostFix = "timeline/0/trang-{_0x0987654321}.htm"
            },
            Children = [
                tuoitreArticleLinkSelector
            ],
            CallbackType = "TUOITREVN_PAGINATION"
        };

        var tuoitreRootSelector = new SelectorNodeConfig{
            Type = ParseType.ROOT,
            RootUrl = "https://tuoitre.vn/",
            Children = [
                tuoitrePaginationSelector
            ],
            CallbackType = "TUOITREVN_ROOT"
        };

        return configName switch
        {
            "VNEXPRESS_ROOT" => rootSelector,
            "VNEXPRESS_CATEGORY" => categoryLinkSelector,
            "VNEXPRESS_CATEGORY_PAGINATION" => categoryPaginationSelector,
            "VNEXPRESS_ARTICLE_LINK" => articleLinkSelector,
            "TUOITREVN_ROOT" => tuoitreRootSelector,
            "TUOITREVN_PAGINATION" => tuoitrePaginationSelector,
            "TUOITREVN_ARTICLE_LINK" => tuoitreArticleLinkSelector,
            _ => null,
        };
    }
}

public enum CrawlSite {
    VNEXPRESS,
    TUOITRE
}

public class ParserConfig {

}

public enum ParseType
{
    ROOT,
    LINK,
    PAGINATION,
    GROUP,
    FIELD,
    CUSTOM

}

public class SelectorNodeConfigConfig {
    public ParseType Type { get; set; }
}

public enum SelectorType
{
    ATTRIBUTE,
    HREF,
    TEXT,
    ELEMENT,
    CUSTOM
}

public class PaginationConfig {
    public int Begin { get; set; } = 1;
    public int End { get; set; } = 10;
    public string Type { get; set; } = "fixed-url-with-page";
    public string? PostFix { get; set; }
}

public class SelectorRule {
    public SelectorType Type { get; set; }
    public string? AttributeName { get; set; }
    public string[]? Path { get; set; } = [];
}

public class SelectorNodeConfig {
    public required ParseType Type { get; set; }
    public string? RootUrl { get; set; }
    public SelectorRule[] Rules { get; set; }  = [];
    public string[] Ignore = [];
    public bool Multiple { get; set; } = false;
    public int? Limit { get; set; }
    public SelectorNodeConfig[]? Children { get; set; }
    public LoaderConfig? LoadConfig { get; set; }
    public string CallbackType { get; set; } = "default";
    public PaginationConfig? PaginationConfig { get; set; }
    public string? FieldName { get; set; }
}
