using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// A wrapper's callbacks see only the events of its own element. Web Awesome's events bubble and Blazor delivers a
/// bubbling event to every handler up the tree, so before the fix the wa-hide of a WaSelect closing inside a WaDialog
/// also invoked WaDialog.OnHide, and the Overlays showcase's invite dialog closed as soon as a role was picked. bUnit
/// dispatches events the way Blazor does (bubbling, honouring stopPropagation).
/// </summary>
public class NestedEventPropagationTests : BunitContext
{
    public NestedEventPropagationTests()
    {
        Services.AddWebAwesome();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task NestedSelectHide_DoesNotInvokeTheDialogsOnHide()
    {
        // Arrange - a select without its own OnHide inside a dialog with one, and a second select with one
        var dialogHides = 0;
        var selectHides = 0;
        var cut = Render(builder =>
        {
            builder.OpenComponent<WaDialog>(0);
            builder.AddComponentParameter(1, nameof(WaDialog.OnHide), EventCallback.Factory.Create<EventArgs>(this, () => dialogHides++));
            builder.AddComponentParameter(2, nameof(WaDialog.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<WaSelect>(0);
                content.AddComponentParameter(1, IdAttribute, UnboundSelectId);
                content.AddComponentParameter(5, nameof(WaSelect.ValueExpression), SelectValueExpression);
                content.CloseComponent();
                content.OpenComponent<WaSelect>(2);
                content.AddComponentParameter(3, IdAttribute, BoundSelectId);
                content.AddComponentParameter(6, nameof(WaSelect.ValueExpression), SelectValueExpression);
                content.AddComponentParameter(4, nameof(WaSelect.OnHide), EventCallback.Factory.Create<EventArgs>(this, () => selectHides++));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Act - each select closes, then the dialog itself
        await cut.Find($"#{BoundSelectId}").TriggerEventAsync(HideEventAttribute, EventArgs.Empty);
        await Assert.ThrowsAsync<MissingEventHandlerException>(() => cut.Find($"#{UnboundSelectId}").TriggerEventAsync(HideEventAttribute, EventArgs.Empty));
        await cut.Find("wa-dialog").TriggerEventAsync(HideEventAttribute, EventArgs.Empty);

        // Assert - the unbound select's event stopped at it (no handler left to reach), the bound one reached only its
        // own callback, and the dialog's callback ran for the dialog's own event only
        Assert.Equal(1, selectHides);
        Assert.Equal(1, dialogHides);
    }

    [Fact]
    public async Task NestedTabGroupTabShow_DoesNotInvokeTheOuterTabGroupsCallback()
    {
        // Arrange - a tab group inside a tab group's panel, both tracking the shown tab
        string? outerShown = null;
        string? innerShown = null;
        var cut = Render(builder =>
        {
            builder.OpenComponent<WaTabGroup>(0);
            builder.AddComponentParameter(1, IdAttribute, OuterTabGroupId);
            builder.AddComponentParameter(2, nameof(WaTabGroup.OnTabChange), EventCallback.Factory.Create<WaTabChangeEventArgs>(this, e => outerShown = e.Name));
            builder.AddComponentParameter(3, nameof(WaTabGroup.ChildContent), (RenderFragment)(content =>
            {
                content.OpenComponent<WaTabGroup>(0);
                content.AddComponentParameter(1, IdAttribute, InnerTabGroupId);
                content.AddComponentParameter(2, nameof(WaTabGroup.OnTabChange), EventCallback.Factory.Create<WaTabChangeEventArgs>(this, e => innerShown = e.Name));
                content.CloseComponent();
            }));
            builder.CloseComponent();
        });

        // Act
        await cut.Find($"#{InnerTabGroupId}").TriggerEventAsync(TabShowEventAttribute, new WaTabChangeEventArgs { Name = InnerPanelName });

        // Assert
        Assert.Equal(InnerPanelName, innerShown);
        Assert.Null(outerShown);
    }

    #region ------ Internals ------

    // an unmatched attribute, rendered on the wrapper's element
    private const string IdAttribute = "id";
    private const string HideEventAttribute = "onwa-hide";
    private const string TabShowEventAttribute = "onwa-tab-show";
    private const string UnboundSelectId = "unbound-select";
    private const string BoundSelectId = "bound-select";
    private const string OuterTabGroupId = "outer-tabs";
    private const string InnerTabGroupId = "inner-tabs";
    private const string InnerPanelName = "inner-panel";

    // the value a WaSelect (an InputBase) must be bound to
    private string? selectValue = null;

    private Expression<Func<string?>> SelectValueExpression => () => selectValue;

    #endregion
}
