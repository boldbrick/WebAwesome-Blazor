using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using WebAwesome.Blazor.Base;

// BL0006: the placement path reads the renderer's current render tree frames on purpose; the metadata is dev-only
#pragma warning disable BL0006

namespace WebAwesome.Blazor.Infrastructure;

/// <summary>
/// Dev-only placement metadata for the WA Inspector browser extension (tools\wa-inspector): when enabled, every
/// wrapper writes its type name, its readable component chain and two placement hashes on its root element
/// (<see cref="Constants.InspectorComponentAttribute"/>, <see cref="Constants.InspectorChainAttribute"/>,
/// <see cref="Constants.InspectorPlacementAttribute"/>, <see cref="Constants.InspectorSiteAttribute"/>).
/// </summary>
/// <remarks>
/// <para>
/// The placement of an instance is the path from the render root to its component frame: for each level, the type of
/// the rendering parent and the sequence numbers of the frames that enclose the child in the parent's current render
/// tree. Razor emits sequence numbers as compile-time constants per source position and a loop reuses them, so all
/// instances one loop renders share one placement, and the same markup at another position gets another one.
/// </para>
/// <para>
/// Blazor has no public API for a component's parent frames: the path is read through reflection on the renderer
/// (ComponentBase._renderHandle, RenderHandle._renderer, Renderer.GetComponentState, Renderer.GetCurrentRenderTreeFrames).
/// When a lookup fails (another .NET version, trimmed members), a wrapper writes its type name only. Each placement is
/// computed once per instance, at its first render, and the levels of the ancestors are cached too.
/// </para>
/// </remarks>
internal static class WaInspectorMetadata
{
    /// <summary>
    /// Whether the wrappers write the metadata; set by AddWebAwesome from
    /// <see cref="Models.WebAwesomeOptions.EmitInspectorMetadata"/>. Process-wide: the setting belongs to the host's
    /// environment (development, test), not to a circuit or a user.
    /// </summary>
    public static bool Enabled { get; set; }

    /// <summary>
    /// Writes the metadata attributes of a wrapper's root element when <see cref="Enabled"/> is set; call it right
    /// after the element is opened, before the wrapper's own attributes.
    /// </summary>
    /// <param name="builder">Render tree builder with the wrapper's root element open</param>
    /// <param name="component">The wrapper component</param>
    /// <param name="sequence">Sequence number for the attribute frames (the element's own sequence)</param>
    public static void AddAttributes(RenderTreeBuilder builder, object component, int sequence)
    {
        if (!Enabled) return;

        var placement = Placements.GetValue(component, Compute);
        builder.AddAttribute(sequence, Constants.InspectorComponentAttribute, placement.Component);
        if (placement.Chain is null) return;

        builder.AddAttribute(sequence, Constants.InspectorChainAttribute, placement.Chain);
        builder.AddAttribute(sequence, Constants.InspectorPlacementAttribute, placement.PlacementHash);
        builder.AddAttribute(sequence, Constants.InspectorSiteAttribute, placement.SiteHash);
    }

    #region ------ Internals ------

    // the computed metadata of one wrapper instance
    private sealed record Placement(string Component, string? Chain, string? PlacementHash, string? SiteHash);

    // the cached path of one component (wrapper or not) from the render root: the joined levels, which the hashes are
    // computed from, its own level, and the readable chain of the application types
    private sealed record Level(string Path, string Own, string Chain);

    private static Placement Compute(object component)
    {
        var name = ShortName(component.GetType());
        if (component is not ComponentBase || Accessors is not { } accessors) return new Placement(name, null, null, null);

        try
        {
            var renderHandle = (RenderHandle)accessors.RenderHandleField.GetValue(component)!;
            var renderer = (Renderer?)accessors.RendererField.GetValue(renderHandle);
            if (renderer is null) return new Placement(name, null, null, null);

            var state = accessors.GetComponentState(renderer, (IComponent)component);
            var level = LevelOf(accessors, renderer, state);
            return new Placement(name, level.Chain, Hash(level.Path), Hash(level.Own));
        }
        catch (Exception exception) when (exception is TargetInvocationException or InvalidOperationException or ArgumentException
                                              or InvalidCastException or NullReferenceException)
        {
            // the metadata is a diagnostic aid: a renderer that does not match the expected internals loses the chain
            return new Placement(name, null, null, null);
        }
    }

    private static Level LevelOf(RendererAccessors accessors, Renderer renderer, ComponentState state)
    {
        if (Levels.TryGetValue(state.Component, out var cached)) return cached;

        var type = state.Component.GetType();
        var parent = state.ParentComponentState;
        var isApplicationType = IsApplicationType(type);

        Level level;
        if (parent is null)
        {
            var own = type.FullName ?? type.Name;
            level = new Level(own, own, isApplicationType ? ShortName(type) : string.Empty);
        }
        else
        {
            var parentLevel = LevelOf(accessors, renderer, parent);
            var frames = accessors.GetCurrentRenderTreeFrames(renderer, parent.ComponentId);
            var own = string.Concat(parent.Component.GetType().FullName, LevelPathSeparator, FramePath(frames, state.Component),
                LevelPathSeparator, type.FullName);
            var chain = !isApplicationType ? parentLevel.Chain
                : parentLevel.Chain.Length == 0 ? ShortName(type)
                : string.Concat(parentLevel.Chain, ChainSeparator, ShortName(type));
            level = new Level(string.Concat(parentLevel.Path, LevelSeparator, own), own, chain);
        }

        // a level stays valid as long as its component lives: an instance never moves to another frame position
        Levels.AddOrUpdate(state.Component, level);
        return level;
    }

