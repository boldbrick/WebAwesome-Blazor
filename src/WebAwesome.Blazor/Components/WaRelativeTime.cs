using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A utility component that outputs localized time phrases relative to the current date and time.
/// Corresponds to the wa-relative-time Web Awesome component.
/// </summary>
/// <remarks>
/// Uses the browser's Intl.RelativeTimeFormat API for localization. No language packs required.
/// Supports automatic updating when sync is enabled.
/// </remarks>
public class WaRelativeTime : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to call methods on the underlying Web Awesome element.
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

    // Common styling parameters
    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Relative time properties
    /// <summary>
    /// The instant from which to calculate elapsed time. Rendered with its offset (ISO 8601,
    /// <c>2026-01-02T03:04:05.678+01:00</c>), so the browser reads the same instant in every time zone. When null, the
    /// attribute is omitted and Web Awesome uses the current time.
    /// </summary>
    /// <remarks>
    /// A <see cref="DateTime"/> converts implicitly: a UTC one (<see cref="DateTimeKind.Utc"/>) keeps its instant, but
    /// an unspecified or local one takes the offset of the server's time zone. Convert a UTC value read without its kind
    /// (e.g. from a database column) with <c>DateTime.SpecifyKind(value, DateTimeKind.Utc)</c> first.
    /// </remarks>
    [Parameter] public DateTimeOffset? Date { get; set; }

    /// <summary>
    /// Keeps the displayed value up to date as time passes.
    /// </summary>
    [Parameter] public bool Sync { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Format"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaRelativeTimeFormat DefaultFormat = WaRelativeTimeFormat.Long;

    /// <summary>
    /// The formatting style to use, e.g. "3 hours ago" (long), "3 hr. ago" (short) or "3h ago" (narrow).
    /// When null, no attribute is emitted and Web Awesome uses its default, long.
    /// </summary>
    [Parameter] public WaRelativeTimeFormat? Format { get; set; }

    /// <summary>
    /// The locale used to format the relative time phrase, e.g. "en-US".
    /// </summary>
    [Parameter] public string? Lang { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Numeric"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaRelativeTimeNumeric DefaultNumeric = WaRelativeTimeNumeric.Auto;

    /// <summary>
    /// Controls whether idiomatic phrases such as "yesterday" and "tomorrow" are used (auto) or numeric
    /// phrases such as "1 day ago" are always used (always).
    /// When null, no attribute is emitted and Web Awesome uses its default, auto.
    /// </summary>
    [Parameter] public WaRelativeTimeNumeric? Numeric { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-relative-time");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add relative time attributes
        builder.AddDateTimeOffsetAttribute(10, "date", Date);
        builder.AddAttribute(11, "sync", Sync);
        builder.AddAttributeIfNotNull(attributes, 12, "format", Format?.ToHtmlValue(), DefaultFormat.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(13, "lang", Lang);
        builder.AddAttributeIfNotNull(attributes, 14, "numeric", Numeric?.ToHtmlValue(), DefaultNumeric.ToHtmlValue());

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __relativeTimeReference => Element = __relativeTimeReference);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (Element != null)
        {
            await JSInterop.InvokeMethodAsync(Element.Value, "update");
        }

        await base.OnParametersSetAsync();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Forces an update of the relative time display
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task UpdateAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot update relative time: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "update");
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
