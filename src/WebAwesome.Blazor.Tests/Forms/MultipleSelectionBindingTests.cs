using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Two-way binding of SelectedValues in multiple selection mode of WaSelect and WaCombobox. In this mode the elements
/// keep a string array in their live value property and report it as an array in the change event, while their value
/// attribute is read as a single option value. Until 3.12.0 the wrappers accepted only a comma-joined string from the
/// change event (so SelectedValuesChanged never fired in the browser) and rendered the selection as a comma-joined
/// value attribute (so it never reached the element). The C# half is checked here with a mocked interop module; the
/// browser half is value-sync-binding.spec.js and the SelectedValuesChanged cases of event-dispatch.spec.js.
/// </summary>
public class MultipleSelectionBindingTests : FormControlTestBase
{
    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_RendersNoValueAttribute(Type componentType)
    {
        // Arrange
        JSInterop.SetupModule(InteropModulePath);

        // Act
        var cut = RenderMultiple(componentType, new Selection(["cheese", "ham"]));

        // Assert - a joined list would be read as one option value
        Assert.False(cut.Find(TagOf(componentType)).HasAttribute(ValueAttribute));
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_FirstRender_PushesTheSelectionAsArray(Type componentType)
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);

        // Act
        RenderMultiple(componentType, new Selection(["cheese", "ham"]));

        // Assert
        var invocation = Assert.Single(SyncInvocations(module));
        Assert.Equal(ValueAttribute, invocation.Arguments[1]);
        Assert.Equal(new[] { "cheese", "ham" }, invocation.Arguments[2]);
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_ArrayChangeEvent_UpdatesSelectedValues(Type componentType)
    {
        // Arrange
        JSInterop.SetupModule(InteropModulePath);
        var selection = new Selection(["cheese"]);
        var cut = RenderMultiple(componentType, selection);

        // Act - Blazor's change reader delivers the element's array value as string[]
        cut.Find(TagOf(componentType)).Change(new[] { "cheese", "olives" });

        // Assert
        Assert.Equal(new[] { "cheese", "olives" }, selection.Values);
        Assert.Equal(1, selection.Changes);
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_ElementChange_IsNotPushedBack(Type componentType)
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var selection = new Selection(["cheese"]);
        var cut = RenderMultiple(componentType, selection);
        var pushesOnFirstRender = SyncInvocations(module).Length;

        // Act - the user picks a second option, then the parent re-renders with it as @bind-SelectedValues would
        cut.Find(TagOf(componentType)).Change(new[] { "cheese", "olives" });
        SetSelectedValues(cut, componentType, selection.Values);

        // Assert
        Assert.Equal(pushesOnFirstRender, SyncInvocations(module).Length);
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_ParentChange_IsPushedOnceAndEqualListsAreNot(Type componentType)
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        var selection = new Selection(["cheese"]);
        var cut = RenderMultiple(componentType, selection);
        var pushesOnFirstRender = SyncInvocations(module).Length;

        // Act - a C# change, then a re-render with an equal but new array
        SetSelectedValues(cut, componentType, ["ham"]);
        SetSelectedValues(cut, componentType, ["ham"]);

        // Assert
        var pushes = SyncInvocations(module).Skip(pushesOnFirstRender).ToArray();
        var invocation = Assert.Single(pushes);
        Assert.Equal(new[] { "ham" }, invocation.Arguments[2]);
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void MultipleMode_CommaSeparatedString_IsStillAccepted(Type componentType)
    {
        // Arrange
        JSInterop.SetupModule(InteropModulePath);
        var selection = new Selection([]);
        var cut = RenderMultiple(componentType, selection);

        // Act
        cut.Find(TagOf(componentType)).Change("cheese,ham");

        // Assert
        Assert.Equal(new[] { "cheese", "ham" }, selection.Values);
    }

    [Theory]
    [InlineData(typeof(WaSelect))]
    [InlineData(typeof(WaCombobox))]
    public void SingleMode_DoesNotSyncTheLiveValue(Type componentType)
    {
        // Arrange - in single mode the value attribute maps to the live value, so nothing is pushed
        var module = JSInterop.SetupModule(InteropModulePath);
        var bound = new Selection([]) { Unused = "apple" };
        var cut = Render(builder =>
        {
            builder.OpenComponent(0, componentType);
            builder.AddComponentParameter(1, nameof(WaSelect.Value), bound.Unused);
            builder.AddComponentParameter(2, nameof(WaSelect.ValueExpression), (Expression<Func<string?>>)(() => bound.Unused));
            builder.CloseComponent();
        });

        // Act
        bound.Unused = "cherry";
        if (componentType == typeof(WaSelect)) cut.FindComponent<WaSelect>().Render(p => p.Add(c => c.Value, bound.Unused));
        else cut.FindComponent<WaCombobox>().Render(p => p.Add(c => c.Value, bound.Unused));

        // Assert
        Assert.Empty(SyncInvocations(module));
        Assert.Equal("cherry", cut.Find(TagOf(componentType)).GetAttribute(ValueAttribute));
    }

    #region ------ Internals ------

    private const string SyncPropertyIdentifier = "syncProperty";
    private const string ValueAttribute = "value";

    // holds the bound selection the way a parent's @bind-SelectedValues field would
    private sealed class Selection(string[] values)
    {
        public IReadOnlyList<string> Values { get; set; } = values;

        public int Changes { get; set; }

        // the single Value: unused in multiple mode, where InputBase still needs an expression for it
        public string? Unused { get; set; }
    }

    private static string TagOf(Type componentType) => componentType == typeof(WaSelect) ? "wa-select" : "wa-combobox";

    private static JSRuntimeInvocation[] SyncInvocations(BunitJSModuleInterop module)
        => module.Invocations.Where(i => i.Identifier == SyncPropertyIdentifier).ToArray();

    private IRenderedComponent<ContainerFragment> RenderMultiple(Type componentType, Selection selection)
        => Render(ParametersFor(componentType, selection));

    // re-renders the wrapper with new SelectedValues, as the parent's @bind-SelectedValues would
    private static void SetSelectedValues(IRenderedComponent<ContainerFragment> cut, Type componentType, IReadOnlyList<string> values)
    {
        if (componentType == typeof(WaSelect)) cut.FindComponent<WaSelect>().Render(p => p.Add(c => c.SelectedValues, values));
        else cut.FindComponent<WaCombobox>().Render(p => p.Add(c => c.SelectedValues, values));
    }

    private RenderFragment ParametersFor(Type componentType, Selection selection) => builder =>
    {
        builder.OpenComponent(0, componentType);
        builder.AddComponentParameter(1, nameof(WaSelect.Multiple), true);
        builder.AddComponentParameter(2, nameof(WaSelect.SelectedValues), selection.Values);
        builder.AddComponentParameter(3, nameof(WaSelect.SelectedValuesChanged), EventCallback.Factory.Create<IReadOnlyList<string>?>(this, values =>
        {
            selection.Values = values ?? [];
            selection.Changes++;
        }));
        builder.AddComponentParameter(4, nameof(WaSelect.ValueExpression), (Expression<Func<string?>>)(() => selection.Unused));
        builder.CloseComponent();
    };

    #endregion
}
