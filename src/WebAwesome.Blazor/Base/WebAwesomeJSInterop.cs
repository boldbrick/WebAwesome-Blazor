using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// Service for Web Awesome JavaScript interop operations
/// </summary>
public class WebAwesomeJSInterop
{
    private readonly IJSRuntime jsRuntime;
    private readonly Lazy<Task<IJSObjectReference>> moduleTask;

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
    /// Invokes a method on a Web Awesome element and returns the result
    /// </summary>
    /// <typeparam name="T">The expected return type of the method</typeparam>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="args">Arguments to pass to the method</param>
    /// <returns>The result of the method call</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when methodName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method call fails</exception>
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
    /// Invokes a void method on a Web Awesome element
    /// </summary>
    /// <param name="elementReference">Reference to the Web Awesome element</param>
    /// <param name="methodName">Name of the method to invoke</param>
    /// <param name="args">Arguments to pass to the method</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentException">Thrown when element reference is invalid</exception>
    /// <exception cref="ArgumentNullException">Thrown when methodName is null</exception>
    /// <exception cref="InvalidOperationException">Thrown when the method call fails</exception>
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
    /// Registers a custom icon library with Web Awesome
    /// </summary>
    /// <param name="name">Name of the icon library</param>
    /// <param name="options">Configuration options for the library</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    /// <exception cref="ArgumentNullException">Thrown when name is null or empty</exception>
    /// <exception cref="InvalidOperationException">Thrown when the registration fails</exception>
    public virtual async Task RegisterIconLibraryAsync(string name, IconLibraryOptions options)
    {
        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        if (options == null)
            throw new ArgumentNullException(nameof(options));

        try
        {
            var module = await moduleTask.Value;
            await module.InvokeVoidAsync("registerIconLibrary", name, options);
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
    /// Unregisters an icon library from Web Awesome
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
            await module.InvokeVoidAsync("unregisterIconLibrary", name);
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
    /// Sets the default icon family for Web Awesome icons
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
            await module.InvokeVoidAsync("setDefaultIconFamily", family);
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
    /// Gets the current default icon family
    /// </summary>
    /// <returns>The current default icon family name</returns>
    /// <exception cref="InvalidOperationException">Thrown when getting the family fails</exception>
    public virtual async Task<string> GetDefaultIconFamilyAsync()
    {
        try
        {
            var module = await moduleTask.Value;
            return await module.InvokeAsync<string>("getDefaultIconFamily");
        }
        catch (JSException ex)
        {
            throw new InvalidOperationException($"Failed to get default icon family: {ex.Message}", ex);
        }
        catch (JSDisconnectedException)
        {
            // JS runtime is disconnected, return default value silently
            return "classic";
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

    private async Task<IJSObjectReference> LoadModuleAsync()
    {
        return await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./_content/WebAwesome.Blazor/webawesome-interop.js");
    }
}