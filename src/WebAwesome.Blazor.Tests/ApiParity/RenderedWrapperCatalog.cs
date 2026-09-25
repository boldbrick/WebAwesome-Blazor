using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;
using WebAwesome.Blazor.Extensions;

// BL0006: the harness reads the renderer's current render tree frames on purpose, see CurrentFrames
#pragma warning disable BL0006

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Renders every wrapper component of the library with bUnit and records what the renderer actually produced
/// on the root element: the tag it opens and the event handler names it registers, once without any
/// callback and once per EventCallback parameter with a no-op delegate on just that parameter. The
/// event-binding parity checks work on this rendered output instead of on source text, so they see bindings
/// made through constants, variables, computed sequence numbers and base-class helpers alike, and they
/// resolve each wrapper by the tag it renders rather than by its class name (e.g. WaRange renders wa-slider).
/// </summary>
internal static class RenderedWrapperCatalog
{
    /// <summary>
    /// The rendered observation of every public, concrete wrapper component in the wrapper assembly, sorted
    /// by type name; computed once per test run.
    /// </summary>
    public static IReadOnlyList<RenderedWrapper> All => all.Value;

    /// <summary>
    /// Renders one component type and records its root tag and event handlers, without and with each of its
    /// EventCallback parameters bound to a no-op delegate.
    /// </summary>
    /// <param name="componentType">Concrete component type</param>
    /// <returns>The rendered observation</returns>
    public static RenderedWrapper Observe(Type componentType)
    {
        using var context = CreateContext();

        var baseline = RenderRoot(context, componentType, callbackProperty: null);
        var callbacks = new List<RenderedCallback>();

        foreach (var property in EventCallbackParameters(componentType))
        {
            var withCallback = RenderRoot(context, componentType, property);
            var added = withCallback.Handlers.Except(baseline.Handlers, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
            callbacks.Add(new RenderedCallback(property.Name, added, withCallback.Error));
        }

        return new RenderedWrapper(componentType, baseline.Tag, baseline.Handlers, callbacks, baseline.Error);
    }

    /// <summary>
    /// Enumerates the public instance [Parameter] properties of type EventCallback or EventCallback&lt;T&gt;.
    /// </summary>
    /// <param name="componentType">Component type</param>
    /// <returns>The callback parameters, sorted by name</returns>
    public static IEnumerable<PropertyInfo> EventCallbackParameters(Type componentType)
    {
        return componentType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.IsDefined(typeof(ParameterAttribute), inherit: true) && IsEventCallbackType(p.PropertyType))
            .OrderBy(p => p.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// Converts a rendered handler attribute name into the DOM event name the renderer listens to, e.g.
    /// "onwa-show" to "wa-show"; a name without the "on" prefix is returned as null, because Blazor's browser
    /// renderer rejects it at runtime (the handler never registers).
    /// </summary>
    /// <param name="handlerName">Handler attribute name as rendered</param>
    /// <returns>The event name, or null when the name is not a valid event handler name</returns>
    public static string? EventNameOf(string handlerName)
    {
        return handlerName.StartsWith(EventHandlerPrefix, StringComparison.Ordinal) && handlerName.Length > EventHandlerPrefix.Length
            ? handlerName[EventHandlerPrefix.Length..]
            : null;
    }

    #region ------ Internals ------

    private const string EventHandlerPrefix = "on";
    private const string CurrentFramesMethodName = "GetCurrentRenderTreeFrames";
    private const string ValueExpressionParameter = "ValueExpression";

    private static readonly Lazy<IReadOnlyList<RenderedWrapper>> all = new(ObserveAll);

    private static IReadOnlyList<RenderedWrapper> ObserveAll()
    {
        return ApiParityData.WrapperAssembly.GetTypes()
            .Where(IsWrapperComponent)
            .OrderBy(t => t.Name, StringComparer.Ordinal)
            .Select(Observe)
            .ToList();
    }

    private static bool IsWrapperComponent(Type type)
    {
        return type.IsPublic
            && type.IsClass
            && !type.IsAbstract
            && !type.ContainsGenericParameters
            && typeof(IComponent).IsAssignableFrom(type);
    }

    private static bool IsEventCallbackType(Type type)
    {
        return type == typeof(EventCallback)
            || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EventCallback<>));
    }

