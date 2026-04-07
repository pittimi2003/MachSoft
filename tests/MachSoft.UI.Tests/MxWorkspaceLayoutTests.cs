using Bunit;
using Bunit.JSInterop;
using FluentAssertions;
using MachSoft.UI.Components;
using MachSoft.UI.Models;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace MachSoft.UI.Tests;

public class MxWorkspaceLayoutTests : TestContext
{
    public MxWorkspaceLayoutTests()
    {
        Services.AddMudServices();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void Render_WithMainMenuClosed_ShouldNotRenderNavigationMenu()
    {
        var cut = RenderLayout(p => p.Add(x => x.MainMenuOpen, false));

        cut.FindAll("[data-region='navigation-menu']").Should().BeEmpty();
        cut.Find(".mx-workspace-layout").GetAttribute("data-main-menu-open").Should().Be("false");
        cut.Find(".mx-workspace-layout").GetAttribute("data-navigation-mode-effective").Should().Be("overlay");
    }

    [Fact]
    public void ToggleMenu_ShouldOpenNavigationMenuAndNotifyCallbacks()
    {
        var changed = false;
        var nextState = false;
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
    public void Render_LeftInline_ShouldAlwaysRenderInlineRegionAndReserveLeftColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.LeftSidebarOpen, false)
            .Add(x => x.LeftSidebarWidth, "280px"));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("280px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
        cut.FindAll("[data-sidebar='left'][data-mode='overlay']").Should().BeEmpty();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("center-with-left-inline");
    }

