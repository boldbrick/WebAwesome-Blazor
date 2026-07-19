using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A radio button component that allows the user to select a single option from a group.
/// Must be used as a child of WaRadioGroup.
/// Corresponds to the wa-radio Web Awesome component.
/// </summary>
public class WaRadio : ComponentBase, IFormValidation
{
    #region ------ Dependency Injection ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

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
    /// Gets or sets a collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    // Common styling parameters
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Radio properties
    /// <summary>
    /// The radio's value. When selected, the radio group receives this value.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>
    /// Checks the radio.
    /// </summary>
    [Parameter] public bool Checked { get; set; }

    /// <summary>
    /// Disables the radio.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The radio's size. When used inside a radio group, the size is determined by the radio group's size,
    /// so this attribute can typically be omitted.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// The radio's visual appearance.
    /// </summary>
    [Parameter] public WaRadioAppearance? Appearance { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

    /// <summary>
    /// Invoked when the control gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the control loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The radio's content (label text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-radio");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(4, "value", Value);
        builder.AddAttribute(5, "checked", Checked);
        builder.AddAttribute(6, "disabled", Disabled);
        builder.AddAttributeIfNotNull(7, "size", Size?.ToHtmlValue());
        builder.AddAttributeIfNotNull(8, "appearance", Appearance?.ToHtmlValue());

        // Add event handlers
        builder.AddAttributeIfHasDelegate(10, "wa-change", OnCheckedChange);

        builder.AddAttributeIfHasDelegate(11, "onfocus", OnFocus);

        builder.AddAttributeIfHasDelegate(12, "onblur", OnBlur);

        // Add element reference capture
        builder.AddElementReferenceCapture(13, __radioReference => Element = __radioReference);

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        builder.CloseElement();
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

    #region ------ Implementation of IFormValidation ------

    /// <inheritdoc />
    public async Task SetCustomValidityAsync(string message)
    {
        if (Element is null)
            throw new InvalidOperationException("Cannot set custom validity before the component is rendered. Element reference is null.");

        await JSInterop.SetCustomValidityAsync(Element.Value, message);
    }

    #endregion
}
