using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

internal static class RenderTreeBuilderExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfNotNullOrEmpty(this RenderTreeBuilder builder, int sequence, string name, string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            builder.AddAttribute(sequence, name, value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void AddAttributeIfNotNull<T>(this RenderTreeBuilder builder, int sequence, string name, T? value)
    {
        if (value != null)
        {
            builder.AddAttribute(sequence, name, value.ToString());
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