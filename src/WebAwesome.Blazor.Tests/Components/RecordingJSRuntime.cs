using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Extensions;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// A JS runtime whose module import returns a RecordingJSObjectReference, plus the setup of a wrapper instance
/// that looks rendered (an injected WebAwesomeJSInterop and an Element reference), so a wrapper method can be
/// called without a browser and its interop call asserted. Any other global JS call fails the test.
/// </summary>
internal sealed class RecordingJSRuntime : IJSRuntime, IDisposable
{
    /// <summary>
    /// The recorded interop module.
    /// </summary>
    public RecordingJSObjectReference Module { get; } = new();

    /// <summary>
    /// Creates a wrapper instance wired to this runtime, with its Element set as if it had been rendered.
    /// </summary>
    /// <typeparam name="TComponent">Wrapper type</typeparam>
    /// <returns>The wrapper instance</returns>
    public TComponent CreateRendered<TComponent>() where TComponent : new()
    {
        var component = CreateUnrendered<TComponent>();
        var element = typeof(TComponent).GetProperty(ElementPropertyName)
            ?? throw new InvalidOperationException($"{typeof(TComponent).Name} has no {ElementPropertyName} property");
        element.SetValue(component, new ElementReference(ElementId));
        return component;
    }

    /// <summary>
    /// Creates a wrapper instance wired to this runtime, with no Element (not rendered yet).
    /// </summary>
    /// <typeparam name="TComponent">Wrapper type</typeparam>
    /// <returns>The wrapper instance</returns>
    public TComponent CreateUnrendered<TComponent>() where TComponent : new()
    {
        var component = new TComponent();
        FindJSInteropProperty(typeof(TComponent)).SetValue(component, services.GetRequiredService<WebAwesomeJSInterop>());
        return component;
    }

    #region ------ Constructors ------

    public RecordingJSRuntime()
    {
        var collection = new ServiceCollection();
        collection.AddWebAwesome();
        collection.AddSingleton<IJSRuntime>(this);
        services = collection.BuildServiceProvider();
    }

    #endregion

    #region ------ Implementation of IJSRuntime ------

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        if (identifier == ImportIdentifier) return ValueTask.FromResult((TValue)(object)Module);

        throw new InvalidOperationException($"Unexpected global JS call '{identifier}'");
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, args);
    }

    #endregion

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        services.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string ImportIdentifier = "import";
    private const string ElementPropertyName = "Element";
    private const string JSInteropPropertyName = "JSInterop";
    private const string ElementId = "recorded-element";

    private readonly ServiceProvider services;

    // the [Inject] JSInterop property is non-public and may be declared by a base class (WaInputBase)
    private static PropertyInfo FindJSInteropProperty(Type type)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            var property = current.GetProperty(JSInteropPropertyName,
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (property != null) return property;
        }

        throw new InvalidOperationException($"{type.Name} has no {JSInteropPropertyName} property");
    }

    #endregion
}
