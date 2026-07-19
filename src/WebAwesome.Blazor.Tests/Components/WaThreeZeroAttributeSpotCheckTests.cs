using Microsoft.AspNetCore.Components;
using System;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Spot-check tests for a representative sample of the new attributes/events added to existing wrappers in
/// Web Awesome 3.0.0. The full completeness authority for these members is
/// <c>ApiParity.ApiSurfaceParityTests</c>; these tests focus on default values, settability, and event wiring.
/// </summary>
public class WaThreeZeroAttributeSpotCheckTests
{
    #region ------ WaColorPicker ------

    [Fact]
    public void WaColorPicker_NewProperties_DefaultToFalse()
    {
        // Arrange & Act
        var component = new WaColorPicker();

        // Assert
        Assert.False(component.Open);
        Assert.False(component.Uppercase);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
    }

    [Fact]
    public void WaColorPicker_NewEvents_CanBeWired()
    {
        // Arrange
        var component = new WaColorPicker();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnShow = callback;
        component.OnHide = callback;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnShow.HasDelegate);
        Assert.True(component.OnHide.HasDelegate);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ WaSelect ------

    [Fact]
    public void WaSelect_NewProperties_DefaultToFalse()
    {
        // Arrange & Act
        var component = new WaSelect();

        // Assert
        Assert.False(component.Open);
        Assert.False(component.WithHint);
        Assert.False(component.WithLabel);
    }

