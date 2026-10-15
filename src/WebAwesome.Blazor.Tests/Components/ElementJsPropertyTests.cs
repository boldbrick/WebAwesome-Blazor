using Bunit;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions;
using WebAwesome.Blazor.Models;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// The function-valued members (<see cref="WaJsFunction"/> parameters) and the ShadowStyles of the element wrappers
/// (docs\technical.md, "JavaScript function references"): every such parameter is listed here with the element member it
/// binds, every listed member has an arity in the elementFunctions table of webawesome-interop.js (and the table lists
/// nothing else), and each wrapper pushes a member or its shadow styles after a render only when the value changed: a
/// set value, a changed value and a removal once each, an equal value and an unset value never. bUnit cannot host the
/// elements, so the tests read the interop calls; the browser behavior is in tools\e2e\tests\element-functions.spec.js.
/// </summary>
public class ElementJsPropertyTests : BunitContext
{
    [Fact]
    public void EveryFunctionParameter_IsListedWithItsElementMember()
    {
        var declared = WrapperTypes()
            .SelectMany(type => Parameters(type).Where(p => p.PropertyType == typeof(WaJsFunction)).Select(p => (type, p.Name)))
            .ToHashSet();
        var listed = FunctionMembers.Select(m => (m.Wrapper, m.Parameter)).ToHashSet();

        Assert.Equal(listed.OrderBy(e => Describe(e)), declared.OrderBy(e => Describe(e)));
    }

    [Fact]
    public void EveryShadowStylesParameter_IsListed()
    {
        var declared = WrapperTypes()
            .Where(type => Parameters(type).Any(p => p.Name == ShadowStylesParameter && p.PropertyType == typeof(string)))
            .ToHashSet();

        Assert.Equal(ShadowStylesWrappers.OrderBy(t => t.Name), declared.OrderBy(t => t.Name));
    }

    [Fact]
    public void ListedMembers_AreTheElementFunctionsOfTheInteropModule()
    {
        var table = ElementFunctionTable();
        var listed = FunctionMembers.Select(m => (m.Tag, m.Member)).ToHashSet();

        Assert.Equal(listed.OrderBy(e => Describe(e)), table.Keys.OrderBy(e => Describe(e)));
        Assert.All(table, entry => Assert.True(entry.Value > 0, $"{Describe(entry.Key)} has no arity"));
    }