    [Fact]
    public void Render_LeftOverlayClosed_ShouldNotRenderStructuralOrOverlayRegion()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, false));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.FindAll("[data-sidebar='left']").Should().BeEmpty();
        cut.FindAll(".mx-workspace-backdrop").Should().BeEmpty();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("full");
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
    }

    [Fact]
    public void Render_LeftOverlayOpen_ShouldRenderOverlayWithBackdropWithoutStructuralWidthLoss()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, true));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='overlay']").Should().NotBeNull();
        cut.Find(".mx-workspace-backdrop").Should().NotBeNull();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
        cut.Find(".mx-workspace-main-region").ClassList.Should().Contain("mx-workspace-main-region-neutral");
    }

    [Fact]
    public void Render_RightInline_ShouldAlwaysRenderInlineRegionAndReserveRightColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarOpen, false)
            .Add(x => x.RightSidebarWidth, "420px"));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 420px");
        cut.Find("[data-sidebar='right'][data-mode='inline']").Should().NotBeNull();
        cut.FindAll("[data-sidebar='right'][data-mode='overlay']").Should().BeEmpty();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("center-with-right-inline");
    }

    [Fact]
    public void Render_RightOverlayClosed_ShouldNotRenderStructuralOrOverlayRegion()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarOpen, false));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.FindAll("[data-sidebar='right']").Should().BeEmpty();
        cut.FindAll(".mx-workspace-backdrop").Should().BeEmpty();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("full");
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
    }

    [Fact]
    public void Render_RightOverlayOpen_ShouldRenderOverlayWithBackdropWithoutStructuralWidthLoss()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarOpen, true));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
        cut.Find(".mx-workspace-backdrop").Should().NotBeNull();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
        cut.Find(".mx-workspace-main-region").ClassList.Should().Contain("mx-workspace-main-region-neutral");
    }

    [Fact]
    public void Render_LeftInlineRightOverlayOpen_ShouldOnlyReserveLeftColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarOpen, true)
            .Add(x => x.LeftSidebarWidth, "300px"));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("300px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
    }

    [Fact]
    public void Render_LeftOverlayOpenRightInline_ShouldOnlyReserveRightColumn()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, true)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarWidth, "380px"));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 380px");
        cut.Find("[data-sidebar='left'][data-mode='overlay']").Should().NotBeNull();
        cut.Find("[data-sidebar='right'][data-mode='inline']").Should().NotBeNull();
    }

    [Fact]
    public void Render_BothInline_ShouldReserveBothColumns()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.LeftSidebarWidth, "260px")
            .Add(x => x.RightSidebarWidth, "340px"));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("260px minmax(0, 1fr) 340px");
        cut.Find("[data-sidebar='left'][data-mode='inline']").Should().NotBeNull();
        cut.Find("[data-sidebar='right'][data-mode='inline']").Should().NotBeNull();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("center-with-both-inline");
    }

    [Fact]
    public void Render_BothOverlayClosed_ShouldUseOnlyCenterColumnWithoutBackdrop()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, false)
            .Add(x => x.RightSidebarOpen, false));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.FindAll("[data-sidebar='left'][data-mode='overlay']").Should().BeEmpty();
        cut.FindAll("[data-sidebar='right'][data-mode='overlay']").Should().BeEmpty();
        cut.FindAll(".mx-workspace-backdrop").Should().BeEmpty();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("full");
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
    }

    [Fact]
    public void Render_BothOverlayOpen_ShouldRenderBothPanelsBackdropAndVisualState()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, true)
            .Add(x => x.RightSidebarOpen, true));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("0px minmax(0, 1fr) 0px");
        cut.Find("[data-sidebar='left'][data-mode='overlay']").Should().NotBeNull();
        cut.Find("[data-sidebar='right'][data-mode='overlay']").Should().NotBeNull();
        cut.Find(".mx-workspace-backdrop").Should().NotBeNull();
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-visual-state").Should().Be("neutral");
        cut.Find(".mx-workspace-main-region").ClassList.Should().Contain("mx-workspace-main-region-neutral");
        cut.Find(".mx-workspace-main-region").GetAttribute("data-main-structural-span").Should().Be("full");
    }


    [Fact]
    public void Render_WithCustomSidebarWidths_ShouldExposeCssVariables()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarWidth, "240px")
            .Add(x => x.RightSidebarWidth, "420px"));

        var style = cut.Find(".mx-workspace-layout").GetAttribute("style");
        style.Should().Contain("--mx-left-sidebar-width:240px");
        style.Should().Contain("--mx-right-sidebar-width:420px");
    }

    [Fact]
    public void Render_WithBlankSidebarWidths_ShouldFallbackToContractDefaults()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.LeftSidebarWidth, "   ")
            .Add(x => x.RightSidebarWidth, string.Empty));

        cut.Find(".mx-workspace-functional-grid").GetAttribute("style").Should().Contain("320px minmax(0, 1fr) 360px");
    }

    [Fact]
    public void Header_ShouldRenderShellControlMenu_AndMainMenuShouldRemainIndependent()
    {
        var cut = RenderLayout(p => p
            .Add(x => x.MainMenuOpen, false)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Inline)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarOpen, false));

        cut.Find(".mx-workspace-header").GetAttribute("data-header-height").Should().Be("48");
        cut.Find(".mx-workspace-shell-control-menu").Should().NotBeNull();
        cut.FindAll("[data-region='navigation-menu']").Should().BeEmpty();

        cut.Find(".mx-workspace-menu-toggle").Click();

        cut.Find(".mx-workspace-layout").GetAttribute("data-main-menu-open").Should().Be("true");
    }

    [Fact]
    public void Render_WithFooterContent_ShouldRenderFooter()
    {
        var cut = RenderLayout(p => p.Add(x => x.FooterContent, b => b.AddContent(0, "footer-meta")));

        cut.Find(".mx-workspace-footer").TextContent.Should().Contain("footer-meta");
    }

    [Fact]
    public void Render_WithoutFooterContent_ShouldNotRenderFooter()
    {
        var cut = RenderLayout();

        cut.FindAll(".mx-workspace-footer").Should().BeEmpty();
    }

    [Fact]
    public void ShellMenu_SetLeftInline_ShouldCloseLeftOverlayState()
    {
        var leftModeChanged = MxSidebarMode.Overlay;
        var leftOpenChanged = true;

        var cut = RenderLayout(p => p
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, true)
            .Add(x => x.LeftSidebarModeChanged, EventCallback.Factory.Create<MxSidebarMode>(this, mode => leftModeChanged = mode))
            .Add(x => x.LeftSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, open => leftOpenChanged = open)));

        var shellMenu = cut.FindComponent<MxMenu>();
        shellMenu.InvokeAsync(() => shellMenu.Instance.OnItemSelected.InvokeAsync("left.mode.inline"));

        leftModeChanged.Should().Be(MxSidebarMode.Inline);
        leftOpenChanged.Should().BeFalse();
    }

    [Fact]
    public void ShellMenu_SetRightInline_ShouldCloseRightOverlayState()
    {
        var rightModeChanged = MxSidebarMode.Overlay;
        var rightOpenChanged = true;

        var cut = RenderLayout(p => p
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarOpen, true)
            .Add(x => x.RightSidebarModeChanged, EventCallback.Factory.Create<MxSidebarMode>(this, mode => rightModeChanged = mode))
            .Add(x => x.RightSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, open => rightOpenChanged = open)));

        var shellMenu = cut.FindComponent<MxMenu>();
        shellMenu.InvokeAsync(() => shellMenu.Instance.OnItemSelected.InvokeAsync("right.mode.inline"));

        rightModeChanged.Should().Be(MxSidebarMode.Inline);
        rightOpenChanged.Should().BeFalse();
    }

    [Fact]
    public void BackdropClick_ShouldCloseNavigationAndOverlaySidebars()
    {
        var menuChanged = true;
        var leftChanged = true;
        var rightChanged = true;

        var cut = RenderLayout(p => p
            .Add(x => x.MainMenuOpen, true)
            .Add(x => x.LeftSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.RightSidebarMode, MxSidebarMode.Overlay)
            .Add(x => x.LeftSidebarOpen, true)
            .Add(x => x.RightSidebarOpen, true)
            .Add(x => x.MainMenuOpenChanged, EventCallback.Factory.Create<bool>(this, value => menuChanged = value))
            .Add(x => x.LeftSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, value => leftChanged = value))
            .Add(x => x.RightSidebarOpenChanged, EventCallback.Factory.Create<bool>(this, value => rightChanged = value)));

        cut.Find(".mx-workspace-backdrop").Click();

        menuChanged.Should().BeFalse();
        leftChanged.Should().BeFalse();
        rightChanged.Should().BeFalse();
    }

    private IRenderedComponent<MxWorkspaceLayout> RenderLayout(Action<ComponentParameterCollectionBuilder<MxWorkspaceLayout>>? setup = null)
    {
        RenderComponent<MudPopoverProvider>();

        return RenderComponent<MxWorkspaceLayout>(p =>
        {
            p.Add(x => x.NavigationMenu, b => b.AddContent(0, "nav-menu"));
            p.Add(x => x.MainContent, b => b.AddContent(0, "main-workspace"));
            p.Add(x => x.LeftSidebar, b => b.AddContent(0, "left-panel"));
            p.Add(x => x.RightSidebar, b => b.AddContent(0, "right-panel"));
            setup?.Invoke(p);
        });
    }
}
