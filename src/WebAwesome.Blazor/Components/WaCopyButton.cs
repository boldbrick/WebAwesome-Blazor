using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A copy button component that copies data to the clipboard when clicked.
/// Corresponds to the wa-copy-button Web Awesome component.
/// </summary>
public class WaCopyButton : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to invoke methods on the underlying element.
    /// </summary>
    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS classes to apply to the component.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline styles to apply to the component.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Copy functionality
    /// <summary>
    /// The text value to copy.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// An id referencing an element in the same document from which data is copied. If both this and
    /// <see cref="Value"/> are set, this value takes precedence. By default, the target element's
    /// <c>textContent</c> is copied. To copy an attribute, append the attribute name wrapped in square brackets,
    /// e.g. <c>from="el[value]"</c>. To copy a property, append a dot and the property name, e.g.
    /// <c>from="el.value"</c>.
    /// </summary>
    [Parameter] public string? From { get; set; }

    /// <summary>
    /// Disables the copy button.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    // Labels for different states
    /// <summary>
    /// A custom label to show in the tooltip.
    /// </summary>
    [Parameter] public string? CopyLabel { get; set; } = "Copy to clipboard";

    /// <summary>
    /// A custom label to show in the tooltip after copying.
    /// </summary>
    [Parameter] public string? SuccessLabel { get; set; } = "Copied!";

    /// <summary>
    /// A custom label to show in the tooltip when a copy error occurs.
    /// </summary>
    [Parameter] public string? ErrorLabel { get; set; } = "Copy failed";

    // Feedback duration in milliseconds
    /// <summary>
    /// The length of time, in milliseconds, to show feedback before restoring the default trigger.
    /// </summary>
    [Parameter] public int FeedbackDuration { get; set; } = 1000;

    /// <summary>
    /// The placement of the tooltip shown for the copy, success, and error labels.
    /// </summary>
    [Parameter] public WaPlacement? TooltipPlacement { get; set; }

    /// <summary>
    /// Controls the built-in tooltip. <see cref="WaCopyButtonTooltip.Full"/> (default) shows the tooltip on hover
    /// and focus and during copy feedback; <see cref="WaCopyButtonTooltip.Copy"/> keeps it silent on hover/focus and
    /// only shows it briefly to confirm a copy; <see cref="WaCopyButtonTooltip.None"/> disables it entirely.
    /// </summary>
    [Parameter] public WaCopyButtonTooltip Tooltip { get; set; } = WaCopyButtonTooltip.Full;

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the data has been copied.
    /// </summary>
    [Parameter] public EventCallback OnCopy { get; set; }

    /// <summary>
    /// Invoked when the success feedback state is shown.
    /// </summary>
    [Parameter] public EventCallback OnSuccess { get; set; }

    /// <summary>
    /// Invoked when the data could not be copied.
    /// </summary>
    [Parameter] public EventCallback OnError { get; set; }

    #endregion

    #region ------ Content Slots ------

    /// <summary>
    /// The trigger element. By default a copy icon button is rendered, so this is optional. Slot in a custom
    /// element such as a <see cref="WaButton"/> or a native button to override it.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Icon to display for the copy state
    /// </summary>
    [Parameter] public RenderFragment? CopyIcon { get; set; }

    /// <summary>
    /// Icon to display for the success state
    /// </summary>
    [Parameter] public RenderFragment? SuccessIcon { get; set; }

    /// <summary>
    /// Icon to display for the error state
    /// </summary>
    [Parameter] public RenderFragment? ErrorIcon { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="CopyIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? CopyIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="SuccessIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? SuccessIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="ErrorIcon"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? ErrorIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-copy-button");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "value", Value);
        builder.AddAttributeIfNotNullOrEmpty(5, "from", From);
        builder.AddAttribute(6, "disabled", Disabled);
        builder.AddAttributeIfNotNullOrEmpty(7, "copy-label", CopyLabel);
        builder.AddAttributeIfNotNullOrEmpty(8, "success-label", SuccessLabel);
        builder.AddAttributeIfNotNullOrEmpty(9, "error-label", ErrorLabel);
        builder.AddAttribute(10, "feedback-duration", FeedbackDuration);
        builder.AddAttributeIfNotNull(11, "tooltip-placement", TooltipPlacement?.ToHtmlValue());
        builder.AddAttribute(12, "tooltip", Tooltip.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-copy", OnCopy);

        builder.AddAttributeIfHasDelegate(21, "onwa-success", OnSuccess);

        builder.AddAttributeIfHasDelegate(22, "onwa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __copyButtonReference => Element = __copyButtonReference);

        // Add icon slots
        if (CopyIcon is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "copy-icon");
            builder.AddContent(32, CopyIcon);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(60, "copy-icon", CopyIconName);
        }

        if (SuccessIcon is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "success-icon");
            builder.AddContent(42, SuccessIcon);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(65, "success-icon", SuccessIconName);
        }

        if (ErrorIcon is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "error-icon");
            builder.AddContent(52, ErrorIcon);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(70, "error-icon", ErrorIconName);
        }

        // default slot: custom trigger element (optional)
        if (ChildContent is not null)
        {
            builder.AddContent(80, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Programmatically trigger the copy operation.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task CopyAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot copy: component has not been rendered yet.");

        // wa-copy-button exposes no copy() method in WA 3.0; clicking the element triggers
        // the copy operation exactly like a user interaction
        await JSInterop.InvokeMethodAsync(Element.Value, "click");
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
