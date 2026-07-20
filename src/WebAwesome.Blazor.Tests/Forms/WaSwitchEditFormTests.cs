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

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
    }

    [Fact]
    public void UserChange_ReadsRealCheckedStateViaInterop()
    {
        // the wrapper reads the custom element's .checked property back through JS interop
        var module = JSInterop.SetupModule(InteropModulePath);
        module.Setup<bool>("getProperty", invocation => Equals(invocation.Arguments[1], "checked")).SetResult(true);

        var model = new SwitchModel { Enabled = false };
        var cut = RenderForm(model);

        cut.Find("wa-switch").Change(bool.TrueString);

        Assert.True(model.Enabled);
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
