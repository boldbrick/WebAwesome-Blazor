using System;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The shared attribute formatter refuses the date and time types, as it refuses bools: Base.RenderTreeBuilderExtensions
/// declares [Obsolete(error: true)] overloads of AddAttributeIfNotNull (nullable and not) and AddNumberAttribute for
/// DateOnly, TimeOnly, DateTime and DateTimeOffset, so a wrapper passing one fails to build, and FormatInvariant throws
/// for a temporal value that reaches the generic path another way. Their invariant default forms (01/02/2026, 13:04)
/// are not what Web Awesome parses; the dedicated AddDateAttribute/AddTimeAttribute/AddDateTimeOffsetAttribute
/// helpers are the only way to render them. The extension class is internal, hence the reflection.
/// </summary>
public class TemporalAttributeGuardTests
{
    /// <summary>
    /// The value types each guard overload must block.
    /// </summary>
    public static TheoryData<string, Type> GuardedOverloads
    {
        get
        {
            var data = new TheoryData<string, Type>();
            foreach (var type in TemporalTypes)
            {
                data.Add(AddAttributeIfNotNullMethod, type);
                data.Add(AddAttributeIfNotNullMethod, typeof(Nullable<>).MakeGenericType(type));
                data.Add(AddNumberAttributeMethod, type);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(GuardedOverloads))]
    public void GenericAttributePath_HasACompileTimeGuard(string methodName, Type valueType)
    {
        // Arrange
        var method = ExtensionsType.GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SingleOrDefault(m => m.Name == methodName && !m.IsGenericMethod
                && m.GetParameters().Select(p => p.ParameterType).SequenceEqual([typeof(RenderTreeBuilder), typeof(int), typeof(string), valueType]));

        // Assert - a non-generic overload wins overload resolution over the generic one and fails the build
        Assert.NotNull(method);
        var obsolete = method!.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(obsolete);
        Assert.True(obsolete!.IsError, $"{methodName}({valueType.Name}) must be obsolete as an error");
    }

    [Theory]
    [MemberData(nameof(TemporalValues))]
    public void FormatInvariant_RefusesTemporalValues(object value)
    {
        // Arrange - the generic path reached with a boxed value, which no overload can intercept
        var formatInvariant = ExtensionsType.GetMethod(FormatInvariantMethod, BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(typeof(object));

        // Act & Assert
        var exception = Assert.Throws<TargetInvocationException>(() => formatInvariant.Invoke(null, [value]));
        Assert.IsType<NotSupportedException>(exception.InnerException);
    }

    [Fact]
    public void FormatInvariant_StillFormatsNumbersInvariantly()
    {
        var formatInvariant = ExtensionsType.GetMethod(FormatInvariantMethod, BindingFlags.Public | BindingFlags.Static)!
            .MakeGenericMethod(typeof(object));

        Assert.Equal("2.5", formatInvariant.Invoke(null, [2.5m]));
    }

    /// <summary>
    /// One value of each temporal type.
    /// </summary>
    public static TheoryData<object> TemporalValues => new()
    {
        new DateOnly(2026, 1, 2),
        new TimeOnly(13, 4, 5),
        new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc),
        new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.FromHours(1)),
    };

    #region ------ Internals ------

    private const string ExtensionsTypeName = "WebAwesome.Blazor.Base.RenderTreeBuilderExtensions";
    private const string AddAttributeIfNotNullMethod = "AddAttributeIfNotNull";
    private const string AddNumberAttributeMethod = "AddNumberAttribute";
    private const string FormatInvariantMethod = "FormatInvariant";

    private static readonly Type[] TemporalTypes = [typeof(DateOnly), typeof(TimeOnly), typeof(DateTime), typeof(DateTimeOffset)];

    private static readonly Type ExtensionsType = typeof(WaButton).Assembly.GetType(ExtensionsTypeName, throwOnError: true)!;

    #endregion
}
