using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

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
    /// Converts an attribute value to its string form with the invariant culture.
    /// </summary>
    /// <typeparam name="T">Type of the value</typeparam>
    /// <param name="value">The value</param>
    /// <returns>The invariant-culture string form</returns>
    public static string? FormatInvariant<T>(T value)
    {
        return value is IFormattable formattable
            ? formattable.ToString(null, CultureInfo.InvariantCulture)
            : value?.ToString();
    }

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

    #region ------ Internals ------

    private const string BooleanToStringMessage =
        "A bool would be emitted as \"True\"/\"False\"; use AddBooleanAttribute or AddTrueFalseAttribute";

    #endregion
}