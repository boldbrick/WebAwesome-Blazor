using System;
using Bunit;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// EditForm integration tests for WaSwitch. Mirrors WaCheckbox: &lt;wa-switch&gt; is a custom
/// element, not a native &lt;input&gt;, so Blazor's built-in change-event value extraction cannot see
/// its real checked state. WaSwitch reads it back explicitly through
/// <see cref="WebAwesomeJSInterop.GetPropertyAsync{T}"/> rather than relying on
/// EventCallback.Factory.CreateBinder&lt;bool&gt;. There is no meaningful DataAnnotations validation
/// scenario for a plain bool switch (no "invalid boolean" state to provoke), so this class covers
/// rendering and the interop-mediated change round-trip only, same as the existing WaCheckbox tests.
/// </summary>
public class WaSwitchEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersCheckedStateAndValidClass()
    {
        var model = new SwitchModel { Enabled = true };
        var cut = RenderForm(model);

        var element = cut.Find("wa-switch");
        Assert.True(element.HasAttribute("checked"));

        var classes = ClassesOf(element);
        Assert.Contains("user-class", classes);
        Assert.Contains("valid", classes);
        Assert.DoesNotContain("invalid", classes);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UserChange_ReadsRealCheckedStateViaInterop(bool elementChecked)
    {
        // the wrapper reads the custom element's .checked property back through JS interop (Strict, see CheckedReadBack)
        var module = JSInterop.SetupCheckedReadBack(elementChecked);

        var model = new SwitchModel { Enabled = !elementChecked };
        var cut = RenderForm(model);

        cut.Find("wa-switch").Change(bool.TrueString);

        module.VerifyCheckedReadBack();
        Assert.Equal(elementChecked, model.Enabled);
    }

    #region ------ Internals ------

    private class SwitchModel
    {
        public bool Enabled { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(SwitchModel model)
    {
        return RenderControlForm<WaSwitch, bool>(
            model,
            model.Enabled,
            value => model.Enabled = value,
            () => model.Enabled);
    }

    #endregion
}
