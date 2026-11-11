using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Models;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Infrastructure;

/// <summary>
/// Pushes the JavaScript properties of one wrapper's element that are no attributes: its function-valued members
/// (<see cref="WaJsFunction"/> parameters) and its shadow styles. A push happens only when the value changed since the
/// last push, because each push makes the element render again; an unset value that was never pushed sends nothing.
/// Call it from <c>OnAfterRenderAsync</c>, when the element exists (docs\technical\technical.md, "JavaScript function references").
/// </summary>
internal sealed class WaJsPropertyPush
{
    /// <summary>
    /// Pushes a function-valued member of the element when its descriptor changed since the last push: the
    /// descriptor JSON, or null once a pushed function was removed.
    /// </summary>
    /// <param name="interop">The interop service of the wrapper</param>
    /// <param name="element">The element, or null before the first render (nothing is pushed then)</param>
    /// <param name="member">The JavaScript member, for example "getTag"</param>
    /// <param name="function">The function reference, or null for the element's own value</param>
    /// <returns>A task that completes when the push was sent</returns>
    public async Task PushFunctionAsync(WebAwesomeJSInterop interop, ElementReference? element, string member, WaJsFunction? function)
    {
        if (element is null) return;

        var descriptor = function?.ToString();
        pushedFunctions.TryGetValue(member, out var pushed);
        if (descriptor == pushed) return;

        // recorded before the call, so a render completing meanwhile does not push the same descriptor again
        pushedFunctions[member] = descriptor;
        await interop.SetElementFunctionAsync(element.Value, member, descriptor);
    }

    /// <summary>
    /// Pushes the shadow styles of the element when their text changed since the last push: the CSS text, or null once
    /// pushed styles were removed. An empty text counts as no styles.
    /// </summary>
    /// <param name="interop">The interop service of the wrapper</param>
    /// <param name="element">The element, or null before the first render (nothing is pushed then)</param>
    /// <param name="cssText">The CSS text, or null for no styles</param>
    /// <returns>A task that completes when the push was sent</returns>
    public async Task PushShadowStylesAsync(WebAwesomeJSInterop interop, ElementReference? element, string? cssText)
    {
        if (element is null) return;

        var css = string.IsNullOrEmpty(cssText) ? null : cssText;
        if (css == pushedShadowStyles) return;

        pushedShadowStyles = css;
        await interop.SetShadowStylesAsync(element.Value, css);
    }

    #region ------ Internals ------

    // member -> the descriptor JSON of its last push (null after a removal); a member never pushed has no entry
    private readonly Dictionary<string, string?> pushedFunctions = new();

    // the CSS text of the last push, null while none or after a removal
    private string? pushedShadowStyles;

    #endregion
}
