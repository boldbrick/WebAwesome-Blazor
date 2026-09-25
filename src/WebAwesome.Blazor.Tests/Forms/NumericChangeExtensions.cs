using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Dispatches the "numericchange" and "numericinput" events the number-valued wrappers (WaSlider, WaRange, WaRating) bind
/// instead of "change". wa-slider and wa-rating report their value as a JS number, which Blazor's built-in
/// change reader rejects, so the library's JS initializer registers "numericchange" as a string-valued
/// alias of the browser change event; bUnit tests drive that alias the same way the browser does.
/// </summary>
internal static class NumericChangeExtensions
{
    /// <summary>
    /// Raises the "numericchange" event on the element with the given string value, as the JS initializer's
    /// alias delivers it.
    /// </summary>
    /// <param name="element">The wa-slider or wa-rating element</param>
    /// <param name="value">The value as the element reports it, e.g. "75" or "20,80" for a range slider</param>
    public static void NumericChange(this IElement element, string value)
        => element.TriggerEvent(NumericChangeEventAttribute, new ChangeEventArgs { Value = value });

    /// <summary>
    /// Raises the "numericinput" event (the input counterpart of "numericchange") on the element with the given
    /// string value, as the JS initializer's alias delivers it.
    /// </summary>
    /// <param name="element">The wa-slider element</param>
    /// <param name="value">The value as the element reports it, e.g. "51" or "21,80" for a range slider</param>
    public static void NumericInput(this IElement element, string value)
        => element.TriggerEvent(NumericInputEventAttribute, new ChangeEventArgs { Value = value });

    // mirrors Constants.NumericChangeEventAttribute in the library (internal there, and InternalsVisibleTo
    // applies to Debug builds only)
    private const string NumericChangeEventAttribute = "onnumericchange";

    // mirrors Constants.NumericInputEventAttribute in the library
    private const string NumericInputEventAttribute = "onnumericinput";
}
