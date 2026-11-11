using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// The shared render tree of the test form controls, derived from <see cref="WaLabeledInputBase{TValue}"/> outside
/// WebAwesome.Blazor.
/// </summary>
public abstract class XyzFieldBase : WaLabeledInputBase<string?>
{
    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenWaElement(this, 0, TagName);
        AddCommonAttributes(builder, 1);
        AddLabelAndHintAttributes(builder, 12);
        AddWithHintAndLabelAttributes(builder, 14);
        builder.AddAttribute(30, ValueAttribute, CurrentValueAsString);
        AddCommonEventHandlers(builder, 40);
        AddLabelAndHintSlots(builder, 70);
        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    #region ------ Internals ------

    private const string ValueAttribute = "value";

    #endregion

    #region ------ Interface for descendants ------

    /// <summary>
    /// The tag of the wrapped element.
    /// </summary>
    protected abstract string TagName { get; }

    #endregion
}
