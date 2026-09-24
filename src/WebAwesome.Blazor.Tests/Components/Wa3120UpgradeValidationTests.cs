using System;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validates the wrapper-level breaking changes of the Web Awesome 3.12.0 bindings (docs\MIGRATION-3.12.0.md):
/// WaRelativeTime's Format/Numeric enums emitted only when set, WaTextArea's nullable Rows and its move onto
/// WaInputBase, the enums realigned with Web Awesome's value sets (new per-component enums, renamed members,
/// WaSize short forms, the WaTrigger flag set), the removed callbacks, parameters and types that never worked,
/// WaZoomableFrame's native load/error events, and WaDropdownItem's link attributes and focus events. Removed
/// or renamed members are compile-time checks by construction; these tests assert the new shape, the emitted
/// markup and, for removals, that the members are gone (reflection), so a reintroduction is caught.
/// </summary>
public class Wa3120UpgradeValidationTests : BunitContext
{
    public Wa3120UpgradeValidationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #region ------ WaRelativeTime Format/Numeric ------

    [Fact]
    public void WaRelativeTime_FormatAndNumeric_DefaultToNull_AndOmitAttributes()
    {
        // Arrange & Act
        var cut = Render<WaRelativeTime>(p => p.Add(c => c.DateString, SampleDate));

        // Assert - unset means Web Awesome's defaults (long / auto)
        var element = cut.Find("wa-relative-time");
        Assert.Null(cut.Instance.Format);
        Assert.Null(cut.Instance.Numeric);
        Assert.False(element.HasAttribute("format"));
        Assert.False(element.HasAttribute("numeric"));
    }

    [Theory]
    [InlineData(WaRelativeTimeFormat.Long, "long")]
    [InlineData(WaRelativeTimeFormat.Short, "short")]
    [InlineData(WaRelativeTimeFormat.Narrow, "narrow")]
    public void WaRelativeTime_Format_WhenSet_EmitsIntlStyle(WaRelativeTimeFormat format, string expected)
    {
        // Arrange & Act
        var cut = Render<WaRelativeTime>(p => p
            .Add(c => c.DateString, SampleDate)
            .Add(c => c.Format, format));

        // Assert
        Assert.Equal(expected, cut.Find("wa-relative-time").GetAttribute("format"));
        Assert.Equal(expected, format.ToHtmlValue());
    }

    [Theory]
    [InlineData(WaRelativeTimeNumeric.Auto, "auto")]
    [InlineData(WaRelativeTimeNumeric.Always, "always")]
    public void WaRelativeTime_Numeric_WhenSet_EmitsIntlNumericOption(WaRelativeTimeNumeric numeric, string expected)
    {
        // Arrange & Act - the old bool Numeric=false was dropped by Blazor, so "always" could never be sent
        var cut = Render<WaRelativeTime>(p => p
            .Add(c => c.DateString, SampleDate)
            .Add(c => c.Numeric, numeric));

        // Assert
        Assert.Equal(expected, cut.Find("wa-relative-time").GetAttribute("numeric"));
        Assert.Equal(expected, numeric.ToHtmlValue());
    }

    [Fact]
    public void WaRelativeTime_FormatAndNumeric_AreNullableEnums()
    {
        // Assert
        Assert.Equal(typeof(WaRelativeTimeFormat?), typeof(WaRelativeTime).GetProperty(nameof(WaRelativeTime.Format))!.PropertyType);
        Assert.Equal(typeof(WaRelativeTimeNumeric?), typeof(WaRelativeTime).GetProperty(nameof(WaRelativeTime.Numeric))!.PropertyType);
    }

    #endregion

    #region ------ WaTextArea Rows and base class ------

    [Fact]
    public void WaTextArea_Rows_IsNullableInt_AndOmittedByDefault()
    {
        // Arrange & Act - rows="0" used to be emitted always, overriding Web Awesome's default of 4
        var cut = RenderTextArea();

        // Assert
        Assert.Equal(typeof(int?), typeof(WaTextArea).GetProperty(nameof(WaTextArea.Rows))!.PropertyType);
        Assert.Null(cut.Instance.Rows);
        Assert.False(cut.Find("wa-textarea").HasAttribute("rows"));
    }