    [Theory]
    [MemberData(nameof(FunctionMemberCases))]
    public void FunctionMember_IsPushedOnlyWhenItChanges(Type wrapper, string parameter, string member)
    {
        var module = SetupInteropModule();
        var first = WaJsFunction.Registered("test.first").WithArgs(new { prefix = ">" });
        var second = WaJsFunction.Module("./Pages/Test.razor.js", "second");

        var host = RenderHost(wrapper, parameter, null);
        Assert.Empty(Pushes(module, SetElementFunctionIdentifier));

        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, parameter, first)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, parameter, WaJsFunction.Registered("test.first").WithArgs(new { prefix = ">" }))));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, parameter, second)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, parameter, null)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, parameter, null)));

        var pushes = Pushes(module, SetElementFunctionIdentifier);
        Assert.Equal([member, member, member], pushes.Select(i => i.Arguments[MemberIndex]));
        Assert.Equal([first.ToString(), second.ToString(), null], pushes.Select(i => i.Arguments[ValueIndex]));
    }

    [Theory]
    [MemberData(nameof(ShadowStylesCases))]
    public void ShadowStyles_ArePushedOnlyWhenTheyChange(Type wrapper)
    {
        var module = SetupInteropModule();

        var host = RenderHost(wrapper, ShadowStylesParameter, null);
        Assert.Empty(Pushes(module, SetShadowStylesIdentifier));

        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, ShadowStylesParameter, FirstStyles)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, ShadowStylesParameter, FirstStyles)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, ShadowStylesParameter, SecondStyles)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, ShadowStylesParameter, string.Empty)));
        host.Render(p => p.Add(h => h.ChildParameters, With(wrapper, ShadowStylesParameter, null)));

        var pushes = Pushes(module, SetShadowStylesIdentifier);
        Assert.Equal([FirstStyles, SecondStyles, null], pushes.Select(i => i.Arguments[StylesIndex]));
    }

    [Fact]
    public void Pagination_WithHrefTemplateAndHrefTemplateFunction_Throws()
    {
        SetupInteropModule();

        var exception = Assert.Throws<InvalidOperationException>(() => Render<WaPagination>(p => p
            .Add(c => c.HrefTemplate, "/products?page={page}")
            .Add(c => c.HrefTemplateFunction, WaJsFunction.Registered("test.href"))));

        Assert.Contains(nameof(WaPagination.HrefTemplateFunction), exception.Message);
    }

    /// <summary>
    /// The function-valued parameters with the member of their element.
    /// </summary>
    public static TheoryData<Type, string, string> FunctionMemberCases()
    {
        var data = new TheoryData<Type, string, string>();
        foreach (var member in FunctionMembers) data.Add(member.Wrapper, member.Parameter, member.Member);
        return data;
    }

    /// <summary>
    /// The wrappers with a ShadowStyles parameter.
    /// </summary>
    public static TheoryData<Type> ShadowStylesCases()
    {
        var data = new TheoryData<Type>();
        foreach (var wrapper in ShadowStylesWrappers) data.Add(wrapper);
        return data;
    }

    #region ------ Constructors ------

    public ElementJsPropertyTests()
    {
        Services.AddWebAwesome();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    #endregion

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string InteropModuleFileName = "webawesome-interop.js";
    private const string SetElementFunctionIdentifier = "setElementFunction";
    private const string SetShadowStylesIdentifier = "setShadowStyles";
    private const string ShadowStylesParameter = "ShadowStyles";
    private const string FirstStyles = ".mine { color: red; }";
    private const string SecondStyles = ".mine { color: blue; }";

    // the arguments of setElementFunction(element, member, descriptorJson, entryPointUrl) and setShadowStyles(element, css)
    private const int MemberIndex = 1;
    private const int ValueIndex = 2;
    private const int StylesIndex = 1;

    private sealed record FunctionMember(Type Wrapper, string Parameter, string Tag, string Member);

    private static readonly FunctionMember[] FunctionMembers =
    [
        new(typeof(WaSelect), nameof(WaSelect.GetTagFunction), "wa-select", "getTag"),
        new(typeof(WaCombobox), nameof(WaCombobox.GetTagFunction), "wa-combobox", "getTag"),
        new(typeof(WaCombobox), nameof(WaCombobox.FilterFunction), "wa-combobox", "filter"),
        new(typeof(WaRating), nameof(WaRating.GetSymbolFunction), "wa-rating", "getSymbol"),
        new(typeof(WaSlider), nameof(WaSlider.ValueFormatterFunction), "wa-slider", "valueFormatter"),
        new(typeof(WaRange), nameof(WaRange.ValueFormatterFunction), "wa-slider", "valueFormatter"),
        new(typeof(WaDatePicker), nameof(WaDatePicker.IsDateDisabledFunction), "wa-date-picker", "isDateDisabled"),
        new(typeof(WaDatePicker), nameof(WaDatePicker.DayContentFunction), "wa-date-picker", "dayContent"),
        new(typeof(WaDateRangePicker), nameof(WaDateRangePicker.IsDateDisabledFunction), "wa-date-picker", "isDateDisabled"),
        new(typeof(WaDateRangePicker), nameof(WaDateRangePicker.DayContentFunction), "wa-date-picker", "dayContent"),
        new(typeof(WaDateInput), nameof(WaDateInput.IsDateDisabledFunction), "wa-date-input", "isDateDisabled"),
        new(typeof(WaDateInput), nameof(WaDateInput.DayContentFunction), "wa-date-input", "dayContent"),
        new(typeof(WaDateRangeInput), nameof(WaDateRangeInput.IsDateDisabledFunction), "wa-date-input", "isDateDisabled"),
        new(typeof(WaDateRangeInput), nameof(WaDateRangeInput.DayContentFunction), "wa-date-input", "dayContent"),
        new(typeof(WaPagination), nameof(WaPagination.HrefTemplateFunction), "wa-pagination", "hrefTemplate"),
    ];

    private static readonly Type[] ShadowStylesWrappers =
    [
        typeof(WaDataGrid), typeof(WaSelect), typeof(WaCombobox), typeof(WaRating),
        typeof(WaDatePicker), typeof(WaDateRangePicker), typeof(WaDateInput), typeof(WaDateRangeInput),
    ];

    // a member of the elementFunctions table: "    'wa-select': {" opens a tag, "        getTag: { arity: 2, ..." a member
    private static readonly Regex TableStartRegex = new(@"^const elementFunctions = \{", RegexOptions.Compiled);
    private static readonly Regex TableTagRegex = new(@"^\s*'(wa-[a-z-]+)': \{\s*$", RegexOptions.Compiled);
    private static readonly Regex TableMemberRegex = new(@"^\s*([A-Za-z_$][\w$]*): \{ arity: (\d+),", RegexOptions.Compiled);
    private const string TableEnd = "};";

    private BunitJSModuleInterop SetupInteropModule()
    {
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid(SetElementFunctionIdentifier, _ => true).SetVoidResult();
        module.SetupVoid(SetShadowStylesIdentifier, _ => true).SetVoidResult();
        return module;
    }

    // renders the wrapper in a host that keeps the instance across the later renders
    private IRenderedComponent<RenderSequenceHost> RenderHost(Type wrapper, string parameter, object? value)
    {
        var names = RenderedWrapperCatalog.RequiredParameters(wrapper).Select(p => p.Name).Append(parameter).ToList();
        return Render<RenderSequenceHost>(p => p
            .Add(h => h.ChildType, wrapper)
            .Add(h => h.ParameterNames, names)
            .Add(h => h.ChildParameters, With(wrapper, parameter, value)));
    }

    private static IReadOnlyList<(string Name, object? Value)> With(Type wrapper, string parameter, object? value) =>
        RenderedWrapperCatalog.RequiredParameters(wrapper).Select(p => (p.Name, (object?)p.Value)).Append((parameter, value)).ToList();

    private static List<JSRuntimeInvocation> Pushes(BunitJSModuleInterop module, string identifier) =>
        module.Invocations.Where(i => i.Identifier == identifier).ToList();

    private static IEnumerable<Type> WrapperTypes() =>
        typeof(WaSelect).Assembly.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract && !t.ContainsGenericParameters && typeof(IComponent).IsAssignableFrom(t));

    private static IEnumerable<PropertyInfo> Parameters(Type type) =>
        type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(p => p.GetCustomAttribute<ParameterAttribute>() != null);

    private static string Describe((Type Wrapper, string Parameter) entry) => $"{entry.Wrapper.Name}.{entry.Parameter}";

    private static string Describe((string Tag, string Member) entry) => $"{entry.Tag} {entry.Member}";

    // (tag, member) -> arity, read from the elementFunctions table of the interop module
    private static Dictionary<(string Tag, string Member), int> ElementFunctionTable()
    {
        var path = Path.Combine(WrapperProjectDirectory(), "wwwroot", InteropModuleFileName);
        var lines = File.ReadAllLines(path).SkipWhile(line => !TableStartRegex.IsMatch(line)).Skip(1).TakeWhile(line => line != TableEnd);

        var table = new Dictionary<(string Tag, string Member), int>();
        string? tag = null;
        foreach (var line in lines)
        {
            var tagMatch = TableTagRegex.Match(line);
            if (tagMatch.Success) tag = tagMatch.Groups[1].Value;

            var memberMatch = TableMemberRegex.Match(line);
            if (memberMatch.Success && tag != null) table[(tag, memberMatch.Groups[1].Value)] = int.Parse(memberMatch.Groups[2].Value, CultureInfo.InvariantCulture);
        }

        Assert.NotEmpty(table);
        return table;
    }

    private static string WrapperProjectDirectory([CallerFilePath] string thisFile = "")
    {
        // this test file lives in src\WebAwesome.Blazor.Tests\Components\
        var testProjectDir = Path.GetDirectoryName(Path.GetDirectoryName(thisFile))!;
        return Path.Combine(Path.GetDirectoryName(testProjectDir)!, "WebAwesome.Blazor");
    }

    #endregion
}
