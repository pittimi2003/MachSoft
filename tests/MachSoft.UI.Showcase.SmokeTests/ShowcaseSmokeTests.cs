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
            new RouteCheck("showcase", "/", "MachSoft UI Platform", "showcase-home.png"),
            new RouteCheck("showcase", "/foundations/colors", "Brand y Semánticos", "showcase-foundations-colors.png"),
            new RouteCheck("showcase", "/components/buttons", "Components / Buttons", "showcase-components-buttons.png"),
            new RouteCheck("showcase", "/components/forms", "Nombre", "showcase-components-forms.png"),
            new RouteCheck("showcase", "/patterns/crud", "Clientes", "showcase-patterns-crud.png")
        };

        var templateChecks = new[]
        {
            new RouteCheck("template-server", "/", "Proyecto base con consumo de componentes Mx*", "template-server-home.png"),
            new RouteCheck("template-wasm", "/", "Plantilla WebAssembly base.", "template-wasm-home.png")
        };

        foreach (var check in showcaseChecks.Concat(templateChecks))
        {
            var baseUrl = _hosts.BaseUrls[check.HostKey];
            var targetUrl = $"{baseUrl}{check.Route}";

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

            await page.ScreenshotAsync(new PageScreenshotOptions
            {
                Path = Path.Combine(_hosts.ScreenshotsDirectory, check.ScreenshotFileName),
                FullPage = true
            });
        }
    }

    private sealed record RouteCheck(string HostKey, string Route, string ExpectedSignal, string ScreenshotFileName);
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

            await WaitUntilReadyAsync();
        }

        public async ValueTask DisposeAsync()
        {
            if (_process.HasExited)
            {
                _process.Dispose();
                return;
            }

            try
            {
                _process.Kill(entireProcessTree: true);
            }
            catch
            {
                // Ignorar: proceso ya finalizó en paralelo.
            }

            await _process.WaitForExitAsync();
            _process.Dispose();
        }

        private static async Task<bool> IsHealthyAsync(string url)
        {
            try
            {
                using var response = await HttpClient.GetAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        private async Task WaitUntilReadyAsync()
        {
            var deadline = DateTime.UtcNow.AddMinutes(3);

            while (DateTime.UtcNow < deadline)
            {
                if (_process.HasExited)
                {
                    throw new InvalidOperationException($"El host {_baseUrl} terminó antes de iniciar. Salida:\n{_outputBuffer}");
                }

                if (await IsHealthyAsync(_baseUrl))
                {
                    return;
                }

                await Task.Delay(1000);
            }

            throw new TimeoutException($"Timeout esperando host {_baseUrl}. Salida parcial:\n{_outputBuffer}");
        }

        private void AppendLine(string? line)
        {
            if (!string.IsNullOrWhiteSpace(line))
            {
                _outputBuffer.AppendLine(line);
            }
        }
    }
}
