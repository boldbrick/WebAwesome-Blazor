using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A select component that allows choosing items from a menu of predefined options.
/// Corresponds to the wa-select Web Awesome component.
/// </summary>
public class WaSelect : WaInputBase<string?>
{
    #region ------ Visual & Behavior Properties ------

    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public WaInputAppearance? Appearance { get; set; }
    [Parameter] public bool Pill { get; set; }
    [Parameter] public bool WithClear { get; set; }
    [Parameter] public bool Multiple { get; set; }
    [Parameter] public int? MaxOptionsVisible { get; set; }
    [Parameter] public WaPlacement? Placement { get; set; }

    #endregion

    #region ------ Multiple Selection Support ------

    /// <summary>
    /// The selected values when Multiple is true. Use this for two-way binding in multiple selection mode.
    /// </summary>
    [Parameter] public string[]? SelectedValues { get; set; }

    /// <summary>
    /// Callback for when SelectedValues changes in multiple selection mode.
    /// </summary>
    [Parameter] public EventCallback<string[]?> SelectedValuesChanged { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback OnClear { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Content to display at the start of the select
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the select
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// The options to display in the select
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ JavaScript Interop Properties ------

    /// <summary>
    /// Custom tag generation function for multiple selection mode.
    /// Note: This requires JavaScript interop to implement.
    /// </summary>
    /// <remarks>
    /// In the actual implementation, this would be a JavaScript function that gets called
    /// for each selected option to generate custom tag markup.
    /// </remarks>
    public Func<WaOption, int, string>? GetTag { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-select");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add select-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(20, "placeholder", Placeholder);
        builder.AddAttributeIfNotNull(21, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttribute(22, "pill", Pill);
        builder.AddAttribute(23, "with-clear", WithClear);
        builder.AddAttribute(24, "multiple", Multiple);
        builder.AddAttributeIfNotNull(25, "max-options-visible", MaxOptionsVisible);
        builder.AddAttributeIfNotNull(26, "placement", Placement?.ToHtmlValue());

        // Add value binding - handle both single and multiple selection
        if (Multiple)
        {
            // For multiple selection, we need special handling
            var selectedValuesString = SelectedValues != null ? string.Join(",", SelectedValues) : string.Empty;
            builder.AddAttribute(30, "value", selectedValuesString);
            builder.AddAttribute(31, "onchange", EventCallback.Factory.Create<ChangeEventArgs>(this, HandleMultipleSelectionChange));
        }
        else
        {
            // For single selection, use standard binding
            builder.AddAttribute(30, "value", CurrentValueAsString);
            builder.AddAttribute(31, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
        }

        builder.SetUpdatesAttributeName("value");

        // Add common event handlers
        AddCommonEventHandlers(builder, 40);

        // Add select-specific event handlers
        if (OnClear.HasDelegate)
            builder.AddAttribute(50, "wa-clear", OnClear);

        // Add element reference capture
        builder.AddElementReferenceCapture(51, __selectReference => Element = __selectReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(60, "span");
            builder.AddAttribute(61, "slot", "start");
            builder.AddContent(62, StartContent);
            builder.CloseElement();
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(65, "span");
            builder.AddAttribute(66, "slot", "end");
            builder.AddContent(67, EndContent);
            builder.CloseElement();
        }

        // Add child content (options)
        if (ChildContent is not null)
        {
            builder.AddContent(70, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 80);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #endregion

    #region ------ Private Methods ------

    /// <summary>
    /// Handles change events for multiple selection mode
    /// </summary>
    private async Task HandleMultipleSelectionChange(ChangeEventArgs args)
    {
        if (args.Value is string stringValue)
        {
            // Parse the comma-separated values
            var values = string.IsNullOrEmpty(stringValue)
                ? Array.Empty<string>()
                : stringValue.Split(',', StringSplitOptions.RemoveEmptyEntries);

            SelectedValues = values;
            await SelectedValuesChanged.InvokeAsync(values);

            // Also update the single value for consistency (use first selected or null)
            var singleValue = values.FirstOrDefault();
            if (CurrentValueAsString != singleValue)
            {
                CurrentValueAsString = singleValue;
            }
        }
    }

    #endregion

    #region ------ Public Methods ------


    /// <summary>
    /// Sets the custom tag generation function for multiple selection mode.
    /// </summary>
    /// <param name="jsFunction">JavaScript function string that generates custom HTML for each selected option</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered or the operation fails</exception>
    /// <exception cref="ArgumentNullException">Thrown when jsFunction is null or empty</exception>
    public async Task SetGetTagFunctionAsync(string jsFunction)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot set get tag function: component has not been rendered yet.");

        if (string.IsNullOrEmpty(jsFunction))
            throw new ArgumentNullException(nameof(jsFunction));

        await JSInterop.SetPropertyAsync(Element.Value, "getTag", jsFunction);
    }

    #endregion
}
