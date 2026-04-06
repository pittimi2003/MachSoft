using System.Diagnostics;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.Playwright;
using Xunit;

namespace MachSoft.UI.Showcase.SmokeTests;

public sealed class ShowcaseSmokeTests : IClassFixture<SmokeTestHostsFixture>
{
    private readonly SmokeTestHostsFixture _hosts;

    public ShowcaseSmokeTests(SmokeTestHostsFixture hosts)
    {
        _hosts = hosts;
    }

    [Fact]
    public async Task VisualSmoke_ShouldCover_ShowcaseAndTemplates()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        var showcaseChecks = new[]
        {
            new RouteCheck("showcase", "/", "MachSoft UI Platform", "showcase-home-light.png", false),
            new RouteCheck("showcase", "/", "MachSoft UI Platform", "showcase-home-dark.png", true),
            new RouteCheck("showcase", "/foundations/colors", "Color system de MachSoft", "showcase-foundations-colors-light.png", false),
            new RouteCheck("showcase", "/foundations/colors", "Color system de MachSoft", "showcase-foundations-colors-dark.png", true),
            new RouteCheck("showcase", "/components/buttons", "Buttons auditables", "showcase-components-buttons-light.png", false),
            new RouteCheck("showcase", "/components/buttons", "Buttons auditables", "showcase-components-buttons-dark.png", true)
        };

        foreach (var check in showcaseChecks)
        {
            var baseUrl = _hosts.BaseUrls[check.HostKey];
            var targetUrl = $"{baseUrl}{check.Route}";
            var theme = check.DarkMode ? "dark" : "light";

            if (check.HostKey == "showcase")
            {
                await page.GotoAsync(baseUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.NetworkIdle,
                    Timeout = 60_000
                });

                await page.EvaluateAsync(
                    "([key, value]) => localStorage.setItem(key, value)",
                    new object[] { "mx-showcase-theme", theme });
            }

            var response = await page.GotoAsync(targetUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.NetworkIdle,
                Timeout = 60_000
            });

            response.Should().NotBeNull($"{targetUrl} debe responder para smoke test");
            response!.Ok.Should().BeTrue($"{targetUrl} debe devolver HTTP 2xx/3xx");

            var locator = page.GetByText(check.ExpectedSignal, new PageGetByTextOptions { Exact = false });
            await locator.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 20_000
            });

            if (check.HostKey == "showcase")
            {
                await page.WaitForFunctionAsync(
                    "theme => (localStorage.getItem('mx-showcase-theme') || 'light') === theme",
                    theme);
            }

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(_hosts.ScreenshotsDirectory, check.ScreenshotFileName),
                FullPage = true
            });
        }
    }


    [Fact]
    public async Task ThemeToggle_ShouldSyncThemeIconAndPersistence()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        var page = await browser.NewPageAsync();

        var showcaseUrl = _hosts.BaseUrls["showcase"];
        await page.GotoAsync(showcaseUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 60_000
        });

        var toggle = page.Locator("button.mx-theme-toggle");
        await toggle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20_000 });

        static string NextTheme(string theme) => theme == "dark" ? "light" : "dark";

        var initialTheme = await page.EvaluateAsync<string>("() => localStorage.getItem('mx-showcase-theme') || 'light'");
        var expectedAfterFirstClick = NextTheme(initialTheme);

        await toggle.ClickAsync();
        await page.WaitForFunctionAsync("theme => localStorage.getItem('mx-showcase-theme') === theme", expectedAfterFirstClick);

        var themeAfterClick = await page.EvaluateAsync<string>("() => localStorage.getItem('mx-showcase-theme') || 'light'");
        themeAfterClick.Should().Be(expectedAfterFirstClick);

        var expectedTitleAfterClick = themeAfterClick == "dark" ? "Cambiar a tema claro" : "Cambiar a tema oscuro";
        (await toggle.GetAttributeAsync("title")).Should().Be(expectedTitleAfterClick);

        await page.ReloadAsync(new PageReloadOptions { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60_000 });
        await toggle.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 20_000 });

        var themeAfterReload = await page.EvaluateAsync<string>("() => localStorage.getItem('mx-showcase-theme') || 'light'");
        themeAfterReload.Should().Be(themeAfterClick);

        var expectedTitleAfterReload = themeAfterReload == "dark" ? "Cambiar a tema claro" : "Cambiar a tema oscuro";
        (await toggle.GetAttributeAsync("title")).Should().Be(expectedTitleAfterReload);
    }

    private sealed record RouteCheck(string HostKey, string Route, string ExpectedSignal, string ScreenshotFileName, bool DarkMode = false);
}

