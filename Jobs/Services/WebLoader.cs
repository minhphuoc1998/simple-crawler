
using Microsoft.Playwright;
using WebApi.Jobs.Config;

namespace WebApi.Jobs.Services;

class WebLoader
{
    public class LoaderResponse {
        public required string Html { get; set; }
        public required string Url { get; set; }
        public required string RedirectUrl { get; set; }
    }

    public static async Task<LoaderResponse?> Load(string url, LoaderConfig? config) {
        string? html = null;
        string? redirectUrl = null;
        if (config == null) {
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode) {
                html = await response.Content.ReadAsStringAsync();
                redirectUrl = response.RequestMessage?.RequestUri?.ToString() ?? url;
            } else {
                Console.WriteLine($"Failed to load page: {url}");
                return null;
            }
        } else {
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
            var page = await browser.NewPageAsync();
            IResponse? response = null;
            try {
                response = await page.GotoAsync(url, new PageGotoOptions {
                    Timeout = 10000,
                    WaitUntil = WaitUntilState.NetworkIdle
                });
            } catch (Exception) {}
            if (config.Actions != null) {
                foreach (var action in config.Actions) {
                    if (action is ClickAction clickAction) {
                        Console.WriteLine("Begin Click Action");

                        if (clickAction.Selector != null) {
                            for (int i = 0; i < clickAction.ClickCount; i++) {
                                IElementHandle? element = null;
                                try {
                                    element = await page.WaitForSelectorAsync(clickAction.Selector, new PageWaitForSelectorOptions {
                                        Timeout = clickAction.Timeout
                                    });
                                } catch (Exception) {
                                    break;
                                }
                                try {
                                    if (element != null) {
                                        await element.ClickAsync();
                                        await page.WaitForTimeoutAsync(clickAction.WaitTime);
                                    }
                                } catch (Exception) {
                                    break;
                                }
                            }
                        }
                        Console.WriteLine("End Click Action");
                    }
                    if (action is WaitAction waitAction) {
                        Console.WriteLine("Begin Wait Action");
                        if (waitAction.WaitForSelectors.Length > 0) {
                            foreach (var selector in waitAction.WaitForSelectors) {
                                Console.WriteLine("Wait for selector: " + selector);
                                try {
                                    await page.WaitForSelectorAsync(selector, new PageWaitForSelectorOptions {
                                        Timeout = waitAction.Timeout
                                    });
                                } catch (Exception) {}
                                Console.WriteLine("End Wait for selector: " + selector);
                            }
                        } else if (waitAction.Time > 0) {
                            await page.WaitForTimeoutAsync(waitAction.Time);
                        }
                        Console.WriteLine("End Wait Action");
                    }
                    if (action is ScrollAction scrollAction) {
                        for (int i = 0; i < scrollAction.ScrollTimes; i++) {
                            await page.Mouse.WheelAsync(0, scrollAction.ScrollDistance);
                            await page.WaitForTimeoutAsync(scrollAction.WaitTime);
                        }
                    }
                    if (action is MouseMoveAction mouseMoveAction) {
                        Console.WriteLine("Begin Mouse Move Action");
                        if (mouseMoveAction.ElementSelector != null) {
                            var element = await page.WaitForSelectorAsync(mouseMoveAction.ElementSelector, new PageWaitForSelectorOptions {
                                Timeout = 10000
                            });
                            if (element != null) {
                                var location = await element.BoundingBoxAsync();
                                if (location != null) {
                                    await page.Mouse.MoveAsync(location.X + location.Width / 2, location.Y + location.Height / 2);
                                }
                            }
                        } else {
                            await page.Mouse.MoveAsync(mouseMoveAction.X, mouseMoveAction.Y);
                        }
                        Console.WriteLine("End Mouse Move Action");
                    }
                }
            }
            html = await page.ContentAsync();
            redirectUrl = response?.Url ?? url;
            await page.CloseAsync();
            await browser.CloseAsync();
        }
        if (html == null) {
            return null;
        }
        return new LoaderResponse {
            Html = html,
            Url = url,
            RedirectUrl = redirectUrl
        };
    }
}
