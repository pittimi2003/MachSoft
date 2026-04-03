using Bunit;
using FluentAssertions;
using MachSoft.UI.Components;
using MachSoft.UI.Models;
using Microsoft.AspNetCore.Components;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxWorkspaceLayoutTests : TestContext
{
    [Fact]
    public void Render_WithBothSidebarsClosed_ShouldUseOnlyCentralColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarVisible, false)
            .Add(x => x.RightSidebarVisible, false));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.FindAll("[data-mode='inline']").Should().BeEmpty();
        cut.FindAll("[data-mode='overlay']").Should().BeEmpty();
    }

    [Fact]
    public void Render_WithLeftInline_ShouldReserveLeftColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarVisible, true)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.LeftSidebarWidth, "280px"));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("280px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithRightInline_ShouldReserveRightColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.RightSidebarVisible, true)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarWidth, "420px"));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 420px");
        cut.Find("[data-sidebar='right'][data-mode='inline']").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithLeftOverlay_ShouldNotReduceCentralGrid()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarVisible, true)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarWidth, "320px"));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='overlay']").Should().NotBeNull();
        cut.Find(".mx-workspace-backdrop").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithRightOverlay_ShouldNotReduceCentralGrid()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.RightSidebarVisible, true)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
        cut.Find(".mx-workspace-backdrop").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithLeftInlineAndRightOverlay_ShouldOnlyReserveLeftColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarVisible, true)
            .Add(x => x.RightSidebarVisible, true)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarWidth, "300px")
            .Add(x => x.RightSidebarWidth, "380px"));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("300px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
    }

    [Fact]
    public void Render_WithFooter_ShouldRenderFooterRegion()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.FooterContent, b => b.AddContent(0, "footer-state")));

        cut.Find(".mx-workspace-footer").TextContent.Should().Contain("footer-state");
    }

    [Fact]
    public void Render_WithoutFooter_ShouldNotRenderFooterRegion()
    {
        var cut = RenderLayout();

        cut.FindAll(".mx-workspace-footer").Should().BeEmpty();
    }

    [Fact]
    public void ToggleMenu_ShouldNotifyChangedAndOnMenuToggle()
    {
        var changed = false;
        var nextState = true;
        var toggled = false;

        var cut = RenderLayout(p => p
            .Add(x => x.MainMenuOpen, false)
            .Add(x => x.MainMenuOpenChanged, EventCallback.Factory.Create<bool>(this, value =>
            {
                changed = true;
                nextState = value;
            }))
            .Add(x => x.OnMenuToggle, EventCallback.Factory.Create(this, () => toggled = true)));

        cut.Find(".mx-workspace-menu-toggle").Click();

        changed.Should().BeTrue();
        toggled.Should().BeTrue();
        nextState.Should().BeTrue();
    }

    [Fact]
    public void Header_ShouldBePersistentWithExpectedHeightMarker()
    {
        var cut = RenderLayout();

        var header = cut.Find(".mx-workspace-header");
        header.GetAttribute("data-header-height").Should().Be("48");
    }

    [Fact]
    public void SidebarVisibleWithoutContent_ShouldStillRenderContainer()
    {
        var cut = RenderComponent<MxWorkspaceLayout>(p => p
            .Add(x => x.MainContent, b => b.AddContent(0, "main"))
            .Add(x => x.LeftSidebarVisible, true)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline));

        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
    }

    [Fact]
    public void OverlayVisibleWithoutContent_ShouldKeepMainGridUnchanged()
    {
        var cut = RenderComponent<MxWorkspaceLayout>(p => p
            .Add(x => x.MainContent, b => b.AddContent(0, "main"))
            .Add(x => x.RightSidebarVisible, true)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay));

        cut.Find(".mx-workspace-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
    }

    private IRenderedComponent<MxWorkspaceLayout> RenderLayout(Action<ComponentParameterCollectionBuilder<MxWorkspaceLayout>>? setup = null)
    {
        return RenderComponent<MxWorkspaceLayout>(p =>
        {
            p.Add(x => x.MainContent, b => b.AddContent(0, "main-workspace"));
            p.Add(x => x.LeftSidebar, b => b.AddContent(0, "left-panel"));
            p.Add(x => x.RightSidebar, b => b.AddContent(0, "right-panel"));
            setup?.Invoke(p);
        });
    }
}
