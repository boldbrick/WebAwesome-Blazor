using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Layout;

/// <summary>
/// A layout component that arranges elements in the block direction with even spacing.
/// Corresponds to the wa-stack utility class.
/// </summary>
public class WaStack : WaLayoutBase
{
    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        AddCommonLayoutAttributes(builder, 0, "div", "wa-stack");
    }

    #endregion
}
