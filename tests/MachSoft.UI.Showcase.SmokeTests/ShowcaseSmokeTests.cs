using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace MachSoft.UI.Showcase.SmokeTests;

public class ShowcaseSmokeTests
{
    [Fact(Skip = "Requiere app levantada en http://localhost:5123 y browsers de Playwright instalados")]
    public async Task Home_ShouldLoad_AndTakeScreenshot()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();
        await page.GotoAsync("http://localhost:5123");
        (await page.TitleAsync()).Should().NotBeNull();
        await page.ScreenshotAsync(new() { Path = "../../artifacts/screenshots/showcase-home.png", FullPage = true });
    }
}
