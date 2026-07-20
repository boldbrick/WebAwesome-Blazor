using System;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Verifies that the common WaInputBase parameters (Label, Hint, Required, Disabled, Size) render
/// the expected attributes, across representative controls covering both the shared
/// WaInputBase.AddCommonAttributes code path (WaInput, WaSelect, WaRange) and WaTextArea's
/// independent, hand-rolled attribute rendering.
/// </summary>
public class FormControlCommonParameterTests : FormControlTestBase
{
    [Fact]
    public void WaInput_CommonParameters_RenderExpectedAttributes()
    {
        var model = new TextModel { Name = "Ada" };
        var cut = RenderControlForm<WaInput, string?>(
            model,
            model.Name,
            value => model.Name = value,
            () => model.Name,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaInput.Label), "Full name");
                builder.AddComponentParameter(11, nameof(WaInput.Hint), "As it appears on your ID");
                builder.AddComponentParameter(12, nameof(WaInput.Required), true);
                builder.AddComponentParameter(13, nameof(WaInput.Disabled), true);
                builder.AddComponentParameter(14, nameof(WaInput.Size), WaSize.Large);
            });

        var element = cut.Find("wa-input");
        Assert.Equal("Full name", element.GetAttribute("label"));
        Assert.Equal("As it appears on your ID", element.GetAttribute("hint"));
        Assert.True(element.HasAttribute("required"));
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("large", element.GetAttribute("size"));
    }

    [Fact]
    public void WaSelect_CommonParameters_RenderExpectedAttributes()
    {
        var model = new TextModel { Name = "apple" };
        var cut = RenderControlForm<WaSelect, string?>(
            model,
            model.Name,
            value => model.Name = value,
            () => model.Name,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaSelect.Label), "Fruit");
                builder.AddComponentParameter(11, nameof(WaSelect.Hint), "Pick one");
                builder.AddComponentParameter(12, nameof(WaSelect.Required), true);
                builder.AddComponentParameter(13, nameof(WaSelect.Disabled), true);
                builder.AddComponentParameter(14, nameof(WaSelect.Size), WaSize.Small);
            });

        var element = cut.Find("wa-select");
        Assert.Equal("Fruit", element.GetAttribute("label"));
        Assert.Equal("Pick one", element.GetAttribute("hint"));
        Assert.True(element.HasAttribute("required"));
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("small", element.GetAttribute("size"));
    }

    [Fact]
    public void WaRange_CommonParameters_RenderExpectedAttributes()
    {
        var model = new DecimalModel { Value = 50m };
        var cut = RenderControlForm<WaRange, decimal>(
            model,
            model.Value,
            value => model.Value = value,
            () => model.Value,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaRange.Label), "Volume");
                builder.AddComponentParameter(11, nameof(WaRange.Hint), "0 to 100");
                builder.AddComponentParameter(12, nameof(WaRange.Required), true);
                builder.AddComponentParameter(13, nameof(WaRange.Disabled), true);
                builder.AddComponentParameter(14, nameof(WaRange.Size), WaSize.Medium);
            });

        var element = cut.Find("wa-slider");
        Assert.Equal("Volume", element.GetAttribute("label"));
        Assert.Equal("0 to 100", element.GetAttribute("hint"));
        Assert.True(element.HasAttribute("required"));
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("medium", element.GetAttribute("size"));
    }

    [Fact]
    public void WaTextArea_CommonParameters_RenderExpectedAttributes()
    {
        // WaTextArea implements InputBase<string?> directly (not WaInputBase) and renders its
        // attributes independently, so its Label/Hint/Required/Disabled/Size wiring is verified
        // separately rather than assumed from the WaInputBase-derived controls above
        var model = new TextModel { Name = "Ada" };
        var cut = RenderControlForm<WaTextArea, string?>(
            model,
            model.Name,
            value => model.Name = value,
            () => model.Name,
            builder =>
            {
                builder.AddComponentParameter(10, nameof(WaTextArea.Label), "Bio");
                builder.AddComponentParameter(11, nameof(WaTextArea.Hint), "Tell us about yourself");
                builder.AddComponentParameter(12, nameof(WaTextArea.Required), true);
                builder.AddComponentParameter(13, nameof(WaTextArea.Disabled), true);
                builder.AddComponentParameter(14, nameof(WaTextArea.Size), WaSize.Small);
            });

        var element = cut.Find("wa-textarea");
        Assert.Equal("Bio", element.GetAttribute("label"));
        Assert.Equal("Tell us about yourself", element.GetAttribute("hint"));
        Assert.True(element.HasAttribute("required"));
        Assert.True(element.HasAttribute("disabled"));
        Assert.Equal("small", element.GetAttribute("size"));
    }

    #region ------ Internals ------

    private class TextModel
    {
        public string? Name { get; set; }
    }

    private class DecimalModel
    {
        public decimal Value { get; set; }
    }

    #endregion
}
