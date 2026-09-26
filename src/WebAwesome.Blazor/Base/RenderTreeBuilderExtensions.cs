using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Helper extensions for <see cref="RenderTreeBuilder"/> that conditionally emit attributes,
/// reducing repetitive null/empty checks in generated component render trees.
/// </summary>
internal static class RenderTreeBuilderExtensions
{
    /// <summary>
    /// Adds a string attribute to the render tree only when the value is not null or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfNotNullOrEmpty(this RenderTreeBuilder builder, int sequence, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            builder.AddAttribute(sequence, name, value);
        }
    }

    /// <summary>
    /// Adds an attribute to the render tree only when the value is not null, converted to a string with the
    /// invariant culture (a number renders as "0.5", never as the current culture's "0,5", which Web Awesome
    /// cannot parse).
    /// </summary>
    /// <typeparam name="T">Type of the value to convert and emit</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfNotNull<T>(this RenderTreeBuilder builder, int sequence, string name, T? value)
    {
        if (value != null)
        {
            builder.AddAttribute(sequence, name, FormatInvariant(value));
        }
    }

    /// <summary>
    /// Adds a number attribute formatted with the invariant culture. Passing a number straight to
    /// <see cref="RenderTreeBuilder.AddAttribute(int, string, object?)"/> formats it with the current culture
    /// (e.g. "0,5" or a U+2212 minus sign), which Web Awesome cannot parse; use this or
    /// <see cref="AddAttributeIfNotNull{T}(RenderTreeBuilder, int, string, T)"/> for every number.
    /// </summary>
    /// <typeparam name="T">Number type</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddNumberAttribute<T>(this RenderTreeBuilder builder, int sequence, string name, T value)
        where T : struct, IFormattable
    {
        builder.AddAttribute(sequence, name, value.ToString(null, CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Converts an attribute value to its string form with the invariant culture. Refuses the date and time types
    /// (<see cref="DateOnly"/>, <see cref="TimeOnly"/>, <see cref="DateTime"/>, <see cref="DateTimeOffset"/>): their
    /// invariant default forms (<c>01/02/2026</c>, <c>13:04</c>) are not what Web Awesome parses, so each needs its
    /// explicit wire-format helper (<see cref="AddDateAttribute(RenderTreeBuilder, int, string, DateOnly?)"/>, <see cref="AddTimeAttribute(RenderTreeBuilder, int, string, TimeOnly?)"/>,
    /// <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, int, string, DateTimeOffset?)"/>). The compile-time guards below catch the direct calls; this catches a
    /// value reaching the generic path in another way (boxed, or through generic code).
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The invariant-culture string form</returns>
    /// <exception cref="NotSupportedException">Thrown for a date or time value</exception>
    public static string? FormatInvariant<T>(T value)
    {
        if (value is DateOnly or TimeOnly or DateTime or DateTimeOffset)
            throw new NotSupportedException(TemporalToStringMessage);

        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value?.ToString();
    }

    /// <summary>
    /// Blocks dates from the generic invariant-culture path, which would emit <c>01/02/2026</c>; use
    /// <see cref="AddDateAttribute(RenderTreeBuilder, int, string, DateOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateOnly? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks times from the generic invariant-culture path, which would drop the seconds (<c>13:04</c>); use
    /// <see cref="AddTimeAttribute(RenderTreeBuilder, int, string, TimeOnly?)"/> or the wrapper's own value formatting.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, TimeOnly? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks <see cref="DateTime"/> values from the generic invariant-culture path, which would emit
    /// <c>01/02/2026 03:04:05</c> without an offset; use <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, int, string, DateTimeOffset?)"/> for an instant or
    /// <see cref="AddDateAttribute(RenderTreeBuilder, int, string, DateOnly?)"/> for a date.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateTime? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks instants from the generic invariant-culture path, which would emit <c>01/02/2026 03:04:05 +01:00</c>;
    /// use <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, int, string, DateTimeOffset?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateTimeOffset? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks non-nullable dates from the generic invariant-culture path; see the nullable overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateOnly value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks non-nullable times from the generic invariant-culture path; see the nullable overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, TimeOnly value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks non-nullable DateTime values from the generic invariant-culture path; see the nullable overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateTime value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks non-nullable instants from the generic invariant-culture path; see the nullable overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, DateTimeOffset value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks dates from <see cref="AddNumberAttribute{T}(RenderTreeBuilder, int, string, T)"/>, whose <see cref="IFormattable"/> constraint admits them;
    /// use <see cref="AddDateAttribute(RenderTreeBuilder, int, string, DateOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddNumberAttribute(this RenderTreeBuilder builder, int sequence, string name, DateOnly value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks times from <see cref="AddNumberAttribute{T}(RenderTreeBuilder, int, string, T)"/>; use <see cref="AddTimeAttribute(RenderTreeBuilder, int, string, TimeOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddNumberAttribute(this RenderTreeBuilder builder, int sequence, string name, TimeOnly value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks <see cref="DateTime"/> values from <see cref="AddNumberAttribute{T}(RenderTreeBuilder, int, string, T)"/>; use
    /// <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, int, string, DateTimeOffset?)"/> or <see cref="AddDateAttribute(RenderTreeBuilder, int, string, DateOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddNumberAttribute(this RenderTreeBuilder builder, int sequence, string name, DateTime value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks instants from <see cref="AddNumberAttribute{T}(RenderTreeBuilder, int, string, T)"/>; use <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, int, string, DateTimeOffset?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddNumberAttribute(this RenderTreeBuilder builder, int sequence, string name, DateTimeOffset value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks boolean values from the <see cref="object.ToString"/> path: it would emit "True"/"False", which no
    /// Web Awesome attribute reads correctly. Use <see cref="AddBooleanAttribute(RenderTreeBuilder, int, string, bool?)"/> for a plain boolean attribute
    /// or <see cref="AddTrueFalseAttribute(RenderTreeBuilder, int, string, bool?)"/> for an attribute whose converter reads "true"/"false".
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(BooleanToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, bool? value)
    {
        throw new NotSupportedException(BooleanToStringMessage);
    }

    /// <summary>
    /// Blocks boolean values from the <see cref="object.ToString"/> path; see the nullable overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(BooleanToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, int sequence, string name, bool value)
    {
        throw new NotSupportedException(BooleanToStringMessage);
    }

    /// <summary>
    /// Adds a plain boolean attribute (Lit <c>type: Boolean</c>, where any present attribute means true):
    /// emitted without a value when true, omitted when false or null.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; only true emits the attribute</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddBooleanAttribute(this RenderTreeBuilder builder, int sequence, string name, bool? value)
    {
        // Blazor renders a bool attribute as present (true) or absent (false)
        builder.AddAttribute(sequence, name, value == true);
    }

    /// <summary>
    /// Adds an attribute whose Web Awesome converter reads the literal strings "true" and "false" (e.g.
    /// spellcheck): emits exactly "true" or "false" in lowercase, and nothing when null so the component
    /// keeps its own default. A present but empty attribute would read as false for such converters, so
    /// the Blazor boolean rendering must not be used for them.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddTrueFalseAttribute(this RenderTreeBuilder builder, int sequence, string name, bool? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, value.Value ? Constants.TrueAttributeValue : Constants.FalseAttributeValue);
        }
    }

    /// <summary>
    /// Adds an attribute whose Web Awesome converter reads "on" and "off" (<c>autocorrect</c>): emits exactly "on"
    /// or "off", and nothing when null.
    /// </summary>
    /// <remarks>
    /// The element's converter is <c>!value || value === "off" ? false : true</c>, so a present but empty attribute
    /// (Blazor's rendering of true) would read as false; only "on" reads as true.
    /// </remarks>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddOnOffAttribute(this RenderTreeBuilder builder, int sequence, string name, bool? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, value.Value ? Constants.OnAttributeValue : Constants.OffAttributeValue);
        }
    }

    /// <summary>
    /// Adds a step attribute in its wire form (<see cref="WaStep.ToString"/>: "any", or the number in the invariant
    /// culture); nothing when null.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddStepAttribute(this RenderTreeBuilder builder, int sequence, string name, WaStep? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, value.Value.ToString());
        }
    }

    /// <summary>
    /// Adds a date attribute as ISO <c>yyyy-MM-dd</c> (<see cref="WaWireFormat.FormatDate"/>), the only date form
    /// Web Awesome parses; nothing when null.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    public static void AddDateAttribute(this RenderTreeBuilder builder, int sequence, string name, DateOnly? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatDate(value.Value));
        }
    }

    /// <summary>
    /// Adds a time bound attribute (a min or max) as 24-hour <c>HH:mm</c>, or <c>HH:mm:ss</c> when the time has
    /// seconds (<see cref="WaWireFormat.FormatTimeBound"/>); nothing when null.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    public static void AddTimeAttribute(this RenderTreeBuilder builder, int sequence, string name, TimeOnly? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatTimeBound(value.Value));
        }
    }

    /// <summary>
    /// Adds a date-list attribute as ascending ISO dates separated by a space (<see cref="WaWireFormat.FormatDates"/>);
    /// nothing when the set is null or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    public static void AddDateSetAttribute(this RenderTreeBuilder builder, int sequence, string name, IReadOnlySet<DateOnly>? value)
    {
        if (value is { Count: > 0 })
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatDates(value));
        }
    }

    /// <summary>
    /// Adds a weekday-list attribute as the lower-case three-letter tokens Web Awesome reads (<c>sun</c> … <c>sat</c>),
    /// Sunday first, separated by a space (<see cref="WaWireFormat.FormatDaysOfWeek"/>); nothing when the set is null
    /// or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    public static void AddDaysOfWeekAttribute(this RenderTreeBuilder builder, int sequence, string name, IReadOnlySet<DayOfWeek>? value)
    {
        if (value is { Count: > 0 })
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatDaysOfWeek(value));
        }
    }

    /// <summary>
    /// Adds a number-list attribute: each number in the invariant round-trip form, in list order, separated by a
    /// space (<see cref="WaWireFormat.FormatNumbers"/>); nothing when the list is null or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a number that is not finite</exception>
    public static void AddNumberListAttribute(this RenderTreeBuilder builder, int sequence, string name, IReadOnlyList<double>? value)
    {
        if (value is { Count: > 0 })
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatNumbers(value, WaWireFormat.SpaceSeparator));
        }
    }

    /// <summary>
    /// Adds a token-list attribute: the tokens in list order, joined by the separator the element splits on
    /// (<see cref="WaWireFormat.FormatTokens"/>); nothing when the list is null or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    /// <param name="separator">The separator the element splits the attribute on</param>
    /// <param name="forbidden">The characters the element also splits on, which a token must not contain</param>
    /// <exception cref="ArgumentException">Thrown for an empty token or one containing a forbidden character</exception>
    public static void AddTokenListAttribute(this RenderTreeBuilder builder, int sequence, string name, IReadOnlyCollection<string>? value,
        string separator, char[] forbidden)
    {
        if (value is { Count: > 0 })
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatTokens(value, separator, forbidden));
        }
    }

    /// <summary>
    /// Adds a token-set attribute: the tokens in ordinal order (so an equal set always renders the same text),
    /// separated by a space; nothing when the set is null or empty.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null or empty</param>
    /// <exception cref="ArgumentException">Thrown for an empty token or one containing whitespace</exception>
    public static void AddTokenSetAttribute(this RenderTreeBuilder builder, int sequence, string name, IReadOnlySet<string>? value)
    {
        if (value is { Count: > 0 })
        {
            builder.AddAttribute(sequence, name,
                WaWireFormat.FormatTokens(value.Order(StringComparer.Ordinal), WaWireFormat.SpaceSeparator, WaWireFormat.WhitespaceSeparators));
        }
    }

    /// <summary>
    /// Adds an instant attribute in the ECMAScript date-time string format with its offset,
    /// <c>yyyy-MM-ddTHH:mm:ss.fff+01:00</c> (<see cref="WaWireFormat.FormatInstant"/>), which <c>new Date(text)</c>
    /// reads as that instant in every browser time zone; nothing when null.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; nothing is emitted when null</param>
    public static void AddDateTimeOffsetAttribute(this RenderTreeBuilder builder, int sequence, string name, DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            builder.AddAttribute(sequence, name, WaWireFormat.FormatInstant(value.Value));
        }
    }

    /// <summary>
    /// Adds an event handler attribute to the render tree only when the callback has a delegate attached.
    /// </summary>
    /// <typeparam name="T">Type of the event arguments</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Event attribute name</param>
    /// <param name="callback">Event callback; nothing is emitted when no delegate is attached</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfHasDelegate<T>(this RenderTreeBuilder builder, int sequence, string name, EventCallback<T> callback)
    {
        if (callback.HasDelegate)
        {
            builder.AddAttribute(sequence, name, callback);
        }
    }

    /// <summary>
    /// Adds an event handler attribute to the render tree only when the callback has a delegate attached.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Event attribute name</param>
    /// <param name="callback">Event callback; nothing is emitted when no delegate is attached</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfHasDelegate(this RenderTreeBuilder builder, int sequence, string name, EventCallback callback)
    {
        if (callback.HasDelegate)
        {
            builder.AddAttribute(sequence, name, callback);
        }
    }

    /// <summary>
    /// Adds the handler of an event the JS initializer relays (a Constants.Relayed*EventAttribute) together with
    /// Blazor's stopPropagation for it, only when the callback has a delegate attached: the relayed event bubbles,
    /// so without the stopPropagation it would also reach a wrapper of the same element further up the tree.
    /// Uses sequence + 0..1.
    /// </summary>
    /// <typeparam name="T">Type of the event arguments</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Constant base sequence number</param>
    /// <param name="name">Relayed event attribute name, e.g. <see cref="Constants.RelayedShowEventAttribute"/></param>
    /// <param name="callback">Event callback; nothing is emitted when no delegate is attached</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddRelayedEventIfHasDelegate<T>(this RenderTreeBuilder builder, int sequence, string name, EventCallback<T> callback)
    {
        if (callback.HasDelegate)
        {
            builder.AddAttribute(sequence, name, callback);
            builder.AddEventStopPropagationAttribute(sequence + 1, name, true);
        }
    }

    /// <summary>
    /// Renders a wa-icon element into a named slot when an icon name is provided.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Constant base sequence number; uses sequence + 0..3</param>
    /// <param name="slotName">Target slot name, or null for the default slot</param>
    /// <param name="iconName">Icon name; nothing is rendered when null or empty</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddIconSlot(this RenderTreeBuilder builder, int sequence, string? slotName, string? iconName)
    {
        if (string.IsNullOrEmpty(iconName)) return;

        builder.OpenElement(sequence, "wa-icon");
        builder.AddAttribute(sequence + 1, "name", iconName);
        if (!string.IsNullOrEmpty(slotName))
        {
            builder.AddAttribute(sequence + 2, "slot", slotName);
        }
        builder.CloseElement();
    }

    /// <summary>
    /// Renders a fragment into a named slot, wrapped in a span carrying the slot attribute, when the fragment is set.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Constant base sequence number; uses sequence + 0..2</param>
    /// <param name="slotName">Target slot name</param>
    /// <param name="content">Slot content; nothing is rendered when null</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddSlotContent(this RenderTreeBuilder builder, int sequence, string slotName, RenderFragment? content)
    {
        if (content is null) return;

        builder.OpenElement(sequence + 0, SlotWrapperElement);
        builder.AddAttribute(sequence + 1, SlotAttribute, slotName);
        builder.AddContent(sequence + 2, content);
        builder.CloseElement();
    }

    #region ------ Sticky attributes of the Web Awesome element ------

    // The overloads taking a WaAttributeMemory render an attribute of the wrapper's Web Awesome element under the
    // sticky rule (see WaAttributeMemory): an unset parameter renders nothing, and once rendered, the attribute is
    // never removed again, but falls back to the element default. The nullable-value overloads treat null as unset;
    // AddNumberAttribute and AddDefaultedAttribute, for non-nullable parameters, treat the element default as unset
    // until the attribute has been rendered. Plain boolean attributes keep Blazor's present/absent rendering.

    /// <summary>
    /// Opens the wrapper's Web Awesome element and returns the component's attribute memory for its attributes.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="component">The wrapper component rendering the element</param>
    /// <param name="sequence">Sequence number of the element frame</param>
    /// <param name="tag">The element tag, e.g. "wa-slider"</param>
    /// <returns>The attribute memory to pass to the sticky attribute helpers</returns>
    public static WaAttributeMemory OpenWaElement(this RenderTreeBuilder builder, object component, int sequence, string tag)
    {
        var memory = WaAttributeMemory.Of(component);
        memory.Open(tag);
        builder.OpenElement(sequence, tag);
        return memory;
    }

    /// <summary>
    /// Adds a sticky attribute of a nullable parameter, converted with the invariant culture; see the non-sticky overload.
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddAttributeIfNotNull<T>(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, T? value)
        => memory.Render(builder, sequence, name, value == null ? null : FormatInvariant(value), omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky string attribute; null or empty is unset.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    public static void AddAttributeIfNotNullOrEmpty(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, string? value)
        => memory.Render(builder, sequence, name, string.IsNullOrEmpty(value) ? null : value, omitWhileDefault: false);

    /// <summary>
    /// Adds the sticky attribute of a non-nullable number parameter, formatted with the invariant culture: nothing
    /// while it holds the element default and was never rendered.
    /// </summary>
    /// <typeparam name="T">Number type</typeparam>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    public static void AddNumberAttribute<T>(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, T value)
        where T : struct, IFormattable
        => memory.Render(builder, sequence, name, FormatInvariant(value), omitWhileDefault: true);

    /// <summary>
    /// Adds the sticky attribute of a non-nullable parameter in its wire form (an enum's ToHtmlValue()): nothing while
    /// it holds the element default and was never rendered.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    public static void AddDefaultedAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, string value)
        => memory.Render(builder, sequence, name, value, omitWhileDefault: true);

    /// <summary>
    /// Adds a sticky attribute read by a "true"/"false" converter; see the non-sticky overload. Returning to null
    /// renders the element default (spellcheck: "true" on wa-input and wa-textarea), because the converter reads a
    /// removed attribute as false.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddTrueFalseAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, bool? value)
        => memory.Render(builder, sequence, name, value.HasValue ? (value.Value ? Constants.TrueAttributeValue : Constants.FalseAttributeValue) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky attribute read by an "on"/"off" converter; see the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddOnOffAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, bool? value)
        => memory.Render(builder, sequence, name, value.HasValue ? (value.Value ? Constants.OnAttributeValue : Constants.OffAttributeValue) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky step attribute; see the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddStepAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, WaStep? value)
        => memory.Render(builder, sequence, name, value?.ToString(), omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky date attribute (ISO <c>yyyy-MM-dd</c>); see the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddDateAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, DateOnly? value)
        => memory.Render(builder, sequence, name, value.HasValue ? WaWireFormat.FormatDate(value.Value) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky time bound attribute; see the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddTimeAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, TimeOnly? value)
        => memory.Render(builder, sequence, name, value.HasValue ? WaWireFormat.FormatTimeBound(value.Value) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky date-list attribute; null or empty is unset. See the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    public static void AddDateSetAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, IReadOnlySet<DateOnly>? value)
        => memory.Render(builder, sequence, name, value is { Count: > 0 } ? WaWireFormat.FormatDates(value) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky weekday-list attribute; null or empty is unset. See the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    public static void AddDaysOfWeekAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, IReadOnlySet<DayOfWeek>? value)
        => memory.Render(builder, sequence, name, value is { Count: > 0 } ? WaWireFormat.FormatDaysOfWeek(value) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky instant attribute; see the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null is unset</param>
    public static void AddDateTimeOffsetAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, DateTimeOffset? value)
        => memory.Render(builder, sequence, name, value.HasValue ? WaWireFormat.FormatInstant(value.Value) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky number-list attribute; null or empty is unset. See the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown for a number that is not finite</exception>
    public static void AddNumberListAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, IReadOnlyList<double>? value)
        => memory.Render(builder, sequence, name, value is { Count: > 0 } ? WaWireFormat.FormatNumbers(value, WaWireFormat.SpaceSeparator) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky token-list attribute; null or empty is unset. See the non-sticky overload.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    /// <param name="separator">The separator the element splits the attribute on</param>
    /// <param name="forbidden">The characters the element also splits on, which a token must not contain</param>
    /// <exception cref="ArgumentException">Thrown for an empty token or one containing a forbidden character</exception>
    public static void AddTokenListAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name,
        IReadOnlyCollection<string>? value, string separator, char[] forbidden)
        => memory.Render(builder, sequence, name, value is { Count: > 0 } ? WaWireFormat.FormatTokens(value, separator, forbidden) : null, omitWhileDefault: false);

    /// <summary>
    /// Adds a sticky token-set attribute (ordinal order, space-separated); null or empty is unset.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value; null or empty is unset</param>
    /// <exception cref="ArgumentException">Thrown for an empty token or one containing whitespace</exception>
    public static void AddTokenSetAttribute(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, IReadOnlySet<string>? value)
        => memory.Render(builder, sequence, name,
            value is { Count: > 0 } ? WaWireFormat.FormatTokens(value.Order(StringComparer.Ordinal), WaWireFormat.SpaceSeparator, WaWireFormat.WhitespaceSeparators) : null,
            omitWhileDefault: false);

    /// <summary>
    /// Blocks bools from the sticky generic path; see the non-sticky guard.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(BooleanToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, bool? value)
        => throw new NotSupportedException(BooleanToStringMessage);

    /// <summary>
    /// Blocks non-nullable bools from the sticky generic path; see the nullable guard.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(BooleanToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, bool value)
        => throw new NotSupportedException(BooleanToStringMessage);

    /// <summary>
    /// Blocks dates from the sticky generic path; use the sticky <see cref="AddDateAttribute(RenderTreeBuilder, WaAttributeMemory, int, string, DateOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, DateOnly? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks times from the sticky generic path; use the sticky <see cref="AddTimeAttribute(RenderTreeBuilder, WaAttributeMemory, int, string, TimeOnly?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, TimeOnly? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks <see cref="DateTime"/> values from the sticky generic path.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, DateTime? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    /// <summary>
    /// Blocks instants from the sticky generic path; use the sticky <see cref="AddDateTimeOffsetAttribute(RenderTreeBuilder, WaAttributeMemory, int, string, DateTimeOffset?)"/>.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="memory">The component's attribute memory</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">Attribute value</param>
    /// <exception cref="NotSupportedException">Always; the overload exists only to fail the build</exception>
    [Obsolete(TemporalToStringMessage, error: true)]
    public static void AddAttributeIfNotNull(this RenderTreeBuilder builder, WaAttributeMemory memory, int sequence, string name, DateTimeOffset? value)
        => throw new NotSupportedException(TemporalToStringMessage);

    #endregion

    #region ------ Internals ------

    private const string SlotWrapperElement = "span";
    private const string SlotAttribute = "slot";

    private const string BooleanToStringMessage =
        "A bool would be emitted as \"True\"/\"False\"; use AddBooleanAttribute or AddTrueFalseAttribute";

    private const string TemporalToStringMessage =
        "A date or time would be emitted in a form Web Awesome does not parse; use AddDateAttribute, AddTimeAttribute, " +
        "AddDateSetAttribute or AddDateTimeOffsetAttribute (WaWireFormat)";

    #endregion
}