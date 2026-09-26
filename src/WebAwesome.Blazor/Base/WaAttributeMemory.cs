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
/// 100 would get <c>max = null</c>. So an unset parameter (null, or a non-nullable parameter at the element default)
/// renders nothing while the attribute was never rendered (the element's own default applies, and the baseline render
/// stays clean), but once the attribute was rendered, a return to the default or to null renders the element default
/// explicitly.
/// The element default is not looked up: every call site passes it, as the wrapper's public
/// <c>Default&lt;Name&gt;</c> constant (docs\technical.md, "Sticky attributes").
/// </para>
/// <para>
/// Exempt: attributes whose CEM default is no literal (null, undefined), where removal restores the unset state and
/// the call site uses the plain helper without a default, and plain boolean attributes, which the wrappers render
/// with Blazor's present/absent booleans (a removed Lit boolean attribute reads false, the default). A computed
/// default (<c>new Date()</c>) is rendered through <see cref="Render(RenderTreeBuilder, int, string, string?, Func{string})"/>.
/// </para>
/// </remarks>
internal sealed class WaAttributeMemory
{
    /// <summary>
    /// The element default each sticky call site passed, by attribute name, in its wire form; the parity tests
    /// compare it with the CEM default of the rendered element.
    /// </summary>
    public IReadOnlyDictionary<string, string> PassedDefaults => passedDefaults;

    /// <summary>
    /// Returns the memory of a component instance, created on first use; it lives as long as the component.
    /// </summary>
    /// <param name="component">The wrapper component</param>
    /// <returns>The component's attribute memory</returns>
    public static WaAttributeMemory Of(object component) => Memories.GetValue(component, _ => new WaAttributeMemory());

    /// <summary>
    /// Renders an attribute under the sticky rule: the value when there is one, the element default in its place when
    /// the attribute was rendered before, and nothing otherwise.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">The wire value, or null when the parameter is unset</param>
    /// <param name="elementDefault">The element default in its wire form, as the call site passes it</param>
    /// <param name="omitWhileDefault">
    /// Whether a value equal to the element default counts as unset until the attribute was rendered (a non-nullable
    /// parameter, whose C# default is the element default); a nullable parameter is unset only while null
    /// </param>
    public void Render(RenderTreeBuilder builder, int sequence, string name, string? value, string elementDefault, bool omitWhileDefault)
    {
        passedDefaults[name] = elementDefault;
        var wasRendered = rendered.Contains(name);

        if (value == null || (omitWhileDefault && !wasRendered && SameValue(value, elementDefault)))
        {
            // unset or at the default: nothing until the attribute was rendered, the default from then on
            if (!wasRendered) return;
            value = elementDefault;
        }

        rendered.Add(name);
        builder.AddAttribute(sequence, name, value);
    }

    /// <summary>
    /// Renders an attribute whose element default is computed when the element is set up (<c>new Date()</c>): the value
    /// when there is one, a fresh stand-in for the computed default (the current instant) when the attribute was
    /// rendered before and the parameter returned to null, and nothing otherwise.
    /// </summary>
    /// <param name="builder">Render tree builder</param>
    /// <param name="sequence">Sequence number for the attribute frame</param>
    /// <param name="name">Attribute name</param>
    /// <param name="value">The wire value, or null when the parameter is unset</param>
    /// <param name="computedDefault">Computes the wire form of the element default, evaluated only when rendered</param>
    public void Render(RenderTreeBuilder builder, int sequence, string name, string? value, Func<string> computedDefault)
    {
        var wasRendered = rendered.Contains(name);

        if (value == null)
        {
            if (!wasRendered) return;
            value = computedDefault();
        }

        rendered.Add(name);
        builder.AddAttribute(sequence, name, value);
    }

    #region ------ Internals ------

    private static readonly ConditionalWeakTable<object, WaAttributeMemory> Memories = new();

    private readonly HashSet<string> rendered = new(StringComparer.Ordinal);

    private readonly Dictionary<string, string> passedDefaults = new(StringComparer.Ordinal);

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
