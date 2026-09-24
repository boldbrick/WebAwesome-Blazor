namespace WebAwesome.Blazor.Base;

/// <summary>
/// Shared constant values used across the WebAwesome.Blazor library.
/// </summary>
public static class Constants
{
    /// <summary>
    /// Render-tree attribute name of the "numericchange" event, an alias of the browser "change" event registered by
    /// the JS initializer (WebAwesome.Blazor.lib.module.js) for elements whose live value is a JS number (wa-slider,
    /// wa-rating). Blazor's built-in change reader cannot carry a number, so these wrappers bind the alias, whose
    /// payload carries the value as a string; being a custom event type, its
    /// <see cref="Microsoft.AspNetCore.Components.ChangeEventArgs.Value"/> arrives as a JSON element, read it with
    /// <see cref="ChangeEventArgsExtensions.GetStringValue"/>.
    /// </summary>
    internal const string NumericChangeEventAttribute = "onnumericchange";

    /// <summary>
    /// Render-tree attribute name of the "numericinput" event, the "input" counterpart of
    /// <see cref="NumericChangeEventAttribute"/>.
    /// </summary>
    internal const string NumericInputEventAttribute = "onnumericinput";
}
