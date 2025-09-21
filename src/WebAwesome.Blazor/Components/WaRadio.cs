using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A radio button component that allows the user to select a single option from a group.
/// Must be used as a child of WaRadioGroup.
/// Corresponds to the wa-radio Web Awesome component.
/// </summary>
public class WaRadio : ComponentBase
{
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

    // Radio properties
    [Parameter] public string? Value { get; set; }
    [Parameter] public bool Checked { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public WaSize? Size { get; set; }
    [Parameter] public WaRadioAppearance? Appearance { get; set; }

    #endregion

    #region ------ Events ------

    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }
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
        if (OnCheckedChange.HasDelegate)
            builder.AddAttribute(10, "wa-change", OnCheckedChange);

        if (OnFocus.HasDelegate)
            builder.AddAttribute(11, "onfocus", OnFocus);

        if (OnBlur.HasDelegate)
            builder.AddAttribute(12, "onblur", OnBlur);

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
}
