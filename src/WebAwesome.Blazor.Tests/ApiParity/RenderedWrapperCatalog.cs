using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AngleSharp.Dom;
using Bunit;
using Bunit.Rendering;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.RenderTree;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Tests.Components;

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
/// RenderRoots (the attribute checks) and RenderSlots (the slot checks) render given parameter sets the same way.
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
            var addedStops = withCallback.StopPropagations.Except(baseline.StopPropagations, StringComparer.Ordinal).ToHashSet(StringComparer.Ordinal);
            callbacks.Add(new RenderedCallback(property.Name, added, withCallback.Error) { AddedStopPropagations = addedStops });
        }

        return new RenderedWrapper(componentType, baseline.Tag, baseline.Handlers, callbacks, baseline.Error) { BaselineStopPropagations = baseline.StopPropagations };
    }

    /// <summary>
    /// The public, concrete wrapper component types of the wrapper assembly, sorted by name.
    /// </summary>
    public static IReadOnlyList<Type> WrapperTypes => wrapperTypes.Value;

    /// <summary>
    /// Renders one component type once per parameter set, under the given culture, and records the tag and the
    /// non-handler attributes of its root element exactly as the render tree holds them (Blazor converts every
    /// attribute value to a string with the current culture when the frame is built, so the values are what the
    /// browser receives).
    /// </summary>
    /// <param name="componentType">Concrete component type</param>
    /// <param name="parameterSets">Parameters to set in each render, in addition to the required ones</param>
    /// <param name="culture">Current culture and UI culture during the renders</param>
    /// <returns>One observation per parameter set, in order</returns>
    public static IReadOnlyList<RenderedRoot> RenderRoots(Type componentType, IReadOnlyList<IReadOnlyList<(string Name, object? Value)>> parameterSets,
        CultureInfo culture)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        try
        {
            using var context = CreateContext();
            return parameterSets.Select(parameters => RenderAttributes(context, componentType, parameters)).ToList();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    /// <summary>
    /// Renders one instance of a component type with each parameter set in turn (a first render, then re-renders of
    /// the same instance, as a parent re-rendering with new values would), under the given culture, and records the
    /// root element's attributes after each render. A parameter keeps its value until a later set assigns it, as in
    /// Blazor, so a set resets a parameter by assigning its default explicitly.
    /// </summary>
    /// <param name="componentType">Concrete component type</param>
    /// <param name="parameterSets">Parameters to set in each render, in addition to the required ones</param>
    /// <param name="culture">Current culture and UI culture during the renders</param>
    /// <returns>One observation per render, in order</returns>
    public static IReadOnlyList<RenderedRoot> RenderSequence(Type componentType, IReadOnlyList<IReadOnlyList<(string Name, object? Value)>> parameterSets,
        CultureInfo culture)
    {
        var previousCulture = CultureInfo.CurrentCulture;
        var previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;

        try
        {
            using var context = CreateContext();
            var required = RequiredParameters(componentType).Select(p => (p.Name, (object?)p.Value)).ToList();
            var names = required.Select(p => p.Name).Concat(ParameterNamesOf(parameterSets)).Distinct(StringComparer.Ordinal).ToList();
            var results = new List<RenderedRoot>();

            var rendered = context.Render<RenderSequenceHost>(p => p
                .Add(h => h.ChildType, componentType)
                .Add(h => h.ParameterNames, names)
                .Add(h => h.ChildParameters, required.Concat(parameterSets[0]).ToList()));

            for (var i = 0; i < parameterSets.Count; i++)
            {
                if (i > 0)
                {
                    var set = parameterSets[i];
                    rendered.Render(p => p.Add(h => h.ChildParameters, required.Concat(set).ToList()));
                }

                var wrapperId = FirstComponentId(CurrentFrames(context.Renderer, rendered.ComponentId));
                results.Add(wrapperId == null
                    ? new RenderedRoot(null, new Dictionary<string, string>(StringComparer.Ordinal), null)
                    : ReadRootAttributes(context.Renderer, wrapperId.Value));
            }
            return results;
        }
        catch (Exception ex)
        {
            return parameterSets.Select(_ => new RenderedRoot(null, new Dictionary<string, string>(StringComparer.Ordinal), $"{ex.GetType().Name}: {ex.Message}")).ToList();
        }
        finally
        {
            CultureInfo.CurrentCulture = previousCulture;
            CultureInfo.CurrentUICulture = previousUiCulture;
        }
    }

    /// <summary>
    /// Renders one component type once per parameter set and records, from the rendered markup, the root element's
    /// direct children with the slot each one is assigned to, and the slot a SlotProbe marker rendered into.
    /// </summary>
    /// <param name="componentType">Concrete component type</param>
    /// <param name="parameterSets">Parameters to set in each render, in addition to the required ones</param>
    /// <returns>One observation per parameter set, in order</returns>
    public static IReadOnlyList<RenderedSlots> RenderSlots(Type componentType, IReadOnlyList<IReadOnlyList<(string Name, object? Value)>> parameterSets)
    {
        using var context = CreateContext();
        return parameterSets.Select(parameters => RenderSlotSet(context, componentType, parameters)).ToList();
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

    // Blazor renders @onX:stopPropagation / @onX:preventDefault as attributes with these name prefixes (see
    // WebRenderTreeBuilderExtensions); the browser renderer applies them as directives, never as DOM attributes
    private const string InternalAttributePrefix = "__internal_";
    private const string StopPropagationAttributePrefix = "__internal_stopPropagation_";
    private const string CurrentFramesMethodName = "GetCurrentRenderTreeFrames";
    private const string ValueExpressionParameter = "ValueExpression";
    private const string SlotAttribute = "slot";
    private const string IconTag = "wa-icon";
    private const string IconNameAttribute = "name";
    private const string ScriptTag = "script";
    private const string AttributeMemoryTypeName = "WebAwesome.Blazor.Base.WaAttributeMemory";
    private const string AttributeMemoryOfMethod = "Of";
    private const string PassedDefaultsPropertyName = "PassedDefaults";

    private static readonly Lazy<IReadOnlyList<Type>> wrapperTypes = new(() => ApiParityData.WrapperAssembly.GetTypes()
        .Where(IsWrapperComponent)
        .OrderBy(t => t.Name, StringComparer.Ordinal)
        .ToList());

    private static readonly Lazy<IReadOnlyList<RenderedWrapper>> all = new(ObserveAll);

    private static IReadOnlyList<RenderedWrapper> ObserveAll()
    {
        return WrapperTypes.Select(Observe).ToList();
    }

    private static RenderedRoot RenderAttributes(BunitContext context, Type componentType, IReadOnlyList<(string Name, object? Value)> parameters)
    {
        try
        {
            var rendered = context.Render(builder =>
            {
                var sequence = 0;
                builder.OpenComponent(sequence++, componentType);

                foreach (var (name, value) in RequiredParameters(componentType))
                    builder.AddComponentParameter(sequence++, name, value);

                foreach (var (name, value) in parameters)
                    builder.AddComponentParameter(sequence++, name, value);

                builder.CloseComponent();
            });

            var wrapperId = FirstComponentId(CurrentFrames(context.Renderer, rendered.ComponentId));
            return wrapperId == null
                ? new RenderedRoot(null, new Dictionary<string, string>(StringComparer.Ordinal), null)
                : ReadRootAttributes(context.Renderer, wrapperId.Value) with { PassedDefaults = PassedDefaultsOf(rendered, componentType) };
        }
        catch (Exception ex)
        {
            return new RenderedRoot(null, new Dictionary<string, string>(StringComparer.Ordinal), $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // the element defaults the wrapper's sticky call sites passed during the render, by attribute name, read from its
    // WaAttributeMemory (internal to the library, hence the reflection)
    private static IReadOnlyDictionary<string, string> PassedDefaultsOf(IRenderedComponent<ContainerFragment> rendered, Type componentType)
    {
        var instance = FindInstanceMethod.MakeGenericMethod(componentType).Invoke(null, [rendered]);
        if (instance == null) return new Dictionary<string, string>(StringComparer.Ordinal);

        var memory = AttributeMemoryOf.Invoke(null, [instance])!;
        var passed = (IReadOnlyDictionary<string, string>)PassedDefaultsProperty.GetValue(memory)!;
        return new Dictionary<string, string>(passed, StringComparer.Ordinal);
    }

    private static object? FindInstance<TComponent>(IRenderedComponent<ContainerFragment> rendered) where TComponent : IComponent
        => rendered.FindComponents<TComponent>().Select(c => (object)c.Instance).FirstOrDefault();

    private static readonly MethodInfo FindInstanceMethod = typeof(RenderedWrapperCatalog)
        .GetMethod(nameof(FindInstance), BindingFlags.NonPublic | BindingFlags.Static)!;

    private static readonly Type AttributeMemoryType = ApiParityData.WrapperAssembly.GetType(AttributeMemoryTypeName, throwOnError: true)!;

    private static readonly MethodInfo AttributeMemoryOf = AttributeMemoryType.GetMethod(AttributeMemoryOfMethod, BindingFlags.Public | BindingFlags.Static)
        ?? throw new InvalidOperationException($"{AttributeMemoryTypeName}.{AttributeMemoryOfMethod} not found");

    private static readonly PropertyInfo PassedDefaultsProperty = AttributeMemoryType.GetProperty(PassedDefaultsPropertyName, BindingFlags.Public | BindingFlags.Instance)
        ?? throw new InvalidOperationException($"{AttributeMemoryTypeName}.{PassedDefaultsPropertyName} not found");

    private static RenderedSlots RenderSlotSet(BunitContext context, Type componentType, IReadOnlyList<(string Name, object? Value)> parameters)
    {
        try
        {
            var rendered = context.Render(builder =>
            {
                var sequence = 0;
                builder.OpenComponent(sequence++, componentType);

                foreach (var (name, value) in RequiredParameters(componentType))
                    builder.AddComponentParameter(sequence++, name, value);

                foreach (var (name, value) in parameters)
                    builder.AddComponentParameter(sequence++, name, value);

                builder.CloseComponent();
            });

            // the markup of a component rendering another component first is flattened, so its first element is the root
            var root = rendered.Nodes.OfType<IElement>().FirstOrDefault();
            if (root == null) return new RenderedSlots(null, Array.Empty<RenderedSlotChild>(), null, null);

            var children = new List<RenderedSlotChild>();
            foreach (var node in root.ChildNodes)
            {
                if (node is IElement element)
                {
                    children.Add(new RenderedSlotChild(element.GetAttribute(SlotAttribute) ?? string.Empty, element.LocalName,
                        element.LocalName == IconTag ? element.GetAttribute(IconNameAttribute) : null));
                }
                else if (node is IText text && !string.IsNullOrWhiteSpace(text.Data))
                {
                    children.Add(new RenderedSlotChild(string.Empty, null, null));
                }
            }

            // the text of a data child (wa-markdown's source script) is raw text in the markup, so a marker rendered
            // there shows as its markup string instead of as an element
            var probeInData = root.Children.Any(c => c.LocalName == ScriptTag && c.TextContent.Contains($"<{SlotProbe.Tag}", StringComparison.Ordinal));

            return new RenderedSlots(root.LocalName, children, SlotProbe.SlotOf(root), null) { ProbeInData = probeInData };
        }
        catch (Exception ex)
        {
            return new RenderedSlots(null, Array.Empty<RenderedSlotChild>(), null, $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    // the attributes of the root element (followed into a child component rendered first, like ObserveRootElement);
    // a bool true attribute renders as present without a value, handlers are left out
    private static RenderedRoot ReadRootAttributes(BunitRenderer renderer, int componentId)
    {
        var frames = CurrentFrames(renderer, componentId);

        for (var i = 0; i < frames.Count; i++)
        {
            var frame = frames.Array[i];

            if (frame.FrameType == RenderTreeFrameType.Component)
                return ReadRootAttributes(renderer, frame.ComponentId);

            if (frame.FrameType != RenderTreeFrameType.Element) continue;

            var attributes = new Dictionary<string, string>(StringComparer.Ordinal);
            for (var j = i + 1; j < frames.Count && frames.Array[j].FrameType == RenderTreeFrameType.Attribute; j++)
            {
                var attribute = frames.Array[j];
                if (attribute.AttributeEventHandlerId != 0 || IsHandlerValue(attribute.AttributeValue)) continue;
                if (attribute.AttributeName.StartsWith(InternalAttributePrefix, StringComparison.Ordinal)) continue;

                attributes[attribute.AttributeName] = attribute.AttributeValue switch
                {
                    bool => string.Empty,
                    string text => text,
                    var other => Convert.ToString(other, CultureInfo.CurrentCulture) ?? string.Empty
                };
            }

            return new RenderedRoot(frame.ElementName, attributes, null);
        }

        return new RenderedRoot(null, new Dictionary<string, string>(StringComparer.Ordinal), null);
    }

    // sub-components rendering content into a slot of the element wrapper hosting them, not an element of their own
    // (docs\technical.md, "Dynamic slots"); they throw outside their host, so the catalog renders none of them on its
    // own: SlotParityTests renders each through its hosts' ChildContent, and their own tests cover the rest
    private static readonly IReadOnlySet<Type> HostedSlotContentComponents = new HashSet<Type> { typeof(WaDayContent) };

    private static bool IsWrapperComponent(Type type)
    {
        return type.IsPublic
            && type.IsClass
            && !type.IsAbstract
            && !type.ContainsGenericParameters
            && typeof(IComponent).IsAssignableFrom(type)
            && !HostedSlotContentComponents.Contains(type);
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

    private sealed record RootObservation(string? Tag, IReadOnlySet<string> Handlers, string? Error)
    {
        // handler attribute names with a Blazor-side stopPropagation directive on the root element
        public IReadOnlySet<string> StopPropagations { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    }

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

    // every parameter name the sets of a sequence assign, in first-seen order
    private static string[] ParameterNamesOf(IReadOnlyList<IReadOnlyList<(string Name, object? Value)>> parameterSets)
        => parameterSets.SelectMany(s => s.Select(p => p.Name)).Distinct(StringComparer.Ordinal).ToArray();

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
            var stops = new HashSet<string>(StringComparer.Ordinal);
            for (var j = i + 1; j < frames.Count && frames.Array[j].FrameType == RenderTreeFrameType.Attribute; j++)
            {
                var attribute = frames.Array[j];
                if (attribute.AttributeEventHandlerId != 0 || IsHandlerValue(attribute.AttributeValue))
                    handlers.Add(attribute.AttributeName);
                else if (attribute.AttributeName.StartsWith(StopPropagationAttributePrefix, StringComparison.Ordinal) && attribute.AttributeValue is true)
                    stops.Add(attribute.AttributeName[StopPropagationAttributePrefix.Length..]);
            }

            return new RootObservation(frame.ElementName, handlers, null) { StopPropagations = stops };
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

    /// <summary>
    /// Handler attribute names with a Blazor-side stopPropagation directive on the root element with no callback set.
    /// </summary>
    public IReadOnlySet<string> BaselineStopPropagations { get; init; } = new HashSet<string>(StringComparer.Ordinal);
}

/// <summary>
/// The root element of one render of a wrapper and its attributes.
/// </summary>
/// <param name="Tag">Local name of the rendered root element, or null when nothing (or no element) was rendered</param>
/// <param name="Attributes">Non-handler attributes of the root element; a present boolean attribute has an empty value</param>
/// <param name="Error">The render exception, or null when the component rendered</param>
internal sealed record RenderedRoot(string? Tag, IReadOnlyDictionary<string, string> Attributes, string? Error)
{
    /// <summary>
    /// The element defaults the wrapper's sticky call sites passed during the render, by attribute name, in their wire
    /// form (RenderRoots only; empty elsewhere).
    /// </summary>
    public IReadOnlyDictionary<string, string> PassedDefaults { get; init; } = new Dictionary<string, string>(StringComparer.Ordinal);
}

/// <summary>
/// The root element of one render of a wrapper and the slots of its direct children, read from the markup.
/// </summary>
/// <param name="Tag">Local name of the rendered root element, or null when nothing (or no element) was rendered</param>
/// <param name="Children">The root element's direct child elements and non-blank text nodes</param>
/// <param name="ProbeSlot">The slot a SlotProbe marker rendered into (empty for the default slot), or null when none rendered</param>
/// <param name="Error">The render exception, or null when the component rendered</param>
internal sealed record RenderedSlots(string? Tag, IReadOnlyList<RenderedSlotChild> Children, string? ProbeSlot, string? Error)
{
    /// <summary>
    /// Whether a SlotProbe marker rendered as the text of a &lt;script&gt; data child of the root element (e.g.
    /// wa-markdown's source), which the element reads instead of slotting.
    /// </summary>
    public bool ProbeInData { get; init; }
}

/// <summary>
/// One direct child of a rendered root element.
/// </summary>
/// <param name="Slot">The child's slot attribute, empty for the default slot (no slot attribute, or a text node)</param>
/// <param name="LocalName">Local name of the child element, or null for a text node</param>
/// <param name="IconName">The name attribute of a wa-icon child, otherwise null</param>
internal sealed record RenderedSlotChild(string Slot, string? LocalName, string? IconName);

/// <summary>
/// The event handlers one EventCallback parameter added to the root element when set to a no-op delegate.
/// </summary>
/// <param name="Name">Name of the EventCallback parameter</param>
/// <param name="AddedHandlers">Handler attribute names present with the callback set but not without it</param>
/// <param name="Error">The render exception, or null when the component rendered</param>
internal sealed record RenderedCallback(string Name, IReadOnlySet<string> AddedHandlers, string? Error)
{
    /// <summary>
    /// Handler attribute names (e.g. "onwablazor-show") whose Blazor-side stopPropagation directive the callback added
    /// to the root element.
    /// </summary>
    public IReadOnlySet<string> AddedStopPropagations { get; init; } = new HashSet<string>(StringComparer.Ordinal);
}
