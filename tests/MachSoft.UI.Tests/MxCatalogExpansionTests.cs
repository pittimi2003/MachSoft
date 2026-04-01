using Bunit;
using Bunit.JSInterop;
using FluentAssertions;
using MachSoft.UI.Components;
using MachSoft.UI.Models;
using MudBlazor.Services;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxCatalogExpansionTests : TestContext
{
    public MxCatalogExpansionTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }

    [Fact]
    public void MxDataGrid_ShouldRenderHeaderAndRows()
    {
        var columns = new[] { new MxDataGridColumn<Row>("name", "Nombre", x => x.Name) { IsPrimary = true } };
        var rows = new[] { new Row("Acme") };

        var cut = RenderComponent<MxDataGrid<Row>>(p => p
            .Add(x => x.Columns, columns)
            .Add(x => x.Items, rows));

        cut.Markup.Should().Contain("Nombre").And.Contain("Acme");
    }

    [Fact]
    public void MxEmptyLoadingAndErrorState_ShouldRenderContent()
    {
        RenderComponent<MxEmptyState>(p => p.Add(x => x.Title, "Vacío")).Markup.Should().Contain("Vacío");
        RenderComponent<MxLoadingState>(p => p.Add(x => x.Title, "Cargando")).Markup.Should().Contain("Cargando");
        RenderComponent<MxErrorState>(p => p.Add(x => x.Title, "Error")).Markup.Should().Contain("Error");
    }

    private sealed record Row(string Name);
}
