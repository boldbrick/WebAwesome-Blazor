using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Remembers, per component instance, which attributes of its Web Awesome element it has rendered, so that an
/// attribute is never removed again once it was rendered (owner rule, "sticky attributes").
/// </summary>
/// <remarks>
/// <para>
/// When Blazor removes an attribute, Lit's attribute converter sets the element's property to null, not back to its
/// default (a number reads null as 0, a string stays null): a <c>WaSlider</c> whose Max goes from 50 back to the default
/// 100 would get <c>max = null</c>. So an unset parameter renders nothing (the element's own default applies, and the
/// baseline render stays clean), but once the attribute was rendered, a return to the default (a non-nullable
/// parameter) or to null (a nullable one) renders the element's default value explicitly, from
/// <see cref="WaElementDefaults"/>, the CEM defaults of the bound Web Awesome version.
/// </para>
/// <para>
/// Exempt: attributes whose CEM default is no literal (null, undefined, or computed such as <c>new Date()</c>), where
/// removal restores the unset state or no value can stand in for it, and plain boolean attributes, which the wrappers
/// render with Blazor's present/absent booleans (a removed Lit boolean attribute reads false, the default).
/// </para>
/// </remarks>
internal sealed class WaAttributeMemory
{
    /// <summary>
    /// The tag of the element the attributes belong to, set when the component opens its element.
    /// </summary>
    public string Tag => tag ?? throw new InvalidOperationException(
        "The element's attributes are rendered before its tag is known; open the element with OpenWaElement.");

    /// <summary>
    /// Returns the memory of a component instance, created on first use; it lives as long as the component.
    /// </summary>
    /// <param name="component">The wrapper component</param>
    /// <returns>The component's attribute memory</returns>
    public static WaAttributeMemory Of(object component) => Memories.GetValue(component, _ => new WaAttributeMemory());

    /// <summary>
    /// Records the tag of the element the component renders.
    /// </summary>
    /// <param name="elementTag">The element tag, e.g. "wa-slider"</param>
    public void Open(string elementTag) => tag = elementTag;

    /// <summary>
    /// Renders an attribute under the sticky rule: the value when there is one, the element's default in its place
    /// when the attribute was rendered before, and nothing otherwise.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">The wire value, or null when the parameter is unset</param>
    /// <param name="omitWhileDefault">
    /// Whether a value equal to the element default counts as unset until the attribute was rendered (a non-nullable
    /// parameter, whose C# default is the element default)
    /// </param>
    public void Render(RenderTreeBuilder builder, int sequence, string name, string? value, bool omitWhileDefault)
    {
        var hasDefault = WaElementDefaults.TryGet(Tag, name, out var elementDefault);
        var wasRendered = rendered.Contains(name);

        if (value != null && omitWhileDefault && hasDefault && !wasRendered && SameValue(value, elementDefault!)) value = null;
        if (value == null && hasDefault && wasRendered) value = elementDefault;
        if (value == null) return;

        rendered.Add(name);
        builder.AddAttribute(sequence, name, value);
    }

    #region ------ Internals ------

    private static readonly ConditionalWeakTable<object, WaAttributeMemory> Memories = new();

    private readonly HashSet<string> rendered = new(StringComparer.Ordinal);

    private string? tag;

    // whether a wire value is the element default: numerically for numbers ("1" and "1.0"), ordinally otherwise
    private static bool SameValue(string value, string elementDefault)
    {
        if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            && double.TryParse(elementDefault, NumberStyles.Float, CultureInfo.InvariantCulture, out var defaultNumber))
        {
            return number.Equals(defaultNumber);
        }

        return string.Equals(value, elementDefault, StringComparison.Ordinal);
    }

    #endregion
}