public sealed class SmokeTestHostsFixture : IAsyncLifetime
{
    private readonly List<HostProcess> _hostProcesses = new();

    public IReadOnlyDictionary<string, string> BaseUrls { get; private set; } = new Dictionary<string, string>();

    public string ScreenshotsDirectory { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        var repositoryRoot = ResolveRepositoryRoot();
        ScreenshotsDirectory = Path.Combine(repositoryRoot, "artifacts", "screenshots");
        Directory.CreateDirectory(ScreenshotsDirectory);

        var hosts = new[]
        {
            new HostDefinition("showcase", "src/MachSoft.UI.Showcase/MachSoft.UI.Showcase.csproj", "http://127.0.0.1:5123"),
            new HostDefinition("template-server", "src/MachSoft.Template.Server/MachSoft.Template.Server.csproj", "http://127.0.0.1:5124"),
            new HostDefinition("template-wasm", "src/MachSoft.Template.Wasm/MachSoft.Template.Wasm.csproj", "http://127.0.0.1:5125")
        };

        foreach (var host in hosts)
        {
            var process = new HostProcess(repositoryRoot, host.ProjectPath, host.BaseUrl);
            await process.StartAsync();
            _hostProcesses.Add(process);
        }

        BaseUrls = hosts.ToDictionary(x => x.Key, x => x.BaseUrl);
    }

    public async Task DisposeAsync()
    {
        foreach (var process in _hostProcesses)
        {
            await process.DisposeAsync();
        }
    }

    private static string ResolveRepositoryRoot()
    {
        var currentDirectory = AppContext.BaseDirectory;
        var directoryInfo = new DirectoryInfo(currentDirectory);

        while (directoryInfo is not null)
        {
            if (File.Exists(Path.Combine(directoryInfo.FullName, "MachSoft.UiPlatform.sln")))
            {
                return directoryInfo.FullName;
            }

            directoryInfo = directoryInfo.Parent;
        }

        throw new DirectoryNotFoundException("No se pudo resolver la raíz del repositorio para smoke tests.");
    }

    private sealed record HostDefinition(string Key, string ProjectPath, string BaseUrl);

    private sealed class HostProcess : IAsyncDisposable
    {
        private static readonly HttpClient HttpClient = new();

        private readonly string _baseUrl;
        private readonly Process _process;
        private readonly StringBuilder _outputBuffer = new();

        public HostProcess(string repositoryRoot, string projectPath, string baseUrl)
        {
            _baseUrl = baseUrl;

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dotnet",
                    Arguments = $"run --project {projectPath} --urls {_baseUrl}",
                    WorkingDirectory = repositoryRoot,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.OutputDataReceived += (_, args) => AppendLine(args.Data);
            _process.ErrorDataReceived += (_, args) => AppendLine(args.Data);
        }

        public async Task StartAsync()
        {
            if (!_process.Start())
            {
                throw new InvalidOperationException($"No se pudo iniciar host {_baseUrl}.");
            }

            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(90));
            var delay = TimeSpan.FromMilliseconds(500);
            Exception? lastException = null;

            while (!cts.Token.IsCancellationRequested)
            {
                if (_process.HasExited)
                {
                    throw new InvalidOperationException(
                        $"El host {_baseUrl} finalizó prematuramente con código {_process.ExitCode}.\nSalida:\n{_outputBuffer}");
                }

                try
                {
                    using var response = await HttpClient.GetAsync(_baseUrl, cts.Token);
                    if ((int)response.StatusCode is >= 200 and < 500)
                    {
                        return;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }

                await Task.Delay(delay, cts.Token);
            }

            throw new TimeoutException(
                $"Timeout esperando disponibilidad de {_baseUrl}. Último error: {lastException?.Message}.\nSalida:\n{_outputBuffer}");
        }

        private void AppendLine(string? line)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                return;
            }

            lock (_outputBuffer)
            {
                _outputBuffer.AppendLine(line);
            }
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                if (!_process.HasExited)
                {
                    _process.Kill(entireProcessTree: true);
                }
            }
            catch
            {
                // ignore cleanup exceptions
            }

            await _process.WaitForExitAsync();
            _process.Dispose();
        }
    }
}
