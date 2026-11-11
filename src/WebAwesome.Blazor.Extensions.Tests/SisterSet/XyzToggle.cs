namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// A test-only form control of a sister set whose element declares no size attribute at all: the inherited
/// <c>Size</c> parameter is ignored on it.
/// </summary>
public sealed class XyzToggle : XyzFieldBase
{
    /// <inheritdoc />
    protected override string TagName => "xyz-toggle";

    /// <inheritdoc />
    protected override bool ElementHasSize => false;
}
