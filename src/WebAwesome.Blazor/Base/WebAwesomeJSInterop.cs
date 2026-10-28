using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Service for Web Awesome JavaScript interop operations
/// </summary>
public class WebAwesomeJSInterop
{
    /// <summary>
    /// Sets a custom validation message on a Web Awesome form control element
    /// </summary>
    /// <param name="elementReference">Reference to the form control element</param>
    /// <param name="message">The validation message to display, or empty string to clear</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task SetCustomValidityAsync(ElementReference elementReference, string message)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setCustomValidity", elementReference, message ?? string.Empty);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set custom validity: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Invokes a method on a Web Awesome element and returns the result. A call before the element is defined (its
    /// module is still loading) waits in the browser for the definition.
    /// </summary>
    /// <typeparam name="T">The expected return type of the method</typeparam>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="args">Arguments to pass to the method</param>
    /// <returns>The result of the method call</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when methodName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method call fails, or when the element is still not
    /// defined after the wait</exception>
    public async Task<T> InvokeMethodAsync<T>(ElementReference elementReference, string methodName, params object[] args)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentNullException(nameof(methodName));

        try
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<T>("invokeMethod", elementReference, methodName, args ?? Array.Empty<object>());
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to invoke method '{methodName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, return default value silently
            return default(T)!;
        }
    }

    /// <summary>
    /// Invokes a void method on a Web Awesome element. A call before the element is defined (its module is still
    /// loading) waits in the browser for the definition.
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="args">Arguments to pass to the method</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when methodName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method call fails, or when the element is still not
    /// defined after the wait</exception>
    public async Task InvokeMethodAsync(ElementReference elementReference, string methodName, params object[] args)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        if (string.IsNullOrEmpty(methodName))
            throw new ArgumentNullException(nameof(methodName));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("invokeMethod", elementReference, methodName, args ?? Array.Empty<object>());
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to invoke method '{methodName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Sets a property value on a Web Awesome element
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="propertyName">Name of the property to set</param>
    /// <param name="value">Value to set</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when propertyName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when setting the property fails</exception>
    public async Task SetPropertyAsync(ElementReference elementReference, string propertyName, object value)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setProperty", elementReference, propertyName, value);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set property '{propertyName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Synchronizes a live property of a Web Awesome element with the given value; the property is assigned only
    /// when its current value differs, so an unchanged value never disturbs the element (e.g. the caret position
    /// while the user is typing)
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="propertyName">Name of the live property to synchronize</param>
    /// <param name="value">Value to assign, typed as the element expects it (string, number, boolean or null)</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when propertyName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when setting the property fails</exception>
    public async Task SyncPropertyAsync(ElementReference elementReference, string propertyName, object? value)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("syncProperty", elementReference, propertyName, value);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to sync property '{propertyName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Gets a property value from a Web Awesome element
    /// </summary>
    /// <typeparam name="T">The expected type of the property value</typeparam>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="propertyName">Name of the property to get</param>
    /// <returns>The property value</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when propertyName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when getting the property fails</exception>
    public async Task<T> GetPropertyAsync<T>(ElementReference elementReference, string propertyName)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        if (string.IsNullOrEmpty(propertyName))
            throw new ArgumentNullException(nameof(propertyName));

        try
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<T>("getProperty", elementReference, propertyName);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to get property '{propertyName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, return default value silently
            return default(T)!;
        }
    }

    /// <summary>
    /// Registers an icon library with Web Awesome (its <c>registerIconLibrary</c>). Registering an existing name
    /// replaces that library, and rendered icons of the library re-resolve.
    /// </summary>
    /// <param name="name">Name of the icon library, referenced by <c>WaIcon.Library</c></param>
    /// <param name="options">Configuration options for the library; exactly one of
    /// <see cref="IconLibraryOptions.Resolver"/> and <see cref="IconLibraryOptions.ResolverFunction"/> is required</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null or empty, or options is null</exception>
    /// <exception cref="ArgumentException">Thrown when the options have no resolver, or both a resolver URL template
    /// and a resolver function, or both the obsolete mutator path and a mutator function</exception>
    /// <exception cref="InvalidOperationException">Thrown when the registration fails, e.g. Web Awesome is not loaded,
    /// the resolver function does not resolve, or the obsolete mutator path does not name a global function</exception>
    public virtual async Task RegisterIconLibraryAsync(string name, IconLibraryOptions options)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        ValidateIconLibraryOptions(name, options);

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("registerIconLibrary", ConfiguredLoaderUrl(), name, options);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to register icon library '{name}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Unregisters an icon library from Web Awesome (its <c>unregisterIconLibrary</c>). Icons already rendered
    /// from the library keep their current image.
    /// </summary>
    /// <param name="name">Name of the icon library to remove</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when the unregistration fails</exception>
    public virtual async Task UnregisterIconLibraryAsync(string name)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("unregisterIconLibrary", ConfiguredLoaderUrl(), name);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to unregister icon library '{name}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Sets the icon family used by icons without an explicit family (Web Awesome's <c>setDefaultIconFamily</c>);
    /// rendered icons re-resolve
    /// </summary>
    /// <param name="family">The icon family name (e.g., "classic", "sharp", "brands")</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when family is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when setting the family fails</exception>
    public virtual async Task SetDefaultIconFamilyAsync(string family)
    {
        if (string.IsNullOrEmpty(family))
            throw new ArgumentNullException(nameof(family));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setDefaultIconFamily", ConfiguredLoaderUrl(), family);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set default icon family to '{family}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Gets the icon family used by icons without an explicit family (Web Awesome's <c>getDefaultIconFamily</c>)
    /// </summary>
    /// <returns>The current default icon family name</returns>
    /// <exception cref="InvalidOperationException">Thrown when getting the family fails</exception>
    public virtual async Task<string> GetDefaultIconFamilyAsync()
    {
        try
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("getDefaultIconFamily", ConfiguredLoaderUrl());
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to get default icon family: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, return default value silently
            return WebAwesomeDefaultIconFamily;
        }
    }

    /// <summary>
    /// Sets the Font Awesome kit code that unlocks the Pro icons of the default icon library (Web Awesome's
    /// <c>setKitCode</c>); rendered icons re-resolve. <see cref="WebAwesomeOptions.FontAwesomeKitCode"/> applies
    /// the same setting at startup.
    /// </summary>
    /// <param name="kitCode">Font Awesome kit code; supply it from configuration, never hard-code it</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when kitCode is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when setting the kit code fails</exception>
    public virtual async Task SetKitCodeAsync(string kitCode)
    {
        if (string.IsNullOrEmpty(kitCode))
            throw new ArgumentNullException(nameof(kitCode));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setKitCode", ConfiguredLoaderUrl(), kitCode);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set the Font Awesome kit code: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Fires the slotchange of an element's default slot, so the element re-reads its light-DOM children through its
    /// own slotchange handler; used to make wa-date-input forward day slots added or removed after its first update
    /// (see <see cref="WaDateInputBase{TValue}"/>)
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the call fails</exception>
    internal async Task SignalDefaultSlotChangeAsync(ElementReference elementReference)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("signalDefaultSlotChange", elementReference);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to signal a default slot change: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Assigns the columns of a wa-data-grid from their JSON. The browser resolves each <see cref="WaJsFunction"/>
    /// descriptor into its function, registers the Web Awesome elements the functions declare, assigns the element's
    /// <c>columns</c> property, and registers the elements the rendered cells use. A reference that does not resolve
    /// is logged and left out. The same JSON again for the same element assigns nothing.
    /// </summary>
    /// <param name="elementReference">Reference to the wa-data-grid element</param>
    /// <param name="columnsJson">The JSON array of the column definitions</param>
    /// <returns>A task that completes when the columns are assigned</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the call fails</exception>
    internal async Task SetDataGridColumnsAsync(ElementReference elementReference, string columnsJson)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setDataGridColumns", elementReference, columnsJson, ConfiguredLoaderUrl());
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set the data grid columns: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Assigns the data of a wa-data-grid. While function members of the grid are still resolving in the browser, the
    /// assignment waits for them, because the grid reads <c>childRows</c> only when it receives data; a newer push
    /// replaces a waiting one. The call returns at once.
    /// </summary>
    /// <param name="elementReference">Reference to the wa-data-grid element</param>
    /// <param name="data">The row objects</param>
    /// <returns>A task that completes when the push was sent</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the call fails</exception>
    internal async Task SetDataGridDataAsync(ElementReference elementReference, IReadOnlyList<object> data)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setDataGridData", elementReference, data);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set the data grid data: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Assigns a function-valued member of a Web Awesome element from the JSON of a <see cref="WaJsFunction"/>
    /// descriptor. The browser waits until the element is defined, resolves the reference, registers the Web Awesome
    /// elements it declares, and assigns the member. Null restores the element's own value of the member. A reference
    /// that does not resolve is logged and leaves the element's own value. The same JSON again assigns nothing.
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="member">The JavaScript member, for example "getTag"</param>
    /// <param name="descriptorJson">The JSON of the descriptor, or null to remove the function</param>
    /// <returns>A task that completes when the member is assigned</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the call fails</exception>
    internal async Task SetElementFunctionAsync(ElementReference elementReference, string member, string? descriptorJson)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setElementFunction", elementReference, member, descriptorJson, ConfiguredLoaderUrl());
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set the function member '{member}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Adopts CSS text into the shadow root of a Web Awesome element as a constructed stylesheet, once the element is
    /// defined and has rendered. A new text replaces the sheet's content; null removes the sheet.
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="cssText">The CSS text, or null to remove the styles</param>
    /// <returns>A task that completes when the styles are adopted</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the call fails</exception>
    internal async Task SetShadowStylesAsync(ElementReference elementReference, string? cssText)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("setShadowStyles", elementReference, cssText);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to set the shadow styles: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Invokes a parameterless method on a Web Awesome element once its custom element is defined, and does nothing
    /// before: an element whose module is still loading has none of its methods yet, and its upgrade renders it from
    /// its current attributes anyway. For refreshes a wrapper requests on every parameter change.
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="InvalidOperationException">Thrown when the defined element's method call fails</exception>
    internal async Task InvokeMethodIfDefinedAsync(ElementReference elementReference, string methodName)
    {
        if (elementReference.Id == null)
            throw new ArgumentException("Element reference is not valid", nameof(elementReference));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeAsync<bool>("invokeMethodIfDefined", elementReference, methodName);
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to invoke method '{methodName}': {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, ignore silently
        }
    }

    /// <summary>
    /// Disposes the JavaScript module reference
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (moduleTask.IsValueCreated)
        {
            var module = await moduleTask.Value;
            await module.DisposeAsync();
        }
    }

    #region ------ Constructors ------

    /// <summary>
    /// Initializes a new instance of the <see cref="WebAwesomeJSInterop"/> service.
    /// </summary>
    /// <param name="jsRuntime">JavaScript runtime used to load and invoke the Web Awesome interop module</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsRuntime"/> is null</exception>
    public WebAwesomeJSInterop(IJSRuntime jsRuntime)
    {
        this.jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
        moduleTask = new Lazy<Task<IJSObjectReference>>(() => LoadModuleAsync());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WebAwesomeJSInterop"/> service with the registered asset options.
    /// The icon library methods reach Web Awesome through the page's own Web Awesome script tag; the loader URL of
    /// <paramref name="options"/> is used only when the page has none.
    /// </summary>
    /// <param name="jsRuntime">JavaScript runtime used to load and invoke the Web Awesome interop module</param>
    /// <param name="options">Web Awesome asset options, as registered by AddWebAwesome</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="jsRuntime"/> or <paramref name="options"/> is null</exception>
    public WebAwesomeJSInterop(IJSRuntime jsRuntime, WebAwesomeOptions options)
        : this(jsRuntime)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

    #endregion

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    // a resolver is either a URL template or a function, and a mutator either the obsolete global path or a function
    private static void ValidateIconLibraryOptions(string name, IconLibraryOptions options)
    {
        var hasTemplate = !string.IsNullOrEmpty(options.Resolver);
        if (!hasTemplate && options.ResolverFunction is null)
            throw new ArgumentException($"Icon library '{name}' requires a resolver: set {nameof(IconLibraryOptions.Resolver)} (a URL template) or {nameof(IconLibraryOptions.ResolverFunction)}.", nameof(options));

        if (hasTemplate && options.ResolverFunction is not null)
            throw new ArgumentException($"Icon library '{name}' has both {nameof(IconLibraryOptions.Resolver)} and {nameof(IconLibraryOptions.ResolverFunction)}; set only one of them.", nameof(options));

#pragma warning disable CS0618 // the obsolete mutator path stays valid until its removal, but not together with its replacement
        if (!string.IsNullOrEmpty(options.Mutator) && options.MutatorFunction is not null)
            throw new ArgumentException($"Icon library '{name}' has both {nameof(IconLibraryOptions.Mutator)} and {nameof(IconLibraryOptions.MutatorFunction)}; set only {nameof(IconLibraryOptions.MutatorFunction)}.", nameof(options));
#pragma warning restore CS0618
    }

    // initial default icon family of Web Awesome, reported while the JS runtime is disconnected
    private const string WebAwesomeDefaultIconFamily = "classic";

    private readonly IJSRuntime jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;
    private readonly WebAwesomeOptions? options;

    private async Task<IJSObjectReference> LoadModuleAsync()
    {
        return await jsRuntime.InvokeAsync<IJSObjectReference>("import", InteropModulePath);
    }

    private string? ConfiguredLoaderUrl() => options?.ResolveLoaderUrl();

    #endregion
}
