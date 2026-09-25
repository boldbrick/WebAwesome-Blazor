using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Render-level guards for the nullable boolean parameters that do not use Blazor's own boolean rendering.
/// A bool? emitted through ToString() arrives as "True"/"False", which Web Awesome misreads in both cases
/// that exist in 3.12.0:
/// (1) wa-button formnovalidate is a plain Lit Boolean attribute, so any present value (even "False")
/// means true - false must omit the attribute;
/// (2) spellcheck on wa-input, wa-textarea and wa-combobox uses the converter
/// <c>!value || value === "false" ? false : true</c>, which is case-sensitive and reads an empty value as
/// false - true and false must be emitted as exactly "true" and "false", and null must omit the attribute
/// so the element keeps its default (true on wa-input and wa-textarea, false on wa-combobox).
/// </summary>
public class BooleanAttributeEmissionTests : BunitContext
{
    public BooleanAttributeEmissionTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ Plain Boolean attribute ------

    [Fact]
    public void WaButton_FormNoValidateTrue_EmitsPresentAttribute()
    {
        var element = Render<WaButton>(p => p.Add(c => c.FormNoValidate, true)).Find("wa-button");

        Assert.True(element.HasAttribute(FormNoValidateAttribute));
        Assert.Equal(string.Empty, element.GetAttribute(FormNoValidateAttribute));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(null)]
    public void WaButton_FormNoValidateFalseOrNull_OmitsAttribute(bool? value)
    {
        var element = Render<WaButton>(p => p.Add(c => c.FormNoValidate, value)).Find("wa-button");

        // any present value, including "False", would disable form validation
        Assert.False(element.HasAttribute(FormNoValidateAttribute));
    }

    #endregion

    #region ------ "true"/"false" converter attribute ------

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, null)]
    public void WaInput_Spellcheck_EmitsLowercaseTrueFalseOrNothing(bool? value, string? expected)
    {
        var element = Render<WaInput>(p => p
            .Add(c => c.Value, textValue)
            .Add(c => c.ValueExpression, () => textValue)
            .Add(c => c.Spellcheck, value)).Find("wa-input");

        Assert.Equal(expected, element.GetAttribute(SpellcheckAttribute));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, null)]
    public void WaTextArea_Spellcheck_EmitsLowercaseTrueFalseOrNothing(bool? value, string? expected)
    {
        var element = Render<WaTextArea>(p => p
            .Add(c => c.Value, textValue)
            .Add(c => c.ValueExpression, () => textValue)
            .Add(c => c.Spellcheck, value)).Find("wa-textarea");

        Assert.Equal(expected, element.GetAttribute(SpellcheckAttribute));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    [InlineData(null, null)]
    public void WaCombobox_Spellcheck_EmitsLowercaseTrueFalseOrNothing(bool? value, string? expected)
    {
        var element = Render<WaCombobox>(p => p
            .Add(c => c.Value, textValue)
            .Add(c => c.ValueExpression, () => textValue)
            .Add(c => c.Spellcheck, value)).Find("wa-combobox");

        Assert.Equal(expected, element.GetAttribute(SpellcheckAttribute));
    }

    #endregion

    #region ------ Internals ------

    private const string FormNoValidateAttribute = "formnovalidate";
    private const string SpellcheckAttribute = "spellcheck";

    private readonly string? textValue = null;

    #endregion
}
