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

    // Relative time properties
    [Parameter] public DateTime? Date { get; set; }
    [Parameter] public string? DateString { get; set; }
    [Parameter] public bool Sync { get; set; }
    [Parameter] public WaFormat Format { get; set; } = WaFormat.Auto;
    [Parameter] public string? Lang { get; set; }

    // Numeric style for format
    [Parameter] public bool Numeric { get; set; } = true;

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-relative-time");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add relative time attributes
        if (Date.HasValue)
        {
            // Convert DateTime to ISO 8601 string
            builder.AddAttribute(10, "date", Date.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffK"));
        }
        else
        {
            builder.AddAttributeIfNotNullOrEmpty(10, "date", DateString);
        }

        builder.AddAttribute(11, "sync", Sync);
        if (Format != WaFormat.Auto)
            builder.AddAttribute(12, "format", Format.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(13, "lang", Lang);
        if (!Numeric)
            builder.AddAttribute(14, "numeric", Numeric);

        // Add element reference capture
        builder.AddElementReferenceCapture(20, __relativeTimeReference => Element = __relativeTimeReference);

        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // TODO: JS Interop needed
            // Initialize Intl.RelativeTimeFormat and timer management
            // Call: await JSRuntime.InvokeVoidAsync("webAwesome.relativeTime.initialize", Element);

            // The JavaScript should:
            // 1. Parse the date attribute using Date.parse() or new Date()
            // 2. Calculate time difference from current time
            // 3. Use Intl.RelativeTimeFormat with appropriate locale and format options
            // 4. Update display text with formatted relative time
            // 5. If sync=true, set up timer to periodically update the display
            // 6. Handle different format styles (auto, relative, numeric)
            // 7. Support custom lang attribute for localization
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    protected override async Task OnParametersSetAsync()
    {
        // TODO: JS Interop needed
        // Update the component when parameters change
        // Call: await JSRuntime.InvokeVoidAsync("webAwesome.relativeTime.update", Element, GetDateString());

        await base.OnParametersSetAsync();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Forces an update of the relative time display
    /// </summary>
    /// <remarks>
    /// TODO: JS Interop needed - Force recalculation and display update
    /// </remarks>
    public async Task UpdateAsync()
    {
        // TODO: JS Interop needed
        // await JSRuntime.InvokeVoidAsync("webAwesome.relativeTime.forceUpdate", Element);
        await Task.CompletedTask;
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

    /// <summary>
    /// Gets the date as a string for JavaScript interop
    /// </summary>
    private string? GetDateString()
    {
        if (Date.HasValue)
            return Date.Value.ToString("yyyy-MM-ddTHH:mm:ss.fffK");
        return DateString;
    }

    #endregion
}
