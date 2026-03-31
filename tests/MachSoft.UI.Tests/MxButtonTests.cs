using Bunit;
using FluentAssertions;
using MachSoft.UI.Components;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxButtonTests : TestContext
{
    [Fact]
    public void Render_ShouldContainLabel()
    {
        var cut = RenderComponent<MxButton>(p => p.Add(x => x.Label, "Guardar"));
        cut.Markup.Should().Contain("Guardar");
    }
}