    [Fact]
    public void WaTextArea_Rows_WhenSet_EmitsAttribute()
    {
        // Arrange & Act
        var cut = RenderTextArea(p => p.Add(c => c.Rows, 6));

        // Assert
        Assert.Equal("6", cut.Find("wa-textarea").GetAttribute("rows"));
    }

    [Fact]
    public void WaTextArea_DerivesFromWaInputBase()
    {
        // Assert - it shares the value sync, key events and Immediate plumbing of the other text inputs
        Assert.True(typeof(WaInputBase<string?>).IsAssignableFrom(typeof(WaTextArea)));
        Assert.NotNull(typeof(WaTextArea).GetProperty(nameof(WaTextArea.OnKeyDown)));
        Assert.NotNull(typeof(WaTextArea).GetProperty(nameof(WaTextArea.Immediate)));
    }

    [Fact]
    public void WaTextArea_SizeAndAppearance_RenderWebAwesomeValues()
    {
        // Arrange & Act - these were misspelled before the rebase onto WaInputBase
        var cut = RenderTextArea(p => p
            .Add(c => c.Size, WaSize.ExtraLarge)
            .Add(c => c.Appearance, WaInputAppearance.FilledOutlined));

        // Assert
        var element = cut.Find("wa-textarea");
        Assert.Equal("xl", element.GetAttribute("size"));
        Assert.Equal("filled-outlined", element.GetAttribute("appearance"));
    }

    #endregion

    #region ------ New per-component enums ------

    [Fact]
    public void WaDetailsAppearance_ToHtmlValue_MapsAccordionDetailsUnion()
    {
        // Assert
        Assert.Equal("filled", WaDetailsAppearance.Filled.ToHtmlValue());
        Assert.Equal("outlined", WaDetailsAppearance.Outlined.ToHtmlValue());
        Assert.Equal("filled-outlined", WaDetailsAppearance.FilledOutlined.ToHtmlValue());
        Assert.Equal("plain", WaDetailsAppearance.Plain.ToHtmlValue());
        Assert.Equal(4, Enum.GetValues<WaDetailsAppearance>().Length);
    }

    [Fact]
    public void WaBadgeAppearance_ToHtmlValue_MapsBadgeTagUnion()
    {
        // Assert
        Assert.Equal("accent", WaBadgeAppearance.Accent.ToHtmlValue());
        Assert.Equal("filled", WaBadgeAppearance.Filled.ToHtmlValue());
        Assert.Equal("outlined", WaBadgeAppearance.Outlined.ToHtmlValue());
        Assert.Equal("filled-outlined", WaBadgeAppearance.FilledOutlined.ToHtmlValue());
        Assert.Equal(4, Enum.GetValues<WaBadgeAppearance>().Length);
    }

    [Fact]
    public void WaListboxPlacement_ToHtmlValue_MapsTopAndBottom()
    {
        // Assert
        Assert.Equal("top", WaListboxPlacement.Top.ToHtmlValue());
        Assert.Equal("bottom", WaListboxPlacement.Bottom.ToHtmlValue());
        Assert.Equal(2, Enum.GetValues<WaListboxPlacement>().Length);
    }

    [Fact]
    public void WaTooltipSide_ToHtmlValue_MapsFourSides()
    {
        // Assert
        Assert.Equal("top", WaTooltipSide.Top.ToHtmlValue());
        Assert.Equal("right", WaTooltipSide.Right.ToHtmlValue());
        Assert.Equal("bottom", WaTooltipSide.Bottom.ToHtmlValue());
        Assert.Equal("left", WaTooltipSide.Left.ToHtmlValue());
        Assert.Equal(4, Enum.GetValues<WaTooltipSide>().Length);
    }

