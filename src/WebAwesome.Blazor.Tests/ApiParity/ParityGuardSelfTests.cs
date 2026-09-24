using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Components;
using Xunit;
using static WebAwesome.Blazor.Tests.ApiParity.ApiParityData;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Demonstrates on synthetic data that the guards added for 3.12.0 catch the defect classes they were
/// written for, independently of the (fixed) wrapper code they pass against: the enum-value checks
/// (forward and reverse) catch the pre-3.12.0 WaFormat { Auto, Relative, Numeric } mapping of
/// wa-relative-time "format", the bool-vs-literal-union check catches the pre-3.12.0 bool Numeric
/// parameter, and the bound-event check catches the "onwa-*" bindings the 3.12.0 sweep removed or
/// rebound. Each guard is also shown not to flag the fixed shape.
/// </summary>
public class ParityGuardSelfTests
{
    #region ------ Enum values: forward and reverse ------

    [Fact]
    public void EnumValueGuard_FlagsPreFixWaFormatMapping_InBothDirections()
    {
        // Arrange - the pre-3.12.0 WaRelativeTime.Format enum against the real 'long' | 'short' | 'narrow' union
        var binding = CreateBinding(typeof(PreFixFormat), typeof(PreFixFormatMapping), FormatUnion);

        // Act
        var outOfUnion = EnumValueParityTests.OutOfUnionValues(binding).Select(v => v.Member).ToList();
        var unreachable = EnumValueParityTests.UnreachableUnionValues(binding).ToList();

        // Assert - no member emits a valid style, and no valid style can be requested
        Assert.Equal(new[] { "Auto", "Relative", "Numeric" }, outOfUnion);
        Assert.Equal(new[] { "long", "short", "narrow" }, unreachable);
    }

    [Fact]
    public void EnumValueGuard_FlagsMissingEnumMember_InReverseDirectionOnly()
    {
        // Arrange - an enum whose values are all valid but that cannot express 'narrow'
        var binding = CreateBinding(typeof(PartialFormat), typeof(PartialFormatMapping), FormatUnion);

        // Act
        var outOfUnion = EnumValueParityTests.OutOfUnionValues(binding).ToList();
        var unreachable = EnumValueParityTests.UnreachableUnionValues(binding).ToList();

        // Assert - only the reverse check notices the gap
        Assert.Empty(outOfUnion);
        Assert.Equal(new[] { "narrow" }, unreachable);
    }

    [Fact]
    public void EnumValueGuard_AcceptsFixedWaRelativeTimeFormat()
    {
        // Arrange - the fixed enum and its real ToHtmlValue from the wrapper assembly
        var toHtmlValue = typeof(WaRelativeTimeFormat).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods())
            .Single(m => m.Name == ToHtmlValueMethodName
                && m.IsStatic
                && m.GetParameters() is { Length: 1 } p
                && p[0].ParameterType == typeof(WaRelativeTimeFormat));
        var binding = new EnumValueParityTests.EnumAttributeBinding("wa-relative-time", "format", typeof(WaRelativeTime),
            typeof(WaRelativeTime).GetProperty(nameof(WaRelativeTime.Format))!, typeof(WaRelativeTimeFormat), FormatUnion, toHtmlValue);

