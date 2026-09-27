using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Renders one child component with the parameters it is given, keeping the child instance across its own re-renders,
/// as a parent does; RenderedWrapperCatalog.RenderSequence re-renders it with each parameter set in turn.
/// </summary>
public sealed class RenderSequenceHost : ComponentBase
{
    /// <summary>
    /// The child component type.
    /// </summary>
    [Parameter, EditorRequired] public Type ChildType { get; set; } = null!;

    /// <summary>
    /// Every parameter name any render assigns, in a fixed order, so each keeps its sequence number across renders.
    /// </summary>
    [Parameter, EditorRequired] public IReadOnlyList<string> ParameterNames { get; set; } = [];

    /// <summary>
    /// The parameters of the current render.
    /// </summary>
    [Parameter, EditorRequired] public IReadOnlyList<(string Name, object? Value)> ChildParameters { get; set; } = [];

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, ChildType);
        foreach (var (name, value) in ChildParameters)
            builder.AddComponentParameter(1 + IndexOf(name), name, value);
        builder.CloseComponent();
    }

    #region ------ Internals ------

    private int IndexOf(string name)
    {
        for (var i = 0; i < ParameterNames.Count; i++)
        {
            if (string.Equals(ParameterNames[i], name, StringComparison.Ordinal)) return i;
        }

        throw new InvalidOperationException($"Parameter '{name}' is not listed in {nameof(ParameterNames)}.");
    }

    #endregion
}
