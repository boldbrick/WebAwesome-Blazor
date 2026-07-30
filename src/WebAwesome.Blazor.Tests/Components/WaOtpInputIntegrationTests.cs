using Bunit;
using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.Forms;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Integration tests for the new WaOtpInput component introduced in Web Awesome 3.11.0.
/// Covers attribute emission (including the two independent mask attributes), enum mappings, the
/// clear/complete/invalid events, and label/hint slots. WaOtpInput derives from
/// WaInputBase&lt;string?&gt;, so like the other WaInputBase-derived controls it requires a cascading
/// EditContext to render; tests use <see cref="FormControlTestBase.RenderControlForm{TComponent, TValue}"/>
/// for that purpose. EditForm binding and validation lifecycle are covered separately in
/// Forms/WaOtpInputEditFormTests.cs.
/// </summary>
public class WaOtpInputIntegrationTests : FormControlTestBase
{
    [Fact]
    public void DefaultRender_OmitsOptionalAttributes()
    {
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code);

        var element = cut.Find("wa-otp-input");
        Assert.False(element.HasAttribute("length"));
        Assert.False(element.HasAttribute("appearance"));
        Assert.False(element.HasAttribute("type"));
        Assert.False(element.HasAttribute("case"));
        Assert.False(element.HasAttribute("format"));
        // boolean attributes are omitted when false
        Assert.False(element.HasAttribute("autosubmit"));
        Assert.False(element.HasAttribute("autofocus"));
        Assert.False(element.HasAttribute("mask"));
        Assert.False(element.HasAttribute("with-mask"));
    }

    [Fact]
    public void Parameters_WhenSet_RenderExpectedAttributes()
    {
        var model = new OtpModel { Code = "123456" };
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaOtpInput.Length), 6);
                builder.AddComponentParameter(11, nameof(WaOtpInput.Appearance), WaOtpInputAppearance.Filled);
                builder.AddComponentParameter(12, nameof(WaOtpInput.Type), WaOtpInputType.Numeric);
                builder.AddComponentParameter(13, nameof(WaOtpInput.Case), WaOtpInputCase.Upper);
                builder.AddComponentParameter(14, nameof(WaOtpInput.AutoSubmit), true);
                builder.AddComponentParameter(15, nameof(WaOtpInput.AutoFocus), true);
            });

        var element = cut.Find("wa-otp-input");
        Assert.Equal("123456", element.GetAttribute("value"));
        Assert.Equal("6", element.GetAttribute("length"));
        Assert.Equal("filled", element.GetAttribute("appearance"));
        Assert.Equal("numeric", element.GetAttribute("type"));
        Assert.Equal("upper", element.GetAttribute("case"));
        Assert.True(element.HasAttribute("autosubmit"));
        Assert.True(element.HasAttribute("autofocus"));
    }

    [Fact]
    public void Format_WhenSet_RendersFormatAttribute()
    {
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.Format), "### ###"));

        Assert.Equal("### ###", cut.Find("wa-otp-input").GetAttribute("format"));
    }

    [Fact]
    public void Mask_And_WithMask_AreDistinctFeatures()
    {
        // mask (obscures entered characters) and with-mask (hints empty segments) are independent
        // booleans, not aliases of one another - both must be individually settable and renderable.
        var model = new OtpModel();

        var maskOnly = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.Mask), true));
        var maskOnlyElement = maskOnly.Find("wa-otp-input");
        Assert.True(maskOnlyElement.HasAttribute("mask"));
        Assert.False(maskOnlyElement.HasAttribute("with-mask"));

        var withMaskOnly = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.WithMask), true));
        var withMaskOnlyElement = withMaskOnly.Find("wa-otp-input");
        Assert.False(withMaskOnlyElement.HasAttribute("mask"));
        Assert.True(withMaskOnlyElement.HasAttribute("with-mask"));

        var both = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaOtpInput.Mask), true);
                builder.AddComponentParameter(11, nameof(WaOtpInput.WithMask), true);
            });
        var bothElement = both.Find("wa-otp-input");
        Assert.True(bothElement.HasAttribute("mask"));
        Assert.True(bothElement.HasAttribute("with-mask"));
    }

    [Fact]
    public void Appearance_MapsToHtmlValue()
    {
        Assert.Equal("outlined", WaOtpInputAppearance.Outlined.ToHtmlValue());
        Assert.Equal("filled", WaOtpInputAppearance.Filled.ToHtmlValue());
        Assert.Equal("filled-outlined", WaOtpInputAppearance.FilledOutlined.ToHtmlValue());
        Assert.Equal("contained", WaOtpInputAppearance.Contained.ToHtmlValue());
    }

    [Fact]
    public void Type_MapsToHtmlValue()
    {
        Assert.Equal("numeric", WaOtpInputType.Numeric.ToHtmlValue());
        Assert.Equal("alpha", WaOtpInputType.Alpha.ToHtmlValue());
        Assert.Equal("alphanumeric", WaOtpInputType.Alphanumeric.ToHtmlValue());
    }

    [Fact]
    public void Case_MapsToHtmlValue()
    {
        Assert.Equal("preserve", WaOtpInputCase.Preserve.ToHtmlValue());
        Assert.Equal("upper", WaOtpInputCase.Upper.ToHtmlValue());
        Assert.Equal("lower", WaOtpInputCase.Lower.ToHtmlValue());
    }

    [Fact]
    public void OnClear_WhenWired_ReceivesDomEvent()
    {
        var clearCount = 0;
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.OnClear),
                Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => clearCount++)));

        cut.Find("wa-otp-input").TriggerEvent("onwa-clear", new EventArgs());

        Assert.Equal(1, clearCount);
    }

    [Fact]
    public void OnComplete_WhenWired_ReceivesDomEvent()
    {
        var completeCount = 0;
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.OnComplete),
                Microsoft.AspNetCore.Components.EventCallback.Factory.Create(this, () => completeCount++)));

        cut.Find("wa-otp-input").TriggerEvent("onwa-complete", new EventArgs());

        Assert.Equal(1, completeCount);
    }

    [Fact]
    public void OnInvalid_WhenWired_ReceivesDomEvent()
    {
        var invalidCount = 0;
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder => builder.AddComponentParameter(10, nameof(WaOtpInput.OnInvalid),
                Microsoft.AspNetCore.Components.EventCallback.Factory.Create<EventArgs>(this, () => invalidCount++)));

        cut.Find("wa-otp-input").TriggerEvent("onwa-invalid", new EventArgs());

        Assert.Equal(1, invalidCount);
    }

    [Fact]
    public void MarkupLabelAndHint_WhenProvided_RenderIntoNamedSlots()
    {
        var model = new OtpModel();
        var cut = RenderControlForm<WaOtpInput, string?>(model, model.Code, v => model.Code = v, () => model.Code,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaOtpInput.MarkupLabel),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Verification code")));
                builder.AddComponentParameter(11, nameof(WaOtpInput.MarkupHint),
                    (Microsoft.AspNetCore.Components.RenderFragment)(b => b.AddContent(0, "Check your inbox")));
            });

        Assert.Equal("Verification code", cut.Find("span[slot='label']").TextContent);
        Assert.Equal("Check your inbox", cut.Find("span[slot='hint']").TextContent);
    }

    [Fact]
    public async Task ClearAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaOtpInput();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.ClearAsync());
        Assert.Contains("Cannot clear the field before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task FocusAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaOtpInput();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.FocusAsync());
        Assert.Contains("Cannot focus the field before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task BlurAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaOtpInput();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.BlurAsync());
        Assert.Contains("Cannot blur the field before the component is rendered", exception.Message);
    }

    [Fact]
    public async Task SelectAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var component = new WaOtpInput();

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => component.SelectAsync());
        Assert.Contains("Cannot select text before the component is rendered", exception.Message);
    }

    #region ------ Internals ------

    private class OtpModel
    {
        public string? Code { get; set; }
    }

    #endregion
}