    [Fact]
    public void WaSelect_NewEvents_CanBeWired()
    {
        // Arrange
        var component = new WaSelect();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ WaSlider ------

    [Fact]
    public void WaSlider_AutofocusAndTooltipDistance_CanBeSet()
    {
        // Arrange
        var component = new WaSlider();

        // Act
        component.AutoFocus = true;
        component.TooltipDistance = 12;

        // Assert
        Assert.True(component.AutoFocus);
        Assert.Equal(12, component.TooltipDistance);
    }

    #endregion

    #region ------ WaSwitch / WaRadioGroup ------

    [Fact]
    public void WaSwitch_WithHint_DefaultsToFalseAndCanBeSet()
    {
        // Arrange
        var component = new WaSwitch();

        // Assert
        Assert.False(component.WithHint);

        // Act
        component.WithHint = true;

        // Assert
        Assert.True(component.WithHint);
    }

    [Fact]
    public void WaRadioGroup_WithHintAndWithLabel_CanBeSet()
    {
        // Arrange
        var component = new WaRadioGroup();

        // Act
        component.WithHint = true;
        component.WithLabel = true;

        // Assert
        Assert.True(component.WithHint);
        Assert.True(component.WithLabel);
    }

    #endregion

    #region ------ WaInput ------

    [Fact]
    public void WaInput_NewBrowserBehaviorProperties_CanBeSet()
    {
        // Arrange
        var component = new WaInput();

        // Act
        component.AutoCapitalize = "words";
        component.AutoCorrect = "off";
        component.AutoFocus = true;
        component.EnterKeyHint = "go";
        component.InputMode = "numeric";
        component.PasswordVisible = true;
        component.WithHint = true;
        component.WithLabel = true;
        component.WithoutSpinButtons = true;

        // Assert
        Assert.Equal("words", component.AutoCapitalize);
        Assert.Equal("off", component.AutoCorrect);
        Assert.True(component.AutoFocus);
        Assert.Equal("go", component.EnterKeyHint);
        Assert.Equal("numeric", component.InputMode);
        Assert.True(component.PasswordVisible);
        Assert.True(component.WithHint);
        Assert.True(component.WithLabel);
        Assert.True(component.WithoutSpinButtons);
    }

    [Fact]
    public void WaInput_OnInvalid_CanBeWired()
    {
        // Arrange
        var component = new WaInput();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ WaButton ------

    [Fact]
    public void WaButton_FormSubmissionProperties_CanBeSet()
    {
        // Arrange
        var component = new WaButton();

        // Act
        component.Form = "my-form";
        component.FormAction = "/submit";
        component.FormEncType = "multipart/form-data";
        component.FormMethod = "post";
        component.FormNoValidate = true;
        component.FormTarget = "_blank";
        component.Name = "action";
        component.Value = "save";

        // Assert
        Assert.Equal("my-form", component.Form);
        Assert.Equal("/submit", component.FormAction);
        Assert.Equal("multipart/form-data", component.FormEncType);
        Assert.Equal("post", component.FormMethod);
        Assert.True(component.FormNoValidate);
        Assert.Equal("_blank", component.FormTarget);
        Assert.Equal("action", component.Name);
        Assert.Equal("save", component.Value);
    }

    [Fact]
    public void WaButton_OnInvalid_CanBeWired()
    {
        // Arrange
        var component = new WaButton();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ WaCheckbox ------

    [Fact]
    public void WaCheckbox_OnInvalid_CanBeWired()
    {
        // Arrange
        var component = new WaCheckbox();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnInvalid = callback;

        // Assert
        Assert.True(component.OnInvalid.HasDelegate);
    }

    #endregion

    #region ------ Overlays: OnAfterShow / OnAfterHide ------

    [Fact]
    public void WaDetails_OnAfterShowAndOnAfterHide_CanBeWired()
    {
        // Arrange
        var component = new WaDetails();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaDialog_OnAfterShowAndOnAfterHide_CanBeWired()
    {
        // Arrange
        var component = new WaDialog();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaDrawer_OnAfterShowAndOnAfterHide_CanBeWired()
    {
        // Arrange
        var component = new WaDrawer();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaDropdown_SizeAndOnAfterShowOnAfterHide()
    {
        // Arrange
        var component = new WaDropdown();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.Size = WaSize.Large;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.Equal(WaSize.Large, component.Size);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaPopover_SkiddingAndOnAfterShowOnAfterHide()
    {
        // Arrange
        var component = new WaPopover();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.Skidding = 8;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.Equal(8, component.Skidding);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaTooltip_NewProperties_CanBeSet()
    {
        // Arrange
        var component = new WaTooltip();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.Disabled = true;
        component.Distance = 10;
        component.HideDelay = 100;
        component.ShowDelay = 50;
        component.Skidding = 5;
        component.OnAfterShow = callback;
        component.OnAfterHide = callback;

        // Assert
        Assert.True(component.Disabled);
        Assert.Equal(10, component.Distance);
        Assert.Equal(100, component.HideDelay);
        Assert.Equal(50, component.ShowDelay);
        Assert.Equal(5, component.Skidding);
        Assert.True(component.OnAfterShow.HasDelegate);
        Assert.True(component.OnAfterHide.HasDelegate);
    }

    [Fact]
    public void WaPopup_OnReposition_CanBeWired()
    {
        // Arrange
        var component = new WaPopup();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnReposition = callback;

        // Assert
        Assert.True(component.OnReposition.HasDelegate);
    }

    #endregion

    #region ------ WaTabGroup ------

    [Fact]
    public void WaTabGroup_WithoutScrollControls_DefaultsToFalseAndCanBeSet()
    {
        // Arrange
        var component = new WaTabGroup();

        // Assert
        Assert.False(component.WithoutScrollControls);

        // Act
        component.WithoutScrollControls = true;

        // Assert
        Assert.True(component.WithoutScrollControls);
    }

    [Fact]
    public void WaTabGroup_OnTabShowAndOnTabHide_CanBeWiredWithWaTabChangeEventArgs()
    {
        // Arrange
        var component = new WaTabGroup();
        WaTabChangeEventArgs? received = null;
        var callback = EventCallback.Factory.Create<WaTabChangeEventArgs>(component, args => received = args);

        // Act
        component.OnTabShow = callback;
        component.OnTabHide = callback;

        // Assert
        Assert.True(component.OnTabShow.HasDelegate);
        Assert.True(component.OnTabHide.HasDelegate);
        Assert.Null(received);
    }

    #endregion

    #region ------ WaDropdownItem ------

    [Fact]
    public void WaDropdownItem_OnBlurAndOnFocus_CanBeWired()
    {
        // Arrange
        var component = new WaDropdownItem();
        var callback = EventCallback.Factory.Create<Microsoft.AspNetCore.Components.Web.FocusEventArgs>(component, args => { });

        // Act
        component.OnBlur = callback;
        component.OnFocus = callback;

        // Assert
        Assert.True(component.OnBlur.HasDelegate);
        Assert.True(component.OnFocus.HasDelegate);
    }

    #endregion

    #region ------ Misc Components ------

    [Fact]
    public void WaAvatar_OnError_CanBeWired()
    {
        // Arrange
        var component = new WaAvatar();
        var callback = EventCallback.Factory.Create<EventArgs>(component, () => { });

        // Act
        component.OnError = callback;

        // Assert
        Assert.True(component.OnError.HasDelegate);
    }

    [Fact]
    public void WaCopyButton_TooltipPlacement_CanBeSet()
    {
        // Arrange
        var component = new WaCopyButton();

        // Act
        component.TooltipPlacement = WaPlacement.Bottom;

        // Assert
        Assert.Equal(WaPlacement.Bottom, component.TooltipPlacement);
    }

    [Fact]
    public void WaIntersectionObserver_Once_DefaultsToFalseAndCanBeSet()
    {
        // Arrange
        var component = new WaIntersectionObserver();

        // Assert
        Assert.False(component.Once);

        // Act
        component.Once = true;

        // Assert
        Assert.True(component.Once);
    }

    [Fact]
    public void WaMutationObserver_NewProperties_CanBeSet()
    {
        // Arrange
        var component = new WaMutationObserver();

        // Act
        component.AttrOldValue = true;
        component.Disabled = true;
        component.CharDataOldValue = true;

        // Assert
        Assert.True(component.AttrOldValue);
        Assert.True(component.Disabled);
        Assert.True(component.CharDataOldValue);
    }

    [Fact]
    public void WaOption_Label_CanBeSet()
    {
        // Arrange
        var component = new WaOption();

        // Act
        component.Label = "Custom label";

        // Assert
        Assert.Equal("Custom label", component.Label);
    }

    [Fact]
    public void WaProgressRing_Label_CanBeSet()
    {
        // Arrange
        var component = new WaProgressRing();

        // Act
        component.Label = "Loading";

        // Assert
        Assert.Equal("Loading", component.Label);
    }

    [Fact]
    public void WaZoomableFrame_SrcDoc_UsesIdiomaticCasing()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act
        component.SrcDoc = "<p>content</p>";

        // Assert - the srcdoc attribute maps to SrcDoc (idiomatic .NET casing, see parity-config attributeOverrides)
        Assert.Equal("<p>content</p>", component.SrcDoc);
        Assert.Null(component.GetType().GetProperty("Srcdoc"));
    }

    [Fact]
    public void WaZoomableFrame_NewIframePassthroughProperties_CanBeSet()
    {
        // Arrange
        var component = new WaZoomableFrame();

        // Act
        component.AllowFullScreen = true;
        component.Loading = WaLoading.Lazy;
        component.ReferrerPolicy = "no-referrer";
        component.Sandbox = "allow-scripts allow-same-origin";

        // Assert
        Assert.True(component.AllowFullScreen);
        Assert.Equal(WaLoading.Lazy, component.Loading);
        Assert.Equal("no-referrer", component.ReferrerPolicy);
        Assert.Equal("allow-scripts allow-same-origin", component.Sandbox);
    }

    [Fact]
    public void WaAnimation_EndDelayAndIterationStart_CanBeSet()
    {
        // Arrange
        var component = new WaAnimation();

        // Act
        component.EndDelay = 250;
        component.IterationStart = 0.5;

        // Assert
        Assert.Equal(250, component.EndDelay);
        Assert.Equal(0.5, component.IterationStart);
    }

    [Fact]
    public void WaBreadcrumb_Label_CanBeSet()
    {
        // Arrange
        var component = new WaBreadcrumb();

        // Act
        component.Label = "Breadcrumbs";

        // Assert
        Assert.Equal("Breadcrumbs", component.Label);
    }

    [Fact]
    public void WaBreadcrumbItem_RelAndTarget_CanBeSet()
    {
        // Arrange
        var component = new WaBreadcrumbItem();

        // Act
        component.Rel = "noopener";
        component.Target = "_blank";

        // Assert
        Assert.Equal("noopener", component.Rel);
        Assert.Equal("_blank", component.Target);
    }

    [Fact]
    public void WaFormatBytes_Display_CanBeSet()
    {
        // Arrange
        var component = new WaFormatBytes();

        // Act
        component.Display = WaDisplay.Long;

        // Assert
        Assert.Equal(WaDisplay.Long, component.Display);
    }

    [Fact]
    public void WaFormatDate_TimeZone_CanBeSet()
    {
        // Arrange
        var component = new WaFormatDate();

        // Act
        component.TimeZone = "America/New_York";

        // Assert
        Assert.Equal("America/New_York", component.TimeZone);
    }

    [Fact]
    public void WaFormatNumber_WithoutGrouping_DefaultsToFalseAndCanBeSet()
    {
        // Arrange
        var component = new WaFormatNumber();

        // Assert
        Assert.False(component.WithoutGrouping);

        // Act
        component.WithoutGrouping = true;

        // Assert
        Assert.True(component.WithoutGrouping);
    }

    [Fact]
    public void WaInclude_AllowScripts_DefaultsToFalseAndCanBeSet()
    {
        // Arrange
        var component = new WaInclude();

        // Assert
        Assert.False(component.AllowScripts);

        // Act
        component.AllowScripts = true;

        // Assert
        Assert.True(component.AllowScripts);
    }

    #endregion
}
