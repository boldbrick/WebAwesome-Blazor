using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WebAwesome.Blazor.Tests.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.ApiParity;

/// <summary>
/// Exports every public wrapper method that reaches the browser through the interop module as the manifest the
/// browser method suite checks its coverage against: tools\e2e\data\wrapper-methods.json lists, per concrete wrapper,
/// each public Task-returning method (inherited ones included) with the module calls it makes - the module function
/// (the kind, e.g. invokeMethod, setProperty, getProperty, setCustomValidity, setElementFunction) and the element
/// member it names (the JS name) - and whether the element is a Web Awesome Pro component. The calls are recorded,
/// not read from source: each method runs once on a wrapper that looks rendered (RecordingJSRuntime), with sample
/// arguments. The browser spec tools\e2e\tests\method-coverage.spec.js requires each listed method to be proven by a
/// browser case or exempted with a reason, so a new method fails here first (the manifest is stale) and then in the
/// browser suite until it is covered. When this test fails, review the received file written next to the test output
/// and copy it over the checked-in manifest.
/// </summary>
public class WrapperMethodManifestTests
{
    /// <summary>
    /// The checked-in manifest must equal the one recorded from the wrappers.
    /// </summary>
    [Fact]
    public async Task WrapperMethodManifest_MatchesRecordedInteropCalls()
    {
        var actual = Serialize(await BuildManifestAsync());

        var receivedPath = Path.Combine(AppContext.BaseDirectory, ReceivedFileName);
        File.WriteAllText(receivedPath, actual);

        var manifestPath = ManifestPath();
        Assert.True(File.Exists(manifestPath),
            $"Wrapper method manifest not found at {manifestPath}. Copy '{receivedPath}' there to create it.");

        var approved = Normalize(File.ReadAllText(manifestPath));

        Assert.True(string.Equals(approved, Normalize(actual), StringComparison.Ordinal),
            "The wrappers' public interop methods differ from the e2e method coverage manifest. " +
            $"Diff '{receivedPath}' against '{manifestPath}'; if the change is intentional, copy the received file over " +
            "the manifest, then cover every new method in tools\\e2e (a case in tests\\helpers\\method-cases, or an " +
            "exemption with a reason in tools\\e2e\\data\\method-coverage-exemptions.json) and drop removed ones.");
    }

    #region ------ Internals ------

    private static readonly string[] ManifestDirectory = ["tools", "e2e", "data"];
    private const string ManifestFileName = "wrapper-methods.json";
    private const string ReceivedFileName = "wrapper-methods.received.json";
    private const string GeneratorPath = "src\\WebAwesome.Blazor.Tests\\ApiParity\\WrapperMethodManifestTests.cs";
    private const string CreateRenderedMethodName = nameof(RecordingJSRuntime.CreateRendered);

    // the module functions whose second argument names the element member they reach
    private static readonly HashSet<string> MemberNamingFunctions = new(StringComparer.Ordinal)
    {
        "invokeMethod", "invokeMethodIfDefined", "setProperty", "getProperty", "syncProperty", "setElementFunction",
    };

    // setCustomValidity(element, message) reaches the element method of the same name
    private const string SetCustomValidityFunction = "setCustomValidity";
    private const int MemberNameIndex = 1;

    // lifecycle members of a component are not imperative wrapper methods
    private static readonly HashSet<string> LifecycleMethodNames = new(StringComparer.Ordinal) { "Dispose", "DisposeAsync" };

    // sample arguments of the recorded calls; their values do not matter, the recording module answers every call
    private const string SampleString = "sample";
    private const int SampleNumber = 1;
    private static readonly DateOnly SampleDate = new(2026, 1, 15);
    private static readonly TimeOnly SampleTime = new(12, 30);
    private static readonly string[] SampleStrings = ["#000000"];

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    /// <summary>
    /// The manifest document.
    /// </summary>
    /// <param name="GeneratedBy">Repository path of the generator (this test)</param>
    /// <param name="Methods">Every public wrapper method that reaches the browser, sorted by id</param>
    private sealed record Manifest(string GeneratedBy, IReadOnlyList<ManifestMethod> Methods);

    /// <summary>
    /// One public method of one concrete wrapper (all overloads of the name together).
    /// </summary>
    /// <param name="Id">"Wrapper.Method", e.g. "WaDialog.ShowAsync"</param>
    /// <param name="Component">Wrapper type name</param>
    /// <param name="Member">Method name</param>
    /// <param name="Tag">Tag the wrapper renders as its root element</param>
    /// <param name="Pro">true when the element is a Web Awesome Pro component</param>
    /// <param name="Kind">Module function of the first call (e.g. invokeMethod, setProperty)</param>
    /// <param name="Js">Element member the first call reaches, or null when the function names none</param>
    /// <param name="Also">Further module calls of the method, as "kind:js"; null when there are none</param>
    private sealed record ManifestMethod(string Id, string Component, string Member, string? Tag, bool Pro, string Kind, string? Js,
        IReadOnlyList<string>? Also);

    /// <summary>
    /// One recorded module call.
    /// </summary>
    /// <param name="Kind">Module function</param>
    /// <param name="Js">Element member it reaches, or null</param>
    private sealed record RecordedCall(string Kind, string? Js);

