using Bunit.JSInterop;
using Bunit;
using FluentAssertions;
using MachSoft.UI.Components;
using MachSoft.UI.Models;
using MudBlazor.Services;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxBlock1ControlsTests : TestContext
{
    public MxBlock1ControlsTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddMudServices();
    }

    [Fact]
    public void MxTextArea_ShouldRenderLabelAndHelper()
    {
        var cut = RenderComponent<MxTextArea>(p => p
            .Add(x => x.Label, "Notas")
            .Add(x => x.HelperText, "Describe la operación."));

        cut.Markup.Should().Contain("Notas").And.Contain("Describe la operación.");
    }

    [Fact]
    public void MxAlert_ShouldRenderToneClass()
    {
        var cut = RenderComponent<MxAlert>(p => p
            .Add(x => x.Tone, MxAlertTone.Warning)
            .AddChildContent("Alerta"));

        cut.Markup.Should().Contain("mx-alert-warning").And.Contain("Alerta");
    }

    [Fact]
    public void MxTabs_ShouldRenderTabLabels()
    {
        var tabs = new[]
        {
            new MxTabItem("a", "Resumen", b => b.AddContent(0, "Tab A")),
            new MxTabItem("b", "Historial", b => b.AddContent(0, "Tab B"))
        };

        var cut = RenderComponent<MxTabs>(p => p.Add(x => x.Items, tabs));

        cut.Markup.Should().Contain("Resumen").And.Contain("Historial");
    }
}
