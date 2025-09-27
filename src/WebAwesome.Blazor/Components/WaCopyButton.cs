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

    [Inject] protected WebAwesomeJSInterop JSInterop { get; set; } = default!;

    #endregion

    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be <see langword="null"/> if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    // Common styling parameters
    [Parameter] public string? Class { get; set; }
    [Parameter] public string? Style { get; set; }

    // Copy functionality
    [Parameter] public string? Value { get; set; }
    [Parameter] public string? From { get; set; }
    [Parameter] public bool Disabled { get; set; }

    // Labels for different states
    [Parameter] public string? CopyLabel { get; set; } = "Copy to clipboard";
    [Parameter] public string? SuccessLabel { get; set; } = "Copied!";
    [Parameter] public string? ErrorLabel { get; set; } = "Copy failed";

    // Feedback duration in milliseconds
    [Parameter] public int FeedbackDuration { get; set; } = 1000;

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback OnCopy { get; set; }
    [Parameter] public EventCallback OnSuccess { get; set; }
    [Parameter] public EventCallback OnError { get; set; }

    #endregion

    #region ------ Content Slots ------

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

        // Add event handlers
        if (OnCopy.HasDelegate)
            builder.AddAttribute(20, "wa-copy", OnCopy);

        if (OnSuccess.HasDelegate)
            builder.AddAttribute(21, "wa-success", OnSuccess);

        if (OnError.HasDelegate)
            builder.AddAttribute(22, "wa-error", OnError);

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

        if (SuccessIcon is not null)
        {
            builder.OpenElement(40, "span");
            builder.AddAttribute(41, "slot", "success-icon");
            builder.AddContent(42, SuccessIcon);
            builder.CloseElement();
        }

        if (ErrorIcon is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "error-icon");
            builder.AddContent(52, ErrorIcon);
            builder.CloseElement();
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

        await JSInterop.InvokeMethodAsync(Element.Value, "copy");
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