    private static async Task<Manifest> BuildManifestAsync()
    {
        var methods = new List<ManifestMethod>();
        var problems = new List<string>();

        foreach (var wrapper in RenderedWrapperCatalog.All)
        {
            var type = wrapper.ComponentType;
            var pro = wrapper.Tag != null && ApiParityData.Surface.Components.TryGetValue(wrapper.Tag, out var surface) && surface.Pro;

            foreach (var overloads in PublicAsyncMethods(type).GroupBy(m => m.Name, StringComparer.Ordinal))
            {
                var calls = new List<RecordedCall>();
                foreach (var method in overloads)
                {
                    var (recorded, error) = await RecordAsync(type, method);
                    if (error != null) problems.Add($"{type.Name}.{method.Name}: {error}");
                    calls.AddRange(recorded.Where(c => !calls.Contains(c)));
                }

                if (calls.Count == 0) continue;

                var also = calls.Skip(1).Select(c => c.Js == null ? c.Kind : $"{c.Kind}:{c.Js}").ToList();
                methods.Add(new ManifestMethod($"{type.Name}.{overloads.Key}", type.Name, overloads.Key, wrapper.Tag, pro,
                    calls[0].Kind, calls[0].Js, also.Count == 0 ? null : also));
            }
        }

        Assert.True(problems.Count == 0,
            "These wrapper methods could not be recorded (give them a sample argument in WrapperMethodManifestTests):\n" +
            string.Join("\n", problems));
        Assert.NotEmpty(methods);

        return new Manifest(GeneratorPath, methods.OrderBy(m => m.Id, StringComparer.Ordinal).ToList());
    }

    // public instance methods that return a task and are declared by the wrapper library (not ComponentBase)
    private static IEnumerable<MethodInfo> PublicAsyncMethods(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Where(m => !m.IsSpecialName
                && !m.IsGenericMethodDefinition
                && !LifecycleMethodNames.Contains(m.Name)
                && m.GetBaseDefinition().DeclaringType?.Assembly == ApiParityData.WrapperAssembly
                && IsTaskType(m.ReturnType));
    }

    private static bool IsTaskType(Type type)
    {
        return typeof(Task).IsAssignableFrom(type)
            || type == typeof(ValueTask)
            || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ValueTask<>));
    }

    // runs one method on a wrapper that looks rendered and returns the module calls it made; a method that fails
    // before its first call is an error, a failure after it (e.g. on the recorded default result) is not, and a
    // method that always throws NotSupportedException (an obsolete member) makes no call
    private static async Task<(IReadOnlyList<RecordedCall> Calls, string? Error)> RecordAsync(Type type, MethodInfo method)
    {
        using var runtime = new RecordingJSRuntime();
        var component = typeof(RecordingJSRuntime).GetMethod(CreateRenderedMethodName)!.MakeGenericMethod(type).Invoke(runtime, null);

        Exception? failure = null;
        try
        {
            var result = method.Invoke(component, method.GetParameters().Select(SampleArgument).ToArray());
            await AwaitResultAsync(result);
        }
        catch (Exception ex)
        {
            failure = ex is TargetInvocationException { InnerException: not null } wrapped ? wrapped.InnerException : ex;
        }

        var calls = runtime.Module.Invocations.Select(i => new RecordedCall(i.Identifier, MemberOf(i.Identifier, i.Args))).Distinct().ToList();
        // an obsolete method that always throws NotSupportedException never reaches the browser
        var error = failure is not null and not NotSupportedException && calls.Count == 0 ? $"{failure.GetType().Name}: {failure.Message}" : null;
        return (calls, error);
    }

    private static async Task AwaitResultAsync(object? result)
    {
        switch (result)
        {
            case Task task:
                await task;
                break;
            case ValueTask valueTask:
                await valueTask;
                break;
            case null:
                break;
            default:
                // ValueTask<T>: convert through its AsTask method
                if (result.GetType().GetMethod(nameof(ValueTask<int>.AsTask))?.Invoke(result, null) is Task asTask) await asTask;
                break;
        }
    }

    private static string? MemberOf(string identifier, object?[] args)
    {
        if (identifier == SetCustomValidityFunction) return SetCustomValidityFunction;
        return MemberNamingFunctions.Contains(identifier) && args.Length > MemberNameIndex ? args[MemberNameIndex] as string : null;
    }

    private static object? SampleArgument(ParameterInfo parameter)
    {
        var type = parameter.ParameterType;
        if (parameter.HasDefaultValue)
        {
            var value = parameter.DefaultValue;
            if (value == null && type.IsValueType && Nullable.GetUnderlyingType(type) == null) return Activator.CreateInstance(type);
            return value;
        }

        return SampleOf(Nullable.GetUnderlyingType(type) ?? type);
    }

    private static object? SampleOf(Type type)
    {
        if (type == typeof(string) || type == typeof(object)) return SampleString;
        if (type == typeof(bool)) return true;
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (type == typeof(int) || type == typeof(long) || type == typeof(double) || type == typeof(decimal) || type == typeof(float))
            return Convert.ChangeType(SampleNumber, type, CultureInfo.InvariantCulture);
        if (type == typeof(DateOnly)) return SampleDate;
        if (type == typeof(TimeOnly)) return SampleTime;
        if (type.IsAssignableFrom(typeof(string[]))) return SampleStrings;
        if (type.IsValueType) return Activator.CreateInstance(type);
        return !type.IsAbstract && type.GetConstructor(Type.EmptyTypes) != null ? Activator.CreateInstance(type) : null;
    }

    private static string Serialize(Manifest manifest)
    {
        // LF line ends on every platform, like the other files under tools\e2e
        return JsonSerializer.Serialize(manifest, SerializerOptions).Replace("\r\n", "\n") + "\n";
    }

    private static string ManifestPath()
    {
        // the wrapper project lives in src\WebAwesome.Blazor, two levels below the repository root
        var repoRoot = Path.GetDirectoryName(Path.GetDirectoryName(ApiParityData.WrapperProjectDirectory()))!;
        return Path.Combine([repoRoot, .. ManifestDirectory, ManifestFileName]);
    }

    private static string Normalize(string text)
    {
        return text.Replace("\r\n", "\n").Trim();
    }

    #endregion
}
