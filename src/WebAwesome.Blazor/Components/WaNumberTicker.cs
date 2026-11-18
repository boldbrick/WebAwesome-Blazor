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
/// Counts up or down to a number when it scrolls into view and formats it with the Intl.NumberFormat API.
/// Corresponds to the wa-number-ticker Web Awesome component (experimental).
/// </summary>
/// <remarks>
/// Use it for dashboard stats, landing page metrics and running totals. Once counting has started, a changed
/// <see cref="Value"/> counts from the number currently shown to the new value. A custom value formatter is set with
/// <see cref="ValueFormatterFunction"/>.
/// </remarks>
public class WaNumberTicker : ComponentBase
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

    /// <summary>
    /// Additional CSS class names to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Additional inline CSS styles to apply to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Value"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultValue = 0m;

    /// <summary>
    /// The number to count to. Once the ticker has started, changing it counts from the number currently shown to the
    /// new value.
    /// </summary>
    [Parameter] public decimal Value { get; set; } = DefaultValue;

    /// <summary>
    /// The Web Awesome default of <see cref="StartValue"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const decimal DefaultStartValue = 0m;

    /// <summary>
    /// The number to count from on the first run and when <see cref="RestartAsync"/> is called.
    /// </summary>
    [Parameter] public decimal StartValue { get; set; } = DefaultStartValue;

    /// <summary>
    /// The Web Awesome default of <see cref="Duration"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultDuration = 2000;

    /// <summary>
    /// The longest a count can take, in milliseconds. A count ends early once the number shown stops changing. Set to 0
    /// to skip the animation.
    /// </summary>
    [Parameter] public int Duration { get; set; } = DefaultDuration;

    /// <summary>
    /// The Web Awesome default of <see cref="Delay"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const int DefaultDelay = 0;

    /// <summary>
    /// The number of milliseconds to wait before counting from the start value. Later value changes aren't delayed.
    /// </summary>
    [Parameter] public int Delay { get; set; } = DefaultDelay;

    /// <summary>
    /// The easing function to use for counting. Accepts any CSS easing function, such as <c>linear</c>,
    /// <c>ease-in-out</c>, <c>cubic-bezier()</c>, <c>steps()</c> or <c>linear()</c>; an invalid value falls back to the
    /// element's default ease-out curve, which also applies while this is null. Typed string because the element takes
    /// an open CSS grammar.
    /// </summary>
    [Parameter] public string? Easing { get; set; }

    /// <summary>
    /// Starts counting right away instead of waiting until the ticker scrolls into view.
    /// </summary>
    [Parameter] public bool Eager { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Type"/>, which renders no attribute until the parameter first differs from it.
    /// </summary>
    public const WaFormatNumberType DefaultType = WaFormatNumberType.Decimal;

    /// <summary>
    /// The formatting style to use.
    /// </summary>
    [Parameter] public WaFormatNumberType Type { get; set; } = DefaultType;

    /// <summary>
    /// Turns off grouping separators, such as thousands separators, regardless of locale.
    /// </summary>
    [Parameter] public bool WithoutGrouping { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Currency"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultCurrency = "USD";

    /// <summary>
    /// The <see href="https://en.wikipedia.org/wiki/ISO_4217">ISO 4217</see> currency code to use when formatting.
    /// </summary>
    [Parameter] public string? Currency { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="CurrencyDisplay"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaCurrencyDisplay DefaultCurrencyDisplay = WaCurrencyDisplay.Symbol;

    /// <summary>
    /// How to display the currency.
    /// </summary>
    [Parameter] public WaCurrencyDisplay? CurrencyDisplay { get; set; }

    /// <summary>
    /// The minimum number of integer digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MinimumIntegerDigits { get; set; }

    /// <summary>
    /// The minimum number of fraction digits to use. Possible values are 0-100. If only one fraction digit option is set,
    /// it's used for both.
    /// </summary>
    [Parameter] public int? MinimumFractionDigits { get; set; }

    /// <summary>
    /// The maximum number of fraction digits to use. Possible values are 0-100. If only one fraction digit option is set,
    /// it's used for both.
    /// </summary>
    [Parameter] public int? MaximumFractionDigits { get; set; }

    /// <summary>
    /// The minimum number of significant digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MinimumSignificantDigits { get; set; }

    /// <summary>
    /// The maximum number of significant digits to use. Possible values are 1-21.
    /// </summary>
    [Parameter] public int? MaximumSignificantDigits { get; set; }

    #endregion

    #region ------ Function Members ------

    /// <summary>
    /// The JavaScript function that formats the number shown (the element's <c>valueFormatter</c>). It is called as
    /// <c>(value, ctx)</c>: the number and <c>ctx</c> with <c>args</c> (the <see cref="WaJsFunction.WithArgs"/> value) and
    /// <c>element</c>; it returns the text. When set, the formatting properties (type, currency, digits, grouping) are ignored. When null, the element formats with its own options.
    /// </summary>
    [Parameter] public WaJsFunction? ValueFormatterFunction { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the ticker finishes counting.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnFinish { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-number-ticker");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);

        // Add ticker attributes
        builder.AddNumberAttribute(attributes, 10, "value", Value, DefaultValue);
        builder.AddNumberAttribute(attributes, 11, "start-value", StartValue, DefaultStartValue);
        builder.AddNumberAttribute(attributes, 12, "duration", Duration, DefaultDuration);
        builder.AddNumberAttribute(attributes, 13, "delay", Delay, DefaultDelay);
        builder.AddAttributeIfNotNullOrEmpty(14, "easing", Easing);
        builder.AddAttribute(15, "eager", Eager);
        builder.AddDefaultedAttribute(attributes, 16, "type", Type.ToHtmlValue(), DefaultType.ToHtmlValue());
        builder.AddAttribute(17, "without-grouping", WithoutGrouping);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 18, "currency", Currency, DefaultCurrency);
        builder.AddAttributeIfNotNull(attributes, 19, "currency-display", CurrencyDisplay?.ToHtmlValue(), DefaultCurrencyDisplay.ToHtmlValue());
        builder.AddAttributeIfNotNull(20, "minimum-integer-digits", MinimumIntegerDigits);
        builder.AddAttributeIfNotNull(21, "minimum-fraction-digits", MinimumFractionDigits);
        builder.AddAttributeIfNotNull(22, "maximum-fraction-digits", MaximumFractionDigits);
        builder.AddAttributeIfNotNull(23, "minimum-significant-digits", MinimumSignificantDigits);
        builder.AddAttributeIfNotNull(24, "maximum-significant-digits", MaximumSignificantDigits);

        // Add event handlers
        builder.AddAttributeIfHasDelegate(30, "onwa-finish", OnFinish);

        // Add element reference capture
        builder.AddElementReferenceCapture(40, __tickerReference => Element = __tickerReference);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        await jsProperties.PushFunctionAsync(JSInterop, Element, ValueFormatterMember, ValueFormatterFunction);
    }

    #endregion

    #region ------ Public Methods ------
    /// <summary>
    /// Plays the ticker again from <see cref="StartValue"/> to <see cref="Value"/>.
    /// </summary>
    /// <returns>A task that completes when the restart is requested</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task RestartAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot restart number ticker: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "restart");
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