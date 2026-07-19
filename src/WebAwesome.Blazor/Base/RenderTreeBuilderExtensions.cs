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
    /// Adds an attribute to the render tree, converted via <see cref="object.ToString"/>, only when the value is not null.
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
            builder.AddAttribute(sequence, name, value.ToString());
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
}