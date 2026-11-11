using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// A test-only form control of a sister set whose element declares no size default.
/// </summary>
public sealed class XyzChoiceGroup : XyzFieldBase
{
    /// <inheritdoc />
    protected override string TagName => "xyz-choice-group";

    /// <inheritdoc />
    protected override WaSize? ElementDefaultSize => null;
}
