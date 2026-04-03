using System;
using System.IO;
using FluentAssertions;
using Xunit;

namespace MachSoft.UI.Tests;

public class WorkspaceHostAdoptionTests
{
    [Theory]
    [InlineData("src/MachSoft.UI.Showcase/Layout/MainLayout.razor")]
    [InlineData("src/MachSoft.Template.Wasm/Layout/MainLayout.razor")]
    [InlineData("src/MachSoft.Template.Server/Components/Layout/MainLayout.razor")]
    public void MainLayout_ShouldUseMxWorkspaceLayoutAsRootShell(string mainLayoutPath)
    {
        var repositoryRoot = ResolveRepositoryRoot();
        var fullPath = Path.Combine(repositoryRoot.FullName, mainLayoutPath);

        File.Exists(fullPath).Should().BeTrue($"debe existir el layout del host: {mainLayoutPath}");

        var markup = File.ReadAllText(fullPath);

        markup.Should().Contain("<MxWorkspaceLayout");
        markup.Should().NotContain("<MudLayout", "el host no debe resolver estructura del shell con MudLayout");
        markup.Should().NotContain("<MudDrawer", "el host no debe resolver sidebars con MudDrawer");
        markup.Should().NotContain("<MudMainContent", "el host no debe resolver distribución principal con MudMainContent");
        markup.Should().NotContain("<MudAppBar", "el host no debe resolver header persistente con MudAppBar");
    }

    private static DirectoryInfo ResolveRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);

        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "MachSoft.UiPlatform.sln")))
            {
                return current;
            }

            current = current.Parent;
        }

        throw new InvalidOperationException("No se pudo resolver la raíz del repositorio desde AppContext.BaseDirectory.");
    }
}
