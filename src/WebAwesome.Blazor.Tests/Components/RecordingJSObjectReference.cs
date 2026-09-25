using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// A stand-in for the imported webawesome-interop.js module that records every call, so a wrapper method test
/// asserts which module function the wrapper invoked with which element method or property name and arguments,
/// instead of only that the call did not throw.
/// </summary>
internal sealed class RecordingJSObjectReference : IJSObjectReference
{
    /// <summary>
    /// Every module call so far, in order: the function identifier and its arguments.
    /// </summary>
    public List<(string Identifier, object?[] Args)> Invocations { get; } = new();

    /// <summary>
    /// The value every call returns, or null for default(TValue).
    /// </summary>
    public object? NextResult { get; set; }

    /// <summary>
    /// Asserts that exactly one module call was made and that it invoked an element method through
    /// "invokeMethod", and returns the method's arguments.
    /// </summary>
    /// <param name="methodName">Expected element method name</param>
    /// <returns>The arguments passed to the element method</returns>
    public object[] AssertInvokedMethod(string methodName)
    {
        var (identifier, args) = Assert.Single(Invocations);
        Assert.Equal(InvokeMethodIdentifier, identifier);
        Assert.Equal(methodName, args[MemberNameIndex]);
        return args[MethodArgumentsIndex] as object[] ?? Array.Empty<object>();
    }

    /// <summary>
    /// Asserts that exactly one module call was made and that it set an element property through "setProperty",
    /// and returns the value set.
    /// </summary>
    /// <param name="propertyName">Expected property name</param>
    /// <returns>The value passed for the property</returns>
    public object? AssertSetProperty(string propertyName)
    {
        var (identifier, args) = Assert.Single(Invocations);
        Assert.Equal(SetPropertyIdentifier, identifier);
        Assert.Equal(propertyName, args[MemberNameIndex]);
        return args[PropertyValueIndex];
    }

    /// <summary>
    /// Asserts that exactly one module call was made and that it read an element property through "getProperty".
    /// </summary>
    /// <param name="propertyName">Expected property name</param>
    public void AssertGotProperty(string propertyName)
    {
        var (identifier, args) = Assert.Single(Invocations);
        Assert.Equal(GetPropertyIdentifier, identifier);
        Assert.Equal(propertyName, args[MemberNameIndex]);
    }

    #region ------ Implementation of IJSObjectReference ------

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
    {
        Invocations.Add((identifier, args ?? Array.Empty<object?>()));
        return ValueTask.FromResult(NextResult is TValue result ? result : default(TValue)!);
    }

    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        return InvokeAsync<TValue>(identifier, args);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    #endregion

    #region ------ Internals ------

    private const string InvokeMethodIdentifier = "invokeMethod";
    private const string SetPropertyIdentifier = "setProperty";
    private const string GetPropertyIdentifier = "getProperty";

    // the module functions take the element first, then the method or property name, then its arguments or value
    private const int MemberNameIndex = 1;
    private const int MethodArgumentsIndex = 2;
    private const int PropertyValueIndex = 2;

    #endregion
}
