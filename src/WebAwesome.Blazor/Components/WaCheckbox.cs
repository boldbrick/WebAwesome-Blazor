using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A checkbox input component for editing <see cref="bool"/> values.
/// Corresponds to the wa-checkbox Web Awesome component.
/// </summary>
public class WaCheckbox : WaInputBase<bool>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Draws the checkbox in an indeterminate state. This is usually applied to checkboxes that represent a
    /// "select all/none" behavior when associated checkboxes have a mix of checked and unchecked states.
    /// </summary>
    [Parameter] public bool Indeterminate { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to display next to the checkbox (typically the label text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-checkbox");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add checkbox-specific attributes
        builder.AddAttribute(20, "checked", BindConverter.FormatValue(CurrentValue));
        builder.AddAttribute(21, "indeterminate", Indeterminate);

        // Include the "value" attribute so that when this is posted by a form, "true"
        // is included in the form fields. That's how <input type="checkbox"> works normally.
        builder.AddAttribute(22, "value", bool.TrueString);

        // <wa-checkbox> is a custom element, not a native <input>, so Blazor's built-in change-event
        // value extraction (which only reads .checked when tagName === "INPUT") can't see its real
        // checked state and would always read the static "value" attribute above instead. Read the
        // real state back explicitly via JS interop rather than relying on CreateBinder<bool>.
        builder.AddAttribute(23, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleCheckedChangedAsync));

        // Add common event handlers
        AddCommonEventHandlers(builder, 30);

        // Add checkbox-specific event handlers
        builder.AddAttributeIfHasDelegate(40, "wa-change", OnCheckedChange);

        builder.AddAttributeIfHasDelegate(42, "wa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(41, __checkboxReference => Element = __checkboxReference);

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
    /// Sets focus on the checkbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the checkbox before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the checkbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the checkbox before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Simulates a click on the checkbox.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ClickAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot click the checkbox before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "click");
    }

    #endregion

    #region ------ Internals ------

    // reads the checkbox's real checked state via JS interop, since the "change" event's own value
    // (Blazor reads .value for non-<input> elements) is a static placeholder, not the actual state
    private async Task HandleCheckedChangedAsync(ChangeEventArgs args)
    {
        if (Element is null) return;

        CurrentValue = await JSInterop.GetPropertyAsync<bool>(Element.Value, "checked");
    }

    #endregion

}
