using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A switch component that allows the user to toggle an option on or off.
/// Corresponds to the wa-switch Web Awesome component.
/// </summary>
public class WaSwitch : WaInputBase<bool>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Whether the switch is checked (mirrors CurrentValue for convenience)
    /// </summary>
    [Parameter]
    public bool Checked
    {
        get => CurrentValue;
        set => CurrentValue = value;
    }

    /// <summary>
    /// Reserves space for the hint even when it is not populated.
    /// </summary>
    [Parameter] public bool WithHint { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the switch's checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints are not satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to display next to the switch (typically the label text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-switch");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add switch-specific attributes
        builder.AddAttribute(20, "checked", BindConverter.FormatValue(CurrentValue));

        // Include the "value" attribute so that when this is posted by a form, "true"
        // is included in the form fields. That's how <input type="checkbox"> works normally.
        builder.AddAttribute(21, "value", bool.TrueString);
        builder.AddAttribute(23, "with-hint", WithHint);

        // <wa-switch> is a custom element, not a native <input>, so Blazor's built-in change-event
        // value extraction (which only reads .checked when tagName === "INPUT") can't see its real
        // checked state and would always read the static "value" attribute above instead. Read the
        // real state back explicitly via JS interop rather than relying on CreateBinder<bool>.
        builder.AddAttribute(22, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleCheckedChangedAsync));

        // Add common event handlers
        AddCommonEventHandlers(builder, 30);

        // Add switch-specific event handlers
        builder.AddAttributeIfHasDelegate(40, "wa-change", OnCheckedChange);
        builder.AddAttributeIfHasDelegate(42, "wa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(43, __switchReference => Element = __switchReference);

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(50, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 60);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
        => throw new NotSupportedException($"This component does not parse string inputs. Bind to the '{nameof(CurrentValue)}' property, not '{nameof(CurrentValueAsString)}'.");

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Removes focus from the switch.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Simulates a click on the switch.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ClickAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot click: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "click");
    }

    /// <summary>
    /// Sets focus on the switch.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    #endregion

    #region ------ Internals ------

    // reads the switch's real checked state via JS interop, since the "change" event's own value
    // (Blazor reads .value for non-<input> elements) is a static placeholder, not the actual state
    private async Task HandleCheckedChangedAsync(ChangeEventArgs args)
    {
        if (Element is null) return;

        CurrentValue = await JSInterop.GetPropertyAsync<bool>(Element.Value, "checked");
    }

    #endregion

}