        // Act & Assert
        Assert.Empty(EnumValueParityTests.OutOfUnionValues(binding));
        Assert.Empty(EnumValueParityTests.UnreachableUnionValues(binding));
    }

    #endregion

    #region ------ Bool parameter bound to a string-literal union ------

    [Fact]
    public void BoolUnionGuard_FlagsPreFixBoolNumericParameter()
    {
        // Arrange - the pre-3.12.0 WaRelativeTime shape: numeric is 'always' | 'auto', the parameter a bool
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(PreFixRelativeTime)));

        // Assert
        Assert.True(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
        Assert.Contains("a bool parameter can emit none of its literals", EnumValueParityTests.DescribeBoolBinding(parameter));
    }

    [Fact]
    public void BoolUnionGuard_FlagsNullableBoolParameter()
    {
        // Arrange - bool? has the same defect: Blazor never emits either literal
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(NullableBoolRelativeTime)));

        // Assert
        Assert.True(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
    }

    [Fact]
    public void BoolUnionGuard_AcceptsFixedEnumNumericParameter()
    {
        // Arrange
        var component = CreateSurface(NumericAttribute, NumericUnionType);

        // Act
        var parameter = Assert.Single(EnumValueParityTests.LiteralUnionParameters(SyntheticTag, component, typeof(WaRelativeTime)));

        // Assert
        Assert.False(EnumValueParityTests.IsBoolType(parameter.Property.PropertyType));
    }

    [Fact]
    public void BoolUnionGuard_IgnoresUnionsWithBooleanMember()
    {
        // Assert - a union that admits boolean is not a pure string-literal union, so a bool parameter is fine
        Assert.Null(EnumValueParityTests.ParseStringLiteralUnion("boolean | 'auto'"));
        Assert.Equal(new[] { "always", "auto" }, EnumValueParityTests.ParseStringLiteralUnion(NumericUnionType));
    }

    #endregion

    #region ------ Bound events declared by the CEM ------

    [Fact]
    public void BoundEventGuard_FlagsEventsTheElementNeverDispatches()
    {
        // Arrange - pre-3.12.0 bindings, checked against the real 3.12.0 expected surface
        const string source = """
            builder.OpenElement(0, "wa-zoomable-frame");
            builder.AddAttributeIfHasDelegate(40, "onwa-zoom-change", OnZoomChange);
            builder.AddAttributeIfHasDelegate(41, "onwa-load", OnLoad);
            builder.CloseElement();
            """;

        // Act
        var undeclared = BoundEventCemParityTests.BoundWaEvents(source)
            .Where(b => !BoundEventCemParityTests.IsDeclared(b, Surface.Components))
            .ToList();

        // Assert
        Assert.Equal(new[] { "wa-zoom-change", "wa-load" }, undeclared.Select(b => b.EventName));
        Assert.All(undeclared, b => Assert.Equal("wa-zoomable-frame", b.Tag));
    }

    [Fact]
    public void BoundEventGuard_AttributesBindingToNearestPrecedingElement()
    {
        // Arrange - "wa-change" is not a wa-checkbox event (it dispatches the native change), "wa-invalid" is
        const string source = """
            builder.OpenElement(0, "wa-checkbox");
            builder.AddAttribute(seq++, "onwa-change", OnCheckedChange);
            builder.AddAttributeIfHasDelegate(42, "onwa-invalid", OnInvalid);
            """;

        // Act
        var bindings = BoundEventCemParityTests.BoundWaEvents(source).ToList();

        // Assert
        Assert.Equal(2, bindings.Count);
        Assert.False(BoundEventCemParityTests.IsDeclared(bindings[0], Surface.Components));
        Assert.True(BoundEventCemParityTests.IsDeclared(bindings[1], Surface.Components));
    }

    [Fact]
    public void BoundEventGuard_ReportsBindingWithoutElementAsUndeclared()
    {
        // Arrange
        const string source = "builder.AddAttributeIfHasDelegate(1, \"onwa-show\", OnShow);";

        // Act
        var binding = Assert.Single(BoundEventCemParityTests.BoundWaEvents(source));

        // Assert
        Assert.Null(binding.Tag);
        Assert.False(BoundEventCemParityTests.IsDeclared(binding, Surface.Components));
    }

    #endregion

    #region ------ Internals ------

    private const string ToHtmlValueMethodName = "ToHtmlValue";
    private const string SyntheticTag = "wa-relative-time";
    private const string FormatAttribute = "format";
    private const string NumericAttribute = "numeric";
    private const string NumericUnionType = "'always' | 'auto'";

    private static readonly IReadOnlyList<string> FormatUnion = new[] { "long", "short", "narrow" };

    private static EnumValueParityTests.EnumAttributeBinding CreateBinding(Type enumType, Type mappingType, IReadOnlyList<string> union)
    {
        var toHtmlValue = mappingType.GetMethod(ToHtmlValueMethodName)!;
        return new EnumValueParityTests.EnumAttributeBinding(SyntheticTag, FormatAttribute, typeof(PreFixRelativeTime),
            typeof(PreFixRelativeTime).GetProperty(nameof(PreFixRelativeTime.Numeric))!, enumType, union, toHtmlValue);
    }

    private static ComponentSurface CreateSurface(string attributeName, string attributeType)
    {
        return new ComponentSurface
        {
            Attributes = new Dictionary<string, AttributeSurface>
            {
                [attributeName] = new AttributeSurface { Type = attributeType }
            }
        };
    }

    // the pre-3.12.0 WaFormat enum and its mapping
    private enum PreFixFormat
    {
        Auto,
        Relative,
        Numeric
    }

    private static class PreFixFormatMapping
    {
        public static string ToHtmlValue(PreFixFormat format) => format.ToString().ToLowerInvariant();
    }

    private enum PartialFormat
    {
        Long,
        Short
    }

    private static class PartialFormatMapping
    {
        public static string ToHtmlValue(PartialFormat format) => format.ToString().ToLowerInvariant();
    }

    // the pre-3.12.0 WaRelativeTime parameter shape
    private class PreFixRelativeTime
    {
        [Parameter] public bool Numeric { get; set; }
    }

    private class NullableBoolRelativeTime
    {
        [Parameter] public bool? Numeric { get; set; }
    }

    #endregion
}
