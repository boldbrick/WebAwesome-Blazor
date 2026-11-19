using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Infrastructure;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;
/// <summary>
/// Counts down to a date or up from one, ticking through years, months, days and smaller units.
/// Corresponds to the wa-counter Web Awesome component (experimental).
/// </summary>
/// <remarks>
/// Use it for launch countdowns, sale timers and showing how long something has been running. Without
/// <see cref="Date"/>, the counter counts down from <see cref="Duration"/>, or up from the moment it starts when
/// <see cref="Direction"/> is up. A custom per-unit value formatter is set with <see cref="ValueFormatterFunction"/>.
/// </remarks>
public class WaCounter : ComponentBase
{
    #region ------ Dependency Injection ------

    /// <summary>
    /// JavaScript interop service used to push the value formatter function to the underlying Web Awesome element.
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
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The date to count down to, or up from, rendered with its offset (ISO 8601, e.g.
    /// <c>2026-01-02T03:04:05.678+01:00</c>) so the browser reads the same instant in every time zone. When null, the
    /// attribute is omitted and the counter counts down from <see cref="Duration"/>, or up from the moment it starts.
    /// </summary>
    /// <remarks>
    /// A <see cref="DateTime"/> converts implicitly: a UTC one keeps its instant, but an unspecified or local one takes
    /// the offset of the server's time zone.
    /// </remarks>
    [Parameter] public DateTimeOffset? Date { get; set; }

    /// <summary>
    /// The number of milliseconds to count down from, starting when the counter is added to the page. Changing it starts
    /// the countdown over. Ignored when <see cref="Date"/> is set or <see cref="Direction"/> is up.
    /// </summary>
    [Parameter] public int? Duration { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Direction"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaCounterDirection DefaultDirection = WaCounterDirection.Down;

    /// <summary>
    /// The direction to count. Down counts down to the date and up counts up from it.
    /// </summary>
    [Parameter] public WaCounterDirection? Direction { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

    /// <summary>
    /// A label that describes the counter to assistive devices.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="LargestUnit"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaCounterUnit DefaultLargestUnit = WaCounterUnit.Days;

    /// <summary>
    /// The largest unit to show. Time beyond it rolls up into it, so two days show as 48 hours when this is hours.
    /// Units at zero ahead of the first non-zero unit are hidden unless <see cref="WithoutLabels"/> is set. The element
    /// falls back to its default range when the smallest unit is larger than the largest.
    /// </summary>
    [Parameter] public WaCounterUnit? LargestUnit { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="SmallestUnit"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaCounterUnit DefaultSmallestUnit = WaCounterUnit.Seconds;

    /// <summary>
    /// The smallest unit to show. Time below it is dropped. The element falls back to its default range when the
    /// smallest unit is larger than the largest.
    /// </summary>
    [Parameter] public WaCounterUnit? SmallestUnit { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Format"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaRelativeTimeFormat DefaultFormat = WaRelativeTimeFormat.Long;

    /// <summary>
    /// The formatting style of the unit labels.
    /// </summary>
    [Parameter] public WaRelativeTimeFormat? Format { get; set; }

    /// <summary>
    /// Hides the unit labels.
    /// </summary>
    [Parameter] public bool WithoutLabels { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Delimiter"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultDelimiter = "";

    /// <summary>
    /// Text to show between units, such as a colon.
    /// </summary>
    [Parameter] public string? Delimiter { get; set; }

    /// <summary>
    /// Pauses the counter. When it's removed, counting continues from the time shown.
    /// </summary>
    [Parameter] public bool Paused { get; set; }

    #endregion

    #region ------ Function Members ------

    /// <summary>
    /// The JavaScript function that formats the value of each unit (the element's <c>valueFormatter</c>). It is called as
    /// <c>(value, unit, ctx)</c>: the value and its unit (years, months, days, hours, minutes or seconds) and <c>ctx</c> with <c>args</c> (the <see cref="WaJsFunction.WithArgs"/> value) and
    /// <c>element</c>; it returns the text. When set, screen readers still read the unformatted value. When null, the element formats with its own options.
    /// </summary>
    [Parameter] public WaJsFunction? ValueFormatterFunction { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// A custom label for years, rendered into the years slot.
    /// </summary>
    [Parameter] public RenderFragment? YearsContent { get; set; }

    /// <summary>
    /// A custom label for months, rendered into the months slot.
    /// </summary>
    [Parameter] public RenderFragment? MonthsContent { get; set; }

    /// <summary>
    /// A custom label for days, rendered into the days slot.
    /// </summary>
    [Parameter] public RenderFragment? DaysContent { get; set; }

    /// <summary>
    /// A custom label for hours, rendered into the hours slot.
    /// </summary>
    [Parameter] public RenderFragment? HoursContent { get; set; }

    /// <summary>
    /// A custom label for minutes, rendered into the minutes slot.
    /// </summary>
    [Parameter] public RenderFragment? MinutesContent { get; set; }

    /// <summary>
    /// A custom label for seconds, rendered into the seconds slot.
    /// </summary>
    [Parameter] public RenderFragment? SecondsContent { get; set; }

    /// <summary>
    /// Content to show in place of the units when a countdown is at zero, including one whose date had already passed,
    /// rendered into the finished slot.
    /// </summary>
    [Parameter] public RenderFragment? FinishedContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when a running countdown reaches zero. Not emitted when the date had already passed.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnFinish { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-counter");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add counter attributes
        builder.AddDateTimeOffsetAttribute(10, "date", Date);
        builder.AddAttributeIfNotNull(11, "duration", Duration);
        builder.AddAttributeIfNotNull(attributes, 12, "direction", Direction?.ToHtmlValue(), DefaultDirection.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 13, "label", Label, DefaultLabel);
        builder.AddAttributeIfNotNull(attributes, 14, "largest-unit", LargestUnit?.ToHtmlValue(), DefaultLargestUnit.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 15, "smallest-unit", SmallestUnit?.ToHtmlValue(), DefaultSmallestUnit.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 16, "format", Format?.ToHtmlValue(), DefaultFormat.ToHtmlValue());
        builder.AddAttribute(17, "without-labels", WithoutLabels);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 18, "delimiter", Delimiter, DefaultDelimiter);
        builder.AddAttribute(19, "paused", Paused);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(30, "onwa-finish", OnFinish);

        // Add element reference capture
        builder.AddElementReferenceCapture(40, __counterReference => Element = __counterReference);

        // Add slot content
        builder.AddSlotContent(50, "years", YearsContent);
        builder.AddSlotContent(55, "months", MonthsContent);
        builder.AddSlotContent(60, "days", DaysContent);
        builder.AddSlotContent(65, "hours", HoursContent);
        builder.AddSlotContent(70, "minutes", MinutesContent);
        builder.AddSlotContent(75, "seconds", SecondsContent);
        builder.AddSlotContent(80, "finished", FinishedContent);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        await jsProperties.PushFunctionAsync(JSInterop, Element, ValueFormatterMember, ValueFormatterFunction);
    }

    #endregion

    #region ------ Internals ------
    // the function-valued member of the element
    private const string ValueFormatterMember = "valueFormatter";

    // the function member last pushed to the element
    private readonly WaJsPropertyPush jsProperties = new();
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