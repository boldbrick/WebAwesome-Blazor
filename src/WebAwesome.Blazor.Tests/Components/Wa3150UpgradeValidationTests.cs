using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Validates the wrapper-level breaking changes of the Web Awesome 3.15.0 bindings: the function-valued members of
/// wa-select, wa-rating, wa-slider and the icon library mutator moved from C# delegates and push methods onto
/// <see cref="WaJsFunction"/> parameters. Removed members are compile-time checks by construction; these tests assert
/// by reflection that they are gone (a reintroduction is caught) and that each replacement exists as a nullable
/// <see cref="WaJsFunction"/>, which a consumer sets as a parameter (or as a property of the library options).
/// That each such parameter reaches the element's member is ElementJsPropertyTests.
/// </summary>
public class Wa3150UpgradeValidationTests
{
    #region ------ Removed members ------

    [Theory]
    [InlineData(typeof(WaSelect), "GetTag")]
    [InlineData(typeof(WaSelect), "SetGetTagFunctionAsync")]
    [InlineData(typeof(WaRating), "SetSymbolFunctionAsync")]
    [InlineData(typeof(WaSliderBase<decimal?>), "SetValueFormatterAsync")]
    [InlineData(typeof(WaSliderBase<decimal>), "SetValueFormatterAsync")]
    [InlineData(typeof(WaSlider), "SetValueFormatterAsync")]
    [InlineData(typeof(WaRange), "SetValueFormatterAsync")]
    [InlineData(typeof(WaSlider), "ValueFormatter")]
    [InlineData(typeof(WaRange), "ValueFormatter")]
    [InlineData(typeof(IconLibraryOptions), "Mutator")]
    public void RemovedMember_IsGone(Type type, string member)
    {
        // Arrange & Act - any member kind, public or not, inherited ones included
        var found = type.GetMember(member, MemberTypes.All, AllMembers);

        // Assert
        Assert.Empty(found);
    }

    [Fact]
    public void WaSliderBase_HasNoValueFormatterMembers()
    {
        // Arrange & Act - the generic definition itself, whatever the type argument
        var found = typeof(WaSliderBase<>).GetMembers(AllMembers)
            .Where(m => m.Name is "SetValueFormatterAsync" or "ValueFormatter");

        // Assert
        Assert.Empty(found);
    }

    #endregion

    #region ------ Replacements ------

    [Theory]
    [InlineData(typeof(WaSelect), "GetTagFunction")]
    [InlineData(typeof(WaRating), "GetSymbolFunction")]
    [InlineData(typeof(WaSlider), "ValueFormatterFunction")]
    [InlineData(typeof(WaRange), "ValueFormatterFunction")]
    public void ReplacementParameter_IsANullableJsFunctionParameter(Type type, string parameter)
    {
        // Arrange & Act
        var property = type.GetProperty(parameter, BindingFlags.Public | BindingFlags.Instance);

        // Assert
        Assert.NotNull(property);
        Assert.Equal(typeof(WaJsFunction), property!.PropertyType);
        Assert.NotNull(property.GetCustomAttribute<ParameterAttribute>());
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).WriteState);
    }

    [Fact]
    public void IconLibraryOptions_MutatorFunction_IsANullableJsFunction()
    {
        // Arrange & Act
        var property = typeof(IconLibraryOptions).GetProperty("MutatorFunction", BindingFlags.Public | BindingFlags.Instance);

        // Assert
        Assert.NotNull(property);
        Assert.Equal(typeof(WaJsFunction), property!.PropertyType);
        Assert.Equal(NullabilityState.Nullable, new NullabilityInfoContext().Create(property).WriteState);
        Assert.Null(new IconLibraryOptions().MutatorFunction);
    }

    #endregion

    #region ------ Internals ------

    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.FlattenHierarchy;

    #endregion
}
