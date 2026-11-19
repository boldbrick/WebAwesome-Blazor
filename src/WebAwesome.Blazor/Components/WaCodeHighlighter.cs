using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Infrastructure;

namespace WebAwesome.Blazor.Components;
/// <summary>
/// Displays syntax-highlighted code with a copy button.
/// Corresponds to the wa-code-highlighter Web Awesome component (experimental).
/// </summary>
/// <remarks>
/// Provide the code either as the <see cref="Code"/> parameter, which renders verbatim and takes precedence, or as
/// <see cref="ChildContent"/> text. Slotted code is dedented automatically unless <see cref="WithoutDedent"/> is set;
/// Blazor renders text as text, so no escaping of angle brackets is needed, but markup elements inside
/// <see cref="ChildContent"/> are not code and make the element warn. Common languages are bundled and the rest load on
/// demand, see <see cref="Language"/>.
/// </remarks>
public class WaCodeHighlighter : ComponentBase
{
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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Code"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultCode = "";

    /// <summary>
    /// The code to display. It takes precedence over <see cref="ChildContent"/> and renders verbatim, without
    /// dedenting. Use it when the code is dynamic.
    /// </summary>
    [Parameter] public string? Code { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Language"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLanguage = "";

    /// <summary>
    /// The language of the code, e.g. html, css, js or python. Common languages are bundled and the rest load on demand;
    /// applications can register further grammars in JavaScript, so the set is open and the parameter is a string.
    /// Leave it empty to display plain text. A language without a grammar raises <see cref="OnError"/> and renders plain text.
    /// </summary>
    [Parameter] public string? Language { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

    /// <summary>
    /// An accessible label for the code region. When omitted, a localized "Code" label is used.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Removes the copy button.
    /// </summary>
    [Parameter] public bool WithoutCopy { get; set; }

    /// <summary>
    /// Preserves slotted code exactly as written instead of dedenting it, like a pre element. A single newline right
    /// after the opening tag is ignored. Has no effect on <see cref="Code"/>, which always renders verbatim.
    /// </summary>
    [Parameter] public bool WithoutDedent { get; set; }

    /// <summary>
    /// Wraps long lines instead of scrolling horizontally.
    /// </summary>
    [Parameter] public bool Wrap { get; set; }

    /// <summary>
    /// The code to display as text, used when <see cref="Code"/> is not set. It is dedented unless
    /// <see cref="WithoutDedent"/> is set.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the grammar for the current language has loaded and the code has been highlighted.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnLoad { get; set; }

    /// <summary>
    /// Emitted when no grammar exists for the current language. The code renders as plain text.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-code-highlighter");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add highlighter attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 10, "code", Code, DefaultCode);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 11, "language", Language, DefaultLanguage);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 12, "label", Label, DefaultLabel);
        builder.AddAttribute(13, "without-copy", WithoutCopy);
        builder.AddAttribute(14, "without-dedent", WithoutDedent);
        builder.AddAttribute(15, "wrap", Wrap);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(20, "onwa-load", OnLoad);
        builder.AddAttributeIfHasDelegate(21, "onwa-error", OnError);

        // Add element reference capture
        builder.AddElementReferenceCapture(30, __highlighterReference => Element = __highlighterReference);

        // Add default slot content
        builder.AddContent(40, ChildContent);

        builder.CloseElement();
    }

    #endregion

    #region ------ Internals ------

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