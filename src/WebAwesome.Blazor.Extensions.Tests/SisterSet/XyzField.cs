using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// A test-only form control of a sister set whose element declares a size default other than medium.
/// </summary>
public sealed class XyzField : XyzFieldBase
{
    /// <summary>
    /// The element's own size default.
    /// </summary>
    public const WaSize DefaultElementSize = WaSize.Small;

    /// <inheritdoc />
    protected override string TagName => "xyz-field";

    /// <inheritdoc />
    protected override WaSize? ElementDefaultSize => DefaultElementSize;
}
