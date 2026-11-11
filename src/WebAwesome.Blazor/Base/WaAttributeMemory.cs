using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Remembers, per component instance, which attributes of its custom element the component has rendered, so that an
/// attribute is never removed again once it was rendered (the sticky rule). Get it from
/// <see cref="RenderTreeBuilderExtensions.OpenWaElement"/> or <see cref="Of"/> and pass it to the sticky overloads of
/// <see cref="RenderTreeBuilderExtensions"/>, which take the memory as their second parameter.
/// </summary>
/// <remarks>
/// <para>
/// Why: when Blazor removes an attribute, Lit's attribute converter sets the element's property to null, not back to
/// its default. A number reads null as 0 and a string stays null, so a slider whose max goes from 50 back to the
/// default 100 would get <c>max = null</c>. A wrapper therefore renders nothing for an unset parameter (null, or a
/// non-nullable parameter at the element default) while the attribute was never rendered, so the element's own default
/// applies and the first render stays clean. Once the attribute was rendered, a return to null or to the default
/// renders the element default explicitly.
/// </para>
/// <para>
/// The element default is not looked up: every call site passes it. Declare it on the wrapper as a public constant
/// <c>Default&lt;Name&gt;</c> (for example <c>DefaultVariant</c>), or as a static readonly field where the type cannot
/// be a constant, and pass that constant, so the default is visible in the wrapper's API and in one place.
/// </para>
/// <para>
/// Exemptions: an attribute whose element default is no literal value (null, undefined) uses the plain helper without
/// the memory and without a default, because its removal restores the unset state. A plain boolean attribute keeps
/// Blazor's present/absent rendering, because Lit reads a removed boolean attribute as false, its default. A default
/// that the element computes when it is set up (<c>new Date()</c>) is rendered through
/// <see cref="Render(RenderTreeBuilder, int, string, string?, Func{string})"/>.
/// </para>
/// </remarks>
public sealed class WaAttributeMemory
{
    /// <summary>
    /// The element default that each sticky call site passed, by attribute name, in its wire form. A test can compare
    /// it with the defaults that the element declares.
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

    #region ------ Constructors ------

    // one memory per component, created only through Of
    private WaAttributeMemory()
    {
    }

    #endregion

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
