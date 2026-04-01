using Bunit;
using Bunit.JSInterop;
using FluentAssertions;
using MachSoft.UI.Components;
using MachSoft.UI.Models;
using MudBlazor.Services;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxBlock2ControlsTests : TestContext
{
    public MxBlock2ControlsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }

    [Fact]
    public void MxConfirmDialog_ShouldRenderTitleAndMessage()
    {
        var cut = RenderComponent<MxConfirmDialog>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Title, "Confirmar")
            .Add(x => x.Message, "Mensaje"));

        cut.Markup.Should().Contain("Confirmar").And.Contain("Mensaje");
    }

    [Fact]
    public void MxFileUpload_ShouldRenderLabelAndPlaceholder()
    {
        var cut = RenderComponent<MxFileUpload>(p => p
            .Add(x => x.Label, "Archivo")
            .Add(x => x.Placeholder, "Selecciona archivo"));

        cut.Markup.Should().Contain("Archivo").And.Contain("Selecciona archivo");
    }


    [Fact]
    public void MxFileUpload_ShouldRenderUploadingProgress()
    {
        var cut = RenderComponent<MxFileUpload>(p => p
            .Add(x => x.Label, "Archivo")
            .Add(x => x.Uploading, true)
            .Add(x => x.UploadProgress, 60));

        cut.Markup.Should().Contain("Procesando 60%");
    }

    [Fact]
    public void MxTable_ShouldRenderEmptyText()
    {
        var columns = new[] { new MxTableColumn<Row>("name", "Nombre", x => x.Name, true) };

        var cut = RenderComponent<MxTable<Row>>(p => p
            .Add(x => x.Columns, columns)
            .Add(x => x.Items, Array.Empty<Row>())
            .Add(x => x.EmptyText, "Sin registros"));

        cut.Markup.Should().Contain("Sin registros");
    }

    [Fact]
    public void MxPaginator_ShouldRenderCurrentPage()
    {
        var cut = RenderComponent<MxPaginator>(p => p
            .Add(x => x.CurrentPage, 2)
            .Add(x => x.TotalItems, 30)
            .Add(x => x.PageSize, 10));

        cut.Markup.Should().Contain("Página 2 de 3");
    }

    private sealed record Row(string Name);
}