    // the sequence numbers of the frames from the parent's root to the child's component frame, for example "3.7.12"
    private static string FramePath(ArrayRange<RenderTreeFrame> frames, IComponent child)
    {
        var array = frames.Array;
        var count = frames.Count;

        var target = -1;
        for (var index = 0; index < count; index++)
        {
            if (array[index].FrameType == RenderTreeFrameType.Component && ReferenceEquals(array[index].Component, child))
            {
                target = index;
                break;
            }
        }

        if (target < 0) return UnknownFramePath;

        // descend from the root: skip each sibling subtree that does not hold the target, enter the one that does
        var path = new StringBuilder();
        var position = 0;
        var end = count;
        while (position < end)
        {
            var length = SubtreeLength(array[position]);
            if (position == target || target < position + length)
            {
                if (path.Length > 0) path.Append(FramePathSeparator);
                path.Append(array[position].Sequence.ToString(CultureInfo.InvariantCulture));
                if (position == target) break;

                end = position + length;
                position++;
            }
            else
            {
                position += length;
            }
        }

        return path.ToString();
    }

    private static int SubtreeLength(in RenderTreeFrame frame) => frame.FrameType switch
    {
        RenderTreeFrameType.Element => frame.ElementSubtreeLength,
        RenderTreeFrameType.Region => frame.RegionSubtreeLength,
        RenderTreeFrameType.Component => frame.ComponentSubtreeLength,
        _ => 1
    };

    // framework components (Router, RouteView, CascadingValue and so on) are part of the placement, not of the chain
    private static bool IsApplicationType(Type type)
    {
        var ns = type.Namespace ?? string.Empty;
        return !ns.StartsWith(MicrosoftNamespacePrefix, StringComparison.Ordinal) && !ns.StartsWith(SystemNamespacePrefix, StringComparison.Ordinal);
    }

    // the type name without the generic arity ("WaSelect`1" -> "WaSelect")
    private static string ShortName(Type type)
    {
        var name = type.Name;
        var arity = name.IndexOf(GenericAritySeparator);
        return arity < 0 ? name : name[..arity];
    }

    // FNV-1a over the UTF-16 code units, 64 bits, shortened to HashLength hex digits
    private static string Hash(string value)
    {
        var hash = FnvOffsetBasis;
        foreach (var character in value)
        {
            hash ^= character;
            hash *= FnvPrime;
        }

        return hash.ToString(HashFormat, CultureInfo.InvariantCulture)[..HashLength];
    }

    // the reflection handles into the renderer, resolved once; null when one of them does not exist
    private sealed record RendererAccessors(
        FieldInfo RenderHandleField,
        FieldInfo RendererField,
        Func<Renderer, IComponent, ComponentState> GetComponentState,
        Func<Renderer, int, ArrayRange<RenderTreeFrame>> GetCurrentRenderTreeFrames);

    private static RendererAccessors? ResolveAccessors()
    {
        const BindingFlags instanceMembers = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        var renderHandleField = typeof(ComponentBase).GetField(RenderHandleFieldName, instanceMembers);
        var rendererField = typeof(RenderHandle).GetField(RendererFieldName, instanceMembers);
        var getComponentState = typeof(Renderer).GetMethod(GetComponentStateMethodName, instanceMembers, [typeof(IComponent)]);
        var getFrames = typeof(Renderer).GetMethod(GetCurrentRenderTreeFramesMethodName, instanceMembers, [typeof(int)]);
        if (renderHandleField is null || rendererField is null || getComponentState is null || getFrames is null) return null;

        return new RendererAccessors(
            renderHandleField,
            rendererField,
            getComponentState.CreateDelegate<Func<Renderer, IComponent, ComponentState>>(),
            getFrames.CreateDelegate<Func<Renderer, int, ArrayRange<RenderTreeFrame>>>());
    }

    private static RendererAccessors? Accessors => LazyAccessors.Value;

    private static readonly Lazy<RendererAccessors?> LazyAccessors = new(ResolveAccessors);

    private static readonly ConditionalWeakTable<object, Placement> Placements = new();
    private static readonly ConditionalWeakTable<IComponent, Level> Levels = new();

    private const string RenderHandleFieldName = "_renderHandle";
    private const string RendererFieldName = "_renderer";
    private const string GetComponentStateMethodName = "GetComponentState";
    private const string GetCurrentRenderTreeFramesMethodName = "GetCurrentRenderTreeFrames";

    private const string MicrosoftNamespacePrefix = "Microsoft.";
    private const string SystemNamespacePrefix = "System.";
    private const char GenericAritySeparator = '`';

    // separators of the readable chain, of the levels in the hashed path, inside one level, and in a frame path
    private const string ChainSeparator = ">";
    private const string LevelSeparator = "|";
    private const string LevelPathSeparator = ":";
    private const char FramePathSeparator = '.';

    // a child that is not (yet) in its parent's current frames
    private const string UnknownFramePath = "?";

    private const ulong FnvOffsetBasis = 14695981039346656037;
    private const ulong FnvPrime = 1099511628211;
    private const string HashFormat = "x16";
    private const int HashLength = 12;

    #endregion
}
