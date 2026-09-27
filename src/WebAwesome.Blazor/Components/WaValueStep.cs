using System;
using System.Globalization;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// The granularity a value must adhere to (the step attribute of <see cref="WaInput"/>, <see cref="WaNumberInput"/>
/// and <see cref="WaTimeInput"/>): a positive number, or <see cref="Any"/>, which disables step matching. The CEM
/// types the attribute <c>number | 'any'</c>.
/// </summary>
/// <remarks>
/// <para>
/// A number converts implicitly, so Razor keeps the natural form: <c>Step="5"</c>, <c>Step="0.5"</c>, and
/// <c>Step="WaValueStep.Any"</c> for "any". A number must be positive and finite (the HTML step rule); another value
/// throws <see cref="ArgumentOutOfRangeException"/> when converted.
/// </para>
/// <para>
/// <see cref="ToString"/> is the wire form, culture-free: <c>any</c>, or the number in the invariant culture
/// (<c>0.5</c>, never <c>0,5</c>). Equality is structural; <c>default</c> is <see cref="Any"/>.
/// </para>
/// </remarks>
public readonly record struct WaValueStep
{
    /// <summary>
    /// No step: any value is allowed (<c>step="any"</c>).
    /// </summary>
    public static WaValueStep Any => default;

    /// <summary>
    /// The step as a number, or null for <see cref="Any"/>.
    /// </summary>
    public decimal? Number { get; }

    /// <summary>
    /// Whether this is <see cref="Any"/>.
    /// </summary>
    public bool IsAny => Number is null;

    /// <summary>
    /// Returns the wire form: <c>any</c>, or the number formatted with the invariant culture.
    /// </summary>
    /// <returns>The wire form</returns>
    public override string ToString() => Number?.ToString(CultureInfo.InvariantCulture) ?? AnyValue;

    /// <summary>
    /// Converts an integral step.
    /// </summary>
    /// <param name="step">A positive step</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step is not positive</exception>
    public static implicit operator WaValueStep(int step) => new(step);

    /// <summary>
    /// Converts an integral step.
    /// </summary>
    /// <param name="step">A positive step</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step is not positive</exception>
    public static implicit operator WaValueStep(long step) => new(step);

    /// <summary>
    /// Converts a fractional step, e.g. the Razor literal <c>0.5</c>.
    /// </summary>
    /// <param name="step">A positive, finite step</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step is not positive or not finite</exception>
    public static implicit operator WaValueStep(double step)
    {
        if (!double.IsFinite(step) || Math.Abs(step) > (double)decimal.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(step), step, NotPositiveMessage);
        return new WaValueStep((decimal)step);
    }

    /// <summary>
    /// Converts a decimal step.
    /// </summary>
    /// <param name="step">A positive step</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step is not positive</exception>
    public static implicit operator WaValueStep(decimal step) => new(step);

    #region ------ Constructors ------

    /// <summary>
    /// Creates a numeric step.
    /// </summary>
    /// <param name="number">A positive step</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the step is not positive</exception>
    public WaValueStep(decimal number)
    {
        if (number <= 0) throw new ArgumentOutOfRangeException(nameof(number), number, NotPositiveMessage);
        Number = number;
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Whether wa-time-input shows (and emits) seconds for this step, as the element's withSecondsForStep reads the
    /// step stepFromAttribute converts (3.12.0, time-input.ts): "any", or a step under a minute or not a whole number
    /// of minutes. The default step (no attribute, 60 seconds) hides them.
    /// </summary>
    internal bool ShowsTimeSeconds => Number is not { } seconds || seconds < SecondsPerMinute || seconds % SecondsPerMinute != 0;

    private const string AnyValue = "any";
    private const decimal SecondsPerMinute = 60;
    private const string NotPositiveMessage = "A step must be a positive, finite number; use WaValueStep.Any for no step.";

    #endregion
}
