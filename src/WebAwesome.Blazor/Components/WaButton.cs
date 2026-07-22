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
/// A button component that represents actions available to the user.
/// Corresponds to the wa-button Web Awesome component.
/// </summary>
public class WaButton : ComponentBase, IFormValidation
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

    // Visual & behavior properties
    /// <summary>
    /// The button's theme variant. Defaults to <c>neutral</c> if not within another element with a variant.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// The button's visual appearance.
    /// </summary>
    [Parameter] public WaAppearance? Appearance { get; set; }

    /// <summary>
    /// The button's size.
    /// </summary>
    [Parameter] public WaSize? Size { get; set; }

    /// <summary>
    /// Draws a pill-style button with rounded edges.
    /// </summary>
    [Parameter] public bool Pill { get; set; }

    /// <summary>
    /// Draws the button with a caret, indicating that it triggers a dropdown menu or similar behavior.
    /// </summary>
    [Parameter] public bool WithCaret { get; set; }

    /// <summary>
    /// Draws the button in a loading state.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// Disables the button. Does not apply to link buttons.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// The type of button. The default is <see cref="WaButtonType.Button"/> rather than <c>submit</c>, which is the
    /// opposite of how native <c>&lt;button&gt;</c> elements behave.
    /// </summary>
    [Parameter] public WaButtonType? Type { get; set; }

    // Link behavior (when href is provided, renders as <a>)
    /// <summary>
    /// When set, the button is rendered as an <c>&lt;a&gt;</c> with this <c>href</c> instead of a
    /// <c>&lt;button&gt;</c>.
    /// </summary>
    [Parameter] public string? Href { get; set; }

    /// <summary>
    /// Tells the browser where to open the link. Only used when <see cref="Href"/> is present.
    /// </summary>
    [Parameter] public string? Target { get; set; }

    /// <summary>
    /// Tells the browser to download the linked file as this filename. Only used when <see cref="Href"/> is present.
    /// </summary>
    [Parameter] public string? Download { get; set; }

    /// <summary>
    /// When using <see cref="Href"/>, this maps to the underlying link's <c>rel</c> attribute.
    /// </summary>
    [Parameter] public string? Rel { get; set; }

    // Form-submission properties
    /// <summary>
    /// Used to override the form owner's <c>action</c> attribute.
    /// </summary>
    [Parameter] public string? FormAction { get; set; }

    /// <summary>
    /// Used to override the form owner's <c>enctype</c> attribute.
    /// </summary>
    [Parameter] public string? FormEncType { get; set; }

    /// <summary>
    /// Used to override the form owner's <c>method</c> attribute.
    /// </summary>
    [Parameter] public string? FormMethod { get; set; }

    /// <summary>
    /// Used to override the form owner's <c>novalidate</c> attribute.
    /// </summary>
    [Parameter] public bool? FormNoValidate { get; set; }

    /// <summary>
    /// Used to override the form owner's <c>target</c> attribute.
    /// </summary>
    [Parameter] public string? FormTarget { get; set; }

    /// <summary>
    /// The name of the button, submitted as a name/value pair with form data, but only when this button is the
    /// submitter. This attribute is ignored when <see cref="Href"/> is present.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// The value of the button, submitted as a pair with the button's name as part of the form data, but only
    /// when this button is the submitter. This attribute is ignored when <see cref="Href"/> is present.
    /// </summary>
    [Parameter] public string? Value { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the button is clicked.
    /// </summary>
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    /// <summary>
    /// Invoked when the button gains focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnFocus { get; set; }

    /// <summary>
    /// Invoked when the button loses focus.
    /// </summary>
    [Parameter] public EventCallback<FocusEventArgs> OnBlur { get; set; }

    /// <summary>
    /// Invoked when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The button's content (label)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Content to display at the start of the button (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? StartContent { get; set; }

    /// <summary>
    /// Content to display at the end of the button (typically icons)
    /// </summary>
    [Parameter] public RenderFragment? EndContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="StartContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? StartIconName { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="EndContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? EndIconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-button");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNull(4, "variant", Variant?.ToHtmlValue());
        builder.AddAttributeIfNotNull(5, "appearance", Appearance?.ToHtmlValue());
        builder.AddAttributeIfNotNull(6, "size", Size?.ToHtmlValue());
        builder.AddAttribute(7, "pill", Pill);
        builder.AddAttribute(8, "with-caret", WithCaret);
        builder.AddAttribute(9, "loading", Loading);
        builder.AddAttribute(10, "disabled", Disabled);
        builder.AddAttributeIfNotNull(11, "type", Type?.ToHtmlValue());

        // Link behavior attributes
        builder.AddAttributeIfNotNullOrEmpty(12, "href", Href);
        builder.AddAttributeIfNotNullOrEmpty(13, "target", Target);
        builder.AddAttributeIfNotNullOrEmpty(14, "download", Download);
        builder.AddAttributeIfNotNullOrEmpty(15, "rel", Rel);

        // Form-submission attributes
        builder.AddAttributeIfNotNullOrEmpty(17, "formaction", FormAction);
        builder.AddAttributeIfNotNullOrEmpty(18, "formenctype", FormEncType);
        builder.AddAttributeIfNotNullOrEmpty(19, "formmethod", FormMethod);
        builder.AddAttributeIfNotNull(60, "formnovalidate", FormNoValidate);
        builder.AddAttributeIfNotNullOrEmpty(61, "formtarget", FormTarget);
        builder.AddAttributeIfNotNullOrEmpty(62, "name", Name);
        builder.AddAttributeIfNotNullOrEmpty(63, "value", Value);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onclick", OnClick);

        builder.AddAttributeIfHasDelegate(21, "onfocus", OnFocus);

        builder.AddAttributeIfHasDelegate(22, "onblur", OnBlur);

        builder.AddAttributeIfHasDelegate(24, "onwa-invalid", OnInvalid);

        // Add element reference capture
        builder.AddElementReferenceCapture(23, __buttonReference => Element = __buttonReference);

        // Add start slot content
        if (StartContent is not null)
        {
            builder.OpenElement(30, "span");
            builder.AddAttribute(31, "slot", "start");
            builder.AddContent(32, StartContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(35, "start", StartIconName);
        }

        // Add main content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(40, ChildContent);
        }

        // Add end slot content
        if (EndContent is not null)
        {
            builder.OpenElement(50, "span");
            builder.AddAttribute(51, "slot", "end");
            builder.AddContent(52, EndContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(55, "end", EndIconName);
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

    #region ------ Public Methods ------

    /// <summary>
    /// Sets focus on the button.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task FocusAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot focus the button before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "focus");
    }

    /// <summary>
    /// Removes focus from the button.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task BlurAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot blur the button before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "blur");
    }

    /// <summary>
    /// Simulates a click on the button.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="InvalidOperationException">Thrown when the element is not rendered</exception>
    public async Task ClickAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot click the button before the component is rendered. Element reference is null.");

        await JSInterop.InvokeMethodAsync(Element.Value, "click");
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
