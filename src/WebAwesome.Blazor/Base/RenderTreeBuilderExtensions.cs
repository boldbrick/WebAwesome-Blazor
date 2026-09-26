using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

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
    /// <see cref="AddAttributeIfNotNull{T}"/> for every number.
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
    /// explicit wire-format helper (<see cref="AddDateAttribute"/>, <see cref="AddTimeAttribute"/>,
    /// <see cref="AddDateTimeOffsetAttribute"/>). The compile-time guards below catch the direct calls; this catches a
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
    /// <see cref="AddDateAttribute"/>.
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
    /// <see cref="AddTimeAttribute"/> or the wrapper's own value formatting.
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
    /// <c>01/02/2026 03:04:05</c> without an offset; use <see cref="AddDateTimeOffsetAttribute"/> for an instant or
    /// <see cref="AddDateAttribute"/> for a date.
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
    /// use <see cref="AddDateTimeOffsetAttribute"/>.
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
    /// Blocks dates from <see cref="AddNumberAttribute{T}"/>, whose <see cref="IFormattable"/> constraint admits them;
    /// use <see cref="AddDateAttribute"/>.
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
    /// Blocks times from <see cref="AddNumberAttribute{T}"/>; use <see cref="AddTimeAttribute"/>.
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
    /// Blocks <see cref="DateTime"/> values from <see cref="AddNumberAttribute{T}"/>; use
    /// <see cref="AddDateTimeOffsetAttribute"/> or <see cref="AddDateAttribute"/>.
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
    /// Blocks instants from <see cref="AddNumberAttribute{T}"/>; use <see cref="AddDateTimeOffsetAttribute"/>.
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
    /// Web Awesome attribute reads correctly. Use <see cref="AddBooleanAttribute"/> for a plain boolean attribute
    /// or <see cref="AddTrueFalseAttribute"/> for an attribute whose converter reads "true"/"false".
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