    private static BunitContext CreateContext()
    {
        var context = new BunitContext();
        context.Services.AddWebAwesome();
        context.JSInterop.Mode = JSRuntimeMode.Loose;
        return context;
    }

    private sealed record RootObservation(string? Tag, IReadOnlySet<string> Handlers, string? Error);

    private static RootObservation RenderRoot(BunitContext context, Type componentType, PropertyInfo? callbackProperty)
    {
        try
        {
            var rendered = context.Render(builder =>
            {
                var sequence = 0;
                builder.OpenComponent(sequence++, componentType);

                foreach (var (name, value) in RequiredParameters(componentType))
                    builder.AddComponentParameter(sequence++, name, value);

                if (callbackProperty != null)
                    builder.AddComponentParameter(sequence++, callbackProperty.Name, CreateNoOpCallback(callbackProperty.PropertyType));

                builder.CloseComponent();
            });

            // the container fragment holds the wrapper as its only component
            var wrapperId = FirstComponentId(CurrentFrames(context.Renderer, rendered.ComponentId));
            return wrapperId == null
                ? new RootObservation(null, new HashSet<string>(StringComparer.Ordinal), null)
                : ObserveRootElement(context.Renderer, wrapperId.Value);
        }
        catch (Exception ex)
        {
            return new RootObservation(null, new HashSet<string>(StringComparer.Ordinal), $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static int? FirstComponentId(ArrayRange<RenderTreeFrame> frames)
    {
        for (var i = 0; i < frames.Count; i++)
        {
            if (frames.Array[i].FrameType == RenderTreeFrameType.Component) return frames.Array[i].ComponentId;
        }

        return null;
    }

    // the first element a component renders is its root; a component rendering another component first is
    // followed into that child. Every attribute frame of the root whose value is a delegate or an EventCallback
    // is an event handler the renderer registers under the attribute name, whatever that name is
    private static RootObservation ObserveRootElement(BunitRenderer renderer, int componentId)
    {
        var frames = CurrentFrames(renderer, componentId);

        for (var i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];

            if (frame.FrameType == RenderTreeFrameType.Component)
                return ObserveRootElement(renderer, frame.ComponentId);

            if (frame.FrameType != RenderTreeFrameType.Element) continue;

            var handlers = new HashSet<string>(StringComparer.Ordinal);
            for (var j = i + 1; j < frames.Count && frames.Array[j].FrameType == RenderTreeFrameType.Attribute; j++)
            {
                var attribute = frames.Array[j];
                if (attribute.AttributeEventHandlerId != 0 || IsHandlerValue(attribute.AttributeValue))
                    handlers.Add(attribute.AttributeName);
            }

            return new RootObservation(frame.ElementName, handlers, null);
        }

        return new RootObservation(null, new HashSet<string>(StringComparer.Ordinal), null);
    }

    // the render tree is the only place that shows a handler bound under a name without the "on" prefix: bUnit's
    // markup (and its TriggerEvent lookup) only knows "on*" handlers, while Blazor's browser renderer throws on
    // such a name. Renderer.GetCurrentRenderTreeFrames is protected, hence the reflection
    private static ArrayRange<RenderTreeFrame> CurrentFrames(BunitRenderer renderer, int componentId)
    {
        return (ArrayRange<RenderTreeFrame>)CurrentFramesMethod.Invoke(renderer, new object[] { componentId })!;
    }

    private static readonly MethodInfo CurrentFramesMethod = typeof(Renderer)
        .GetMethod(CurrentFramesMethodName, BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(int) })
        ?? throw new InvalidOperationException($"Renderer.{CurrentFramesMethodName}(int) not found");

    private static bool IsHandlerValue(object? value)
    {
        return value is MulticastDelegate || (value != null && IsEventCallbackType(value.GetType()));
    }

    // the parameters a component cannot render without: InputBase requires a ValueExpression (normally
    // supplied by @bind-Value), which is pointed at a field of a throwaway holder here
    private static IEnumerable<(string Name, object Value)> RequiredParameters(Type componentType)
    {
        var inputBase = FindGenericBase(componentType, typeof(InputBase<>));
        if (inputBase == null) yield break;

        var valueType = inputBase.GetGenericArguments()[0];
        yield return (ValueExpressionParameter, CreateValueExpression(valueType));
    }

    private static Type? FindGenericBase(Type type, Type genericDefinition)
    {
        for (var current = type; current != null; current = current.BaseType)
        {
            if (current.IsGenericType && current.GetGenericTypeDefinition() == genericDefinition) return current;
        }

        return null;
    }

    private static object CreateValueExpression(Type valueType)
    {
        var holderType = typeof(ValueHolder<>).MakeGenericType(valueType);
        var holder = Activator.CreateInstance(holderType)!;
        var body = Expression.Property(Expression.Constant(holder), nameof(ValueHolder<object>.Value));
        var delegateType = typeof(Func<>).MakeGenericType(valueType);
        return Expression.Lambda(delegateType, body);
    }

    private static object CreateNoOpCallback(Type callbackType)
    {
        if (callbackType == typeof(EventCallback))
            return new EventCallback(null, (Action)NoOp);

        var argumentType = callbackType.GetGenericArguments()[0];
        var noOp = typeof(RenderedWrapperCatalog)
            .GetMethod(nameof(CreateNoOpAction), BindingFlags.NonPublic | BindingFlags.Static)!
            .MakeGenericMethod(argumentType)
            .Invoke(null, null)!;
        return Activator.CreateInstance(callbackType, null, noOp)!;
    }

    private static void NoOp()
    {
    }

    private static Action<T> CreateNoOpAction<T>() => _ => { };

    /// <summary>
    /// Target of the ValueExpression handed to input components.
    /// </summary>
    /// <typeparam name="T">Bound value type</typeparam>
    private sealed class ValueHolder<T>
    {
        public T? Value { get; set; }
    }

    #endregion
}

/// <summary>
/// What a wrapper component rendered on its root element.
/// </summary>
/// <param name="ComponentType">The wrapper component type</param>
/// <param name="Tag">Local name of the rendered root element, or null when nothing (or no element) was rendered</param>
/// <param name="BaselineHandlers">Event handler attribute names on the root element with no callback set</param>
/// <param name="Callbacks">The handlers each EventCallback parameter added when set on its own</param>
/// <param name="Error">The render exception, or null when the component rendered</param>
internal sealed record RenderedWrapper(
    Type ComponentType,
    string? Tag,
    IReadOnlySet<string> BaselineHandlers,
    IReadOnlyList<RenderedCallback> Callbacks,
    string? Error)
{
    /// <summary>
    /// Every event handler attribute name the wrapper renders on its root element in any observed render.
    /// </summary>
    public IEnumerable<string> AllHandlers => BaselineHandlers.Concat(Callbacks.SelectMany(c => c.AddedHandlers)).Distinct(StringComparer.Ordinal);
}

/// <summary>
/// The event handlers one EventCallback parameter added to the root element when set to a no-op delegate.
/// </summary>
/// <param name="Name">Name of the EventCallback parameter</param>
/// <param name="AddedHandlers">Handler attribute names present with the callback set but not without it</param>
/// <param name="Error">The render exception, or null when the component rendered</param>
internal sealed record RenderedCallback(string Name, IReadOnlySet<string> AddedHandlers, string? Error);