    [Fact]
    public void WaPickerPlacement_ToHtmlValue_MapsSixTopBottomPlacements()
    {
        // Assert
        Assert.Equal("top", WaPickerPlacement.Top.ToHtmlValue());
        Assert.Equal("top-start", WaPickerPlacement.TopStart.ToHtmlValue());
        Assert.Equal("top-end", WaPickerPlacement.TopEnd.ToHtmlValue());
        Assert.Equal("bottom", WaPickerPlacement.Bottom.ToHtmlValue());
        Assert.Equal("bottom-start", WaPickerPlacement.BottomStart.ToHtmlValue());
        Assert.Equal("bottom-end", WaPickerPlacement.BottomEnd.ToHtmlValue());
        Assert.Equal(6, Enum.GetValues<WaPickerPlacement>().Length);
    }

    [Fact]
    public void WaDropdownItemVariant_ToHtmlValue_MapsDefaultAndDanger()
    {
        // Assert
        Assert.Equal("default", WaDropdownItemVariant.Default.ToHtmlValue());
        Assert.Equal("danger", WaDropdownItemVariant.Danger.ToHtmlValue());
        Assert.Equal(2, Enum.GetValues<WaDropdownItemVariant>().Length);
    }

    [Fact]
    public void WaFormatDate_PerOptionEnums_ToHtmlValue_MapIntlDateTimeFormatOptions()
    {
        // Assert - out-of-range styles made Intl.DateTimeFormat throw when WaDateTimeStyle was shared by all options
        Assert.Equal("narrow", WaDateTimeTextStyle.Narrow.ToHtmlValue());
        Assert.Equal("short", WaDateTimeTextStyle.Short.ToHtmlValue());
        Assert.Equal("long", WaDateTimeTextStyle.Long.ToHtmlValue());
        Assert.Equal("numeric", WaDateTimeNumericStyle.Numeric.ToHtmlValue());
        Assert.Equal("2-digit", WaDateTimeNumericStyle.TwoDigit.ToHtmlValue());
        Assert.Equal("short", WaTimeZoneNameStyle.Short.ToHtmlValue());
        Assert.Equal("long", WaTimeZoneNameStyle.Long.ToHtmlValue());
    }

