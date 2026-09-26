using System;
using System.Collections.Generic;
using System.Linq;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validates the wrapper-level breaking changes of the Web Awesome 3.12.0 bindings (docs\MIGRATION-3.12.0.md):
/// WaRelativeTime's Format/Numeric enums emitted only when set, WaTextArea's nullable Rows and its move onto
/// WaInputBase, the enums realigned with Web Awesome's value sets (new per-component enums, renamed members,
/// WaSize short forms, the WaTrigger flag set), the removed callbacks, parameters and types that never worked,
/// WaDropdownItem's link attributes, and the form control
/// parameters moved out of WaInputBase into the wrappers whose element declares them. Removed
/// or renamed members are compile-time checks by construction; these tests assert the new shape, the emitted
/// markup and, for removals, that the members are gone (reflection), so a reintroduction is caught. The rebound
/// events (WaZoomableFrame native load/error, WaDropdownItem focus/blur) are covered by EventCallbackBindingParityTests,
/// which rejects a handler without the "on" prefix or under a wa- name the element never dispatches, and by the e2e
/// event-dispatch spec.
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

    // the emitted format/numeric values of every member are checked against the CEM unions by
    // RenderedAttributeParityTests and EnumValueParityTests
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

    // the values of the per-component enums are checked against the CEM unions (forward and reverse) by
    // EnumValueParityTests and, as rendered, by RenderedAttributeParityTests; this asserts the parameter types
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
    public void WaRadioAppearance_NormalMember_IsRemoved()
    {
        // Assert - the old Normal member emitted the invalid "normal"; the values are checked by EnumValueParityTests
        Assert.DoesNotContain("Normal", Enum.GetNames<WaRadioAppearance>());
    }

    [Fact]
    public void WaAutoSize_WidthAndHeightMembers_AreRemoved()
    {
        // Assert - the old Width/Height members emitted "width"/"height", which Web Awesome ignores; the values
        // are checked by EnumValueParityTests
        Assert.DoesNotContain("Width", Enum.GetNames<WaAutoSize>());
        Assert.DoesNotContain("Height", Enum.GetNames<WaAutoSize>());
    }

    [Fact]
    public void WaSize_SmallMediumLarge_EmitShortForms()
    {
        // Assert - change detector for an intentional C# naming choice: the CEM union admits both "s" and the
        // deprecated "small", so only this pins which spelling the long member names emit (Web Awesome 3.12.0
        // deprecates "small"/"medium"/"large")
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

    // every flag combination's tokens are checked against the wa-tooltip trigger tokens by EnumValueParityTests
    // (tokenListAttributes) and, as rendered, by RenderedAttributeParityTests
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
    [InlineData(typeof(WaCheckbox), "OnCheckedChange", "onchange")]
    [InlineData(typeof(WaSwitch), "OnCheckedChange", "onchange")]
    [InlineData(typeof(WaRadioGroup), "OnValueChange", "onchange")]
    [InlineData(typeof(WaSlider), "OnValueChange", "onnumericchange")]
    [InlineData(typeof(WaCopyButton), "OnCopy", "onwa-copy")]
    [InlineData(typeof(WaZoomableFrame), "OnLoad", "onload")]
    [InlineData(typeof(WaZoomableFrame), "OnError", "onerror")]
    public void KeptCallbacks_AreWiredToRealEvents(Type wrapper, string memberName, string expectedHandler)
    {
        // Arrange - these were rewired to real events rather than removed
        var rendered = RenderedWrapperCatalog.Observe(wrapper);

        // Act
        var callback = Assert.Single(rendered.Callbacks, c => c.Name == memberName);
        var handlers = rendered.BaselineHandlers.Concat(callback.AddedHandlers).ToList();

        // Assert - the rendered handler, not just the property: with the callback set, the element carries the
        // real event's handler and no handler for the wa-* name Web Awesome never dispatched
        Assert.Contains(expectedHandler, handlers);
        Assert.DoesNotContain(handlers, h => h.StartsWith("onwa-", StringComparison.Ordinal) && h != expectedHandler);
    }

    #endregion

    #region ------ Form control parameters moved out of WaInputBase ------

    [Theory]
    [MemberData(nameof(RemovedFormControlParameters))]
    public void FormControlParameters_TheElementDoesNotDeclare_AreGone(Type wrapper, string parameterName)
    {
        // Assert - WaInputBase rendered these on every form control; the element ignored them, so they did nothing
        Assert.Empty(wrapper.GetMember(parameterName));
    }

    [Theory]
    [MemberData(nameof(KeptFormControlParameters))]
    public void FormControlParameters_TheElementDeclares_KeepTheirApi(Type wrapper, string parameterName)
    {
        // Arrange
        var property = wrapper.GetProperty(parameterName);

        // Assert - same name, type and [Parameter] as the WaInputBase member they replace, now declared by the wrapper
        // or by the intermediate base of the capability cluster every element under it declares, never WaInputBase
        Assert.NotNull(property);
        Assert.Equal(FormControlParameterTypes[parameterName], property!.PropertyType);
        Assert.Equal(ExpectedDeclaringType(wrapper, parameterName), property.DeclaringType);
        Assert.True(property.IsDefined(typeof(ParameterAttribute), inherit: false));
    }

    [Fact]
    public void WaFileInput_MarkupLabelAndHint_AreNamedLikeEveryFormControl()
    {
        // Assert - LabelContent/HintContent were renamed; the label cluster is the one IWaLabeledControl groups
        Assert.Empty(typeof(WaFileInput).GetMember("LabelContent"));
        Assert.Empty(typeof(WaFileInput).GetMember("HintContent"));
        Assert.Equal(typeof(RenderFragment), typeof(WaFileInput).GetProperty(nameof(WaFileInput.MarkupLabel))!.PropertyType);
        Assert.Equal(typeof(RenderFragment), typeof(WaFileInput).GetProperty(nameof(WaFileInput.MarkupHint))!.PropertyType);
        Assert.True(typeof(IWaLabeledControl).IsAssignableFrom(typeof(WaFileInput)));
    }

    [Fact]
    public void WaInputBase_NoLongerDeclaresElementSpecificParameters()
    {
        // Assert - only the attributes every form control has stay in the base class
        Assert.All(FormControlParameterTypes.Keys, name => Assert.Empty(typeof(WaInputBase<string>).GetMember(name)));
        Assert.NotNull(typeof(WaInputBase<string>).GetProperty(nameof(WaInputBase<string>.Disabled)));
    }

    /// <summary>
    /// The wrapper/parameter pairs removed in 3.12.0 because the element does not declare the attribute (or slot).
    /// </summary>
    public static TheoryData<Type, string> RemovedFormControlParameters => FormControlParameterPairs(kept: false);

    /// <summary>
    /// The wrapper/parameter pairs kept in 3.12.0, declared by the wrapper since.
    /// </summary>
    public static TheoryData<Type, string> KeptFormControlParameters => FormControlParameterPairs(kept: true);

    #endregion

    #region ------ WaDropdownItem links ------

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

    #endregion

    #region ------ Internals ------

    // the WaInputBase parameters that only some elements declare (MarkupLabel/MarkupHint: the label/hint slot), and their types
    private static readonly Dictionary<string, Type> FormControlParameterTypes = new(StringComparer.Ordinal)
    {
        ["Readonly"] = typeof(bool),
        ["Required"] = typeof(bool),
        ["MinLength"] = typeof(int?),
        ["MaxLength"] = typeof(int?),
        ["Autocomplete"] = typeof(string),
        ["Label"] = typeof(string),
        ["Hint"] = typeof(string),
        ["MarkupLabel"] = typeof(RenderFragment),
        ["MarkupHint"] = typeof(RenderFragment),
    };

    // which of them each form control's element declares in the 3.12.0 CEM (docs\MIGRATION-3.12.0.md, section 6)
    private static readonly Dictionary<Type, string[]> DeclaredFormControlParameters = new()
    {
        [typeof(WaInput)] = ["Readonly", "Required", "MinLength", "MaxLength", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaTextArea)] = ["Readonly", "Required", "MinLength", "MaxLength", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaNumberInput)] = ["Readonly", "Required", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaCheckbox)] = ["Required", "Hint", "MarkupHint"],
        [typeof(WaSwitch)] = ["Required", "Hint", "MarkupHint"],
        [typeof(WaRadioGroup)] = ["Required", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaSlider)] = ["Readonly", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaRange)] = ["Readonly", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaRating)] = ["Readonly", "Required", "Label"],
        [typeof(WaColorPicker)] = ["Required", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaKnownDate)] = ["Readonly", "Required", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaOtpInput)] = ["Readonly", "Required", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaTimeInput)] = ["Readonly", "Required", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaSelect)] = ["Required", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaCombobox)] = ["Required", "Label", "Hint", "MarkupLabel", "MarkupHint"],
        [typeof(WaDateInput)] = ["Readonly", "Required", "Autocomplete", "Label", "Hint", "MarkupLabel", "MarkupHint"],
    };

    // the intermediate bases declaring a cluster's parameters for every wrapper under them
    private static readonly (Type GenericBase, string[] Parameters)[] ClusterBases =
    [
        (typeof(WaLabeledInputBase<>), ["Label", "Hint", "MarkupLabel", "MarkupHint"]),
        (typeof(WaSliderBase<>), ["Readonly"]),
        (typeof(WaDateInputBase<>), ["Readonly", "Required", "Autocomplete"]),
    ];

    // the closed cluster base of the wrapper declaring the parameter, or the wrapper itself
    private static Type ExpectedDeclaringType(Type wrapper, string parameterName)
    {
        foreach (var (genericBase, parameters) in ClusterBases.Where(c => c.Parameters.Contains(parameterName)))
        {
            for (var current = wrapper.BaseType; current != null; current = current.BaseType)
            {
                if (current.IsGenericType && current.GetGenericTypeDefinition() == genericBase) return current;
            }
        }

        return wrapper;
    }

    private static TheoryData<Type, string> FormControlParameterPairs(bool kept)
    {
        var data = new TheoryData<Type, string>();
        foreach (var (wrapper, declared) in DeclaredFormControlParameters)
        {
            foreach (var name in FormControlParameterTypes.Keys.Where(n => declared.Contains(n) == kept))
                data.Add(wrapper, name);
        }

        return data;
    }

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
