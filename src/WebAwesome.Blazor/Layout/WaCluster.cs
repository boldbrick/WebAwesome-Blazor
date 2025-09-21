using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout component that arranges elements inline with even spacing, allowing items to wrap when space is limited.
/// Corresponds to the wa-cluster utility class.
/// </summary>
public class WaCluster : WaLayoutBase
{
    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        AddCommonLayoutAttributes(builder, 0, "div", "wa-cluster");
    }

    #endregion
}