    [Theory]
    [InlineData(typeof(WaAccordion), nameof(WaAccordion.Appearance), typeof(WaDetailsAppearance))]
    [InlineData(typeof(WaDetails), nameof(WaDetails.Appearance), typeof(WaDetailsAppearance))]
    [InlineData(typeof(WaBadge), nameof(WaBadge.Appearance), typeof(WaBadgeAppearance))]
    [InlineData(typeof(WaTag), nameof(WaTag.Appearance), typeof(WaBadgeAppearance))]
    [InlineData(typeof(WaSelect), nameof(WaSelect.Placement), typeof(WaListboxPlacement))]
    [InlineData(typeof(WaCombobox), nameof(WaCombobox.Placement), typeof(WaListboxPlacement))]
    [InlineData(typeof(WaSlider), nameof(WaSlider.TooltipPlacement), typeof(WaTooltipSide))]
    [InlineData(typeof(WaRange), nameof(WaRange.TooltipPlacement), typeof(WaTooltipSide))]
    [InlineData(typeof(WaCopyButton), nameof(WaCopyButton.TooltipPlacement), typeof(WaTooltipSide))]
    [InlineData(typeof(WaDateInput), nameof(WaDateInput.Placement), typeof(WaPickerPlacement))]
    [InlineData(typeof(WaTimeInput), nameof(WaTimeInput.Placement), typeof(WaPickerPlacement))]
    [InlineData(typeof(WaDropdownItem), nameof(WaDropdownItem.Variant), typeof(WaDropdownItemVariant))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Weekday), typeof(WaDateTimeTextStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Era), typeof(WaDateTimeTextStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Year), typeof(WaDateTimeNumericStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Month), typeof(WaDateTimeStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Day), typeof(WaDateTimeNumericStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Hour), typeof(WaDateTimeNumericStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Minute), typeof(WaDateTimeNumericStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.Second), typeof(WaDateTimeNumericStyle))]
    [InlineData(typeof(WaFormatDate), nameof(WaFormatDate.TimeZoneName), typeof(WaTimeZoneNameStyle))]
    [InlineData(typeof(WaTooltip), nameof(WaTooltip.Trigger), typeof(WaTrigger))]
    public void RealignedParameters_UseTheirComponentEnum(Type wrapper, string propertyName, Type expectedEnum)
    {
        // Arrange & Act
        var propertyType = wrapper.GetProperty(propertyName)!.PropertyType;

        // Assert
        Assert.Equal(expectedEnum, Nullable.GetUnderlyingType(propertyType) ?? propertyType);
    }

    #endregion

    #region ------ Renamed and remapped members ------

    [Fact]
    public void WaRadioAppearance_Default_EmitsDefault()
    {
        // Assert - the old Normal member emitted the invalid "normal"
        Assert.Equal("default", WaRadioAppearance.Default.ToHtmlValue());
        Assert.Equal("button", WaRadioAppearance.Button.ToHtmlValue());
        Assert.DoesNotContain("Normal", Enum.GetNames<WaRadioAppearance>());
    }

    [Fact]
    public void WaAutoSize_HorizontalAndVertical_EmitWebAwesomeValues()
    {
        // Assert - the old Width/Height members emitted "width"/"height", which Web Awesome ignores
        Assert.Equal("horizontal", WaAutoSize.Horizontal.ToHtmlValue());
        Assert.Equal("vertical", WaAutoSize.Vertical.ToHtmlValue());
        Assert.Equal("both", WaAutoSize.Both.ToHtmlValue());
        Assert.DoesNotContain("Width", Enum.GetNames<WaAutoSize>());
        Assert.DoesNotContain("Height", Enum.GetNames<WaAutoSize>());
    }

    [Fact]
    public void WaSize_SmallMediumLarge_EmitShortForms()
    {
        // Assert - Web Awesome 3.12.0 deprecates "small"/"medium"/"large"
        Assert.Equal("s", WaSize.Small.ToHtmlValue());
        Assert.Equal("m", WaSize.Medium.ToHtmlValue());
        Assert.Equal("l", WaSize.Large.ToHtmlValue());
    }

    [Fact]
    public void WaSize_RendersShortFormOnElement()
    {
        // Arrange & Act
        var cut = Render<WaButton>(p => p.Add(c => c.Size, WaSize.Medium));

        // Assert
        Assert.Equal("m", cut.Find("wa-button").GetAttribute("size"));
    }

    [Theory]
    [InlineData(typeof(WaAppearance), "Text")]
    [InlineData(typeof(WaPlacement), "Start")]
    [InlineData(typeof(WaPlacement), "End")]
    [InlineData(typeof(WaDropdownItemType), "Radio")]
    public void RemovedEnumMembers_AreGone(Type enumType, string memberName)
    {
        // Assert - each emitted a value no Web Awesome component accepts
        Assert.DoesNotContain(memberName, Enum.GetNames(enumType));
    }

    #endregion

    #region ------ WaTrigger flags ------

    [Theory]
    [InlineData(WaTrigger.Click, "click")]
    [InlineData(WaTrigger.Hover, "hover")]
    [InlineData(WaTrigger.Focus, "focus")]
    [InlineData(WaTrigger.Manual, "manual")]
    [InlineData(WaTrigger.Hover | WaTrigger.Focus, "hover focus")]
    [InlineData(WaTrigger.Focus | WaTrigger.Click, "click focus")]
    [InlineData(WaTrigger.Manual | WaTrigger.Hover | WaTrigger.Focus | WaTrigger.Click, "click hover focus manual")]
    public void WaTrigger_ToHtmlValue_EmitsSpaceSeparatedTokensInFixedOrder(WaTrigger trigger, string expected)
    {
        // Act & Assert
        Assert.Equal(expected, trigger.ToHtmlValue());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(16)]
    [InlineData(2 | 32)]
    public void WaTrigger_ToHtmlValue_RejectsEmptyOrUndefinedBits(int value)
    {
        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => ((WaTrigger)value).ToHtmlValue());
    }

    [Fact]
    public void WaTrigger_IsFlagsEnum()
    {
        // Assert
        Assert.True(typeof(WaTrigger).IsDefined(typeof(FlagsAttribute), inherit: false));
    }

    [Fact]
    public void WaTooltip_Trigger_DefaultsToNull_AndOmitsAttribute()
    {
        // Arrange & Act - unset means Web Awesome's default "hover focus"
        var cut = Render<WaTooltip>(p => p.Add(c => c.For, "anchor"));

        // Assert
        Assert.Null(cut.Instance.Trigger);
        Assert.False(cut.Find("wa-tooltip").HasAttribute("trigger"));
    }

    [Fact]
    public void WaTooltip_Trigger_WhenSet_EmitsTokens()
    {
        // Arrange & Act - an explicit Hover now means hover only
        var cut = Render<WaTooltip>(p => p
            .Add(c => c.For, "anchor")
            .Add(c => c.Trigger, WaTrigger.Hover | WaTrigger.Click));

        // Assert
        Assert.Equal("click hover", cut.Find("wa-tooltip").GetAttribute("trigger"));
    }

    #endregion

    #region ------ Removed members and types ------

    [Theory]
    [InlineData(typeof(WaInput), "OnPasswordToggle")]
    [InlineData(typeof(WaInput), "OnPasswordVisibilityChange")]
    [InlineData(typeof(WaCopyButton), "OnSuccess")]
    [InlineData(typeof(WaDialog), "OnInitialFocus")]
    [InlineData(typeof(WaDrawer), "OnInitialFocus")]
    [InlineData(typeof(WaOption), "OnSelectedChange")]
    [InlineData(typeof(WaRadio), "OnCheckedChange")]
    [InlineData(typeof(WaZoomableFrame), "OnZoomChange")]
    [InlineData(typeof(WaFormatNumber), "Notation")]
    [InlineData(typeof(WaFormatNumber), "CompactDisplay")]
    [InlineData(typeof(WaFormatNumber), "UseGrouping")]
    public void RemovedMembers_AreGone(Type wrapper, string memberName)
    {
        // Assert - bound to events Web Awesome never dispatches, or to attributes it does not have
        Assert.Empty(wrapper.GetMember(memberName));
    }

    [Theory]
    [InlineData("WaFormat")]
    [InlineData("WaNotation")]
    [InlineData("WaCompactDisplay")]
    [InlineData("WaDropdownTrigger")]
    [InlineData("WaTriggerType")]
    [InlineData("ZoomChangeEventArgs")]
    public void RemovedTypes_AreGone(string typeName)
    {
        // Arrange & Act
        var matches = typeof(WaButton).Assembly.GetTypes().Where(t => t.Name == typeName);

        // Assert
        Assert.Empty(matches);
    }

    [Theory]
    [InlineData(typeof(WaCheckbox), "OnCheckedChange")]
    [InlineData(typeof(WaSwitch), "OnCheckedChange")]
    [InlineData(typeof(WaRadioGroup), "OnValueChange")]
    [InlineData(typeof(WaSlider), "OnValueChange")]
    [InlineData(typeof(WaCopyButton), "OnCopy")]
    [InlineData(typeof(WaZoomableFrame), "OnLoad")]
    [InlineData(typeof(WaZoomableFrame), "OnError")]
    public void KeptCallbacks_StillExist(Type wrapper, string memberName)
    {
        // Assert - these were rewired to real events rather than removed
        Assert.NotNull(wrapper.GetProperty(memberName));
    }

    #endregion

    #region ------ WaZoomableFrame native load/error ------

    [Fact]
    public void WaZoomableFrame_OnLoadAndOnError_FireFromNativeEvents()
    {
        // Arrange
        var loadCount = 0;
        var errorCount = 0;
        var cut = Render<WaZoomableFrame>(p => p
            .Add(c => c.Src, "/frame.html")
            .Add(c => c.OnLoad, _ => loadCount++)
            .Add(c => c.OnError, _ => errorCount++));
        var element = cut.Find("wa-zoomable-frame");

        // Act
        element.TriggerEvent("onload", new EventArgs());
        element.TriggerEvent("onerror", new EventArgs());

        // Assert
        Assert.Equal(1, loadCount);
        Assert.Equal(1, errorCount);
    }

    [Fact]
    public void WaZoomableFrame_DoesNotBindWaPrefixedLoadOrError()
    {
        // Arrange
        var cut = Render<WaZoomableFrame>(p => p
            .Add(c => c.Src, "/frame.html")
            .Add(c => c.OnLoad, _ => { })
            .Add(c => c.OnError, _ => { }));
        var element = cut.Find("wa-zoomable-frame");

        // Act & Assert - the element never dispatches wa-load/wa-error
        Assert.Throws<MissingEventHandlerException>(() => element.TriggerEvent("onwa-load", new EventArgs()));
        Assert.Throws<MissingEventHandlerException>(() => element.TriggerEvent("onwa-error", new EventArgs()));
    }

    #endregion

    #region ------ WaDropdownItem links and focus events ------

    [Fact]
    public void WaDropdownItem_LinkAttributes_Render()
    {
        // Arrange & Act
        var cut = Render<WaDropdownItem>(p => p
            .Add(c => c.Href, "/files/report.pdf")
            .Add(c => c.Target, "_blank")
            .Add(c => c.Rel, "noopener noreferrer")
            .Add(c => c.Download, "report.pdf"));

        // Assert
        var element = cut.Find("wa-dropdown-item");
        Assert.Equal("/files/report.pdf", element.GetAttribute("href"));
        Assert.Equal("_blank", element.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", element.GetAttribute("rel"));
        Assert.Equal("report.pdf", element.GetAttribute("download"));
    }

    [Fact]
    public void WaDropdownItem_LinkAttributes_OmittedByDefault()
    {
        // Arrange & Act
        var cut = Render<WaDropdownItem>();

        // Assert
        var element = cut.Find("wa-dropdown-item");
        Assert.False(element.HasAttribute("href"));
        Assert.False(element.HasAttribute("target"));
        Assert.False(element.HasAttribute("rel"));
        Assert.False(element.HasAttribute("download"));
    }

    [Fact]
    public void WaDropdownItem_OnBlurAndOnFocus_FireFromDomEvents()
    {
        // Arrange - previously bound under the attribute names "blur"/"focus", so Blazor never registered them
        var blurCount = 0;
        var focusCount = 0;
        var cut = Render<WaDropdownItem>(p => p
            .Add(c => c.OnBlur, _ => blurCount++)
            .Add(c => c.OnFocus, _ => focusCount++));
        var element = cut.Find("wa-dropdown-item");

        // Act
        element.TriggerEvent("onfocus", new FocusEventArgs());
        element.TriggerEvent("onblur", new FocusEventArgs());

        // Assert
        Assert.Equal(1, focusCount);
        Assert.Equal(1, blurCount);
        Assert.False(element.HasAttribute("blur"));
        Assert.False(element.HasAttribute("focus"));
    }

    #endregion

    #region ------ Internals ------

    private const string SampleDate = "2026-01-15T10:00:00Z";

    private string? textAreaValue;

    private IRenderedComponent<WaTextArea> RenderTextArea(Action<ComponentParameterCollectionBuilder<WaTextArea>>? configure = null)
    {
        return Render<WaTextArea>(p =>
        {
            p.Add(c => c.Value, textAreaValue)
                .Add(c => c.ValueChanged, value => textAreaValue = value)
                .Add(c => c.ValueExpression, () => textAreaValue);
            configure?.Invoke(p);
        });
    }

    #endregion
}
