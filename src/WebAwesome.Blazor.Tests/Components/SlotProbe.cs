using AngleSharp.Dom;
using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// A marker element to pass as a RenderFragment parameter, and the lookup of the slot it rendered into, so a
/// slot test asserts where the wrapper puts the content in the markup instead of reading the parameter back.
/// </summary>
internal static class SlotProbe
{
    /// <summary>
    /// Local name of the marker element.
    /// </summary>
    public const string Tag = "slot-probe";

    /// <summary>
    /// A fragment rendering one empty marker element.
    /// </summary>
    public static readonly RenderFragment Fragment = builder =>
    {
        builder.OpenElement(0, Tag);
        builder.CloseElement();
    };

    /// <summary>
    /// Returns the slot the marker rendered into below a root element: the slot attribute of its nearest
    /// ancestor below the root, an empty string for the root's default slot, or null when the marker is not
    /// rendered inside the root.
    /// </summary>
    /// <param name="root">The wrapper's root element</param>
    /// <returns>The slot name, empty for the default slot, or null</returns>
    public static string? SlotOf(IElement root)
    {
        var probe = root.QuerySelector(Tag);
        if (probe == null) return null;

        for (var current = probe.ParentElement; current != null && current != root; current = current.ParentElement)
        {
            if (current.HasAttribute(SlotAttribute)) return current.GetAttribute(SlotAttribute);
        }

        return string.Empty;
    }

    #region ------ Internals ------

    private const string SlotAttribute = "slot";

    #endregion
}
