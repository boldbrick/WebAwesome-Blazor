using Bunit;
using Xunit;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// The checked-state read-back of WaCheckbox and WaSwitch under bUnit: on change they read the element's
/// "checked" property back through the interop module's "getProperty". In JSRuntimeMode.Loose an invocation no
/// setup matches returns default(false), so a read-back of the wrong property (or none) would still pass every
/// test that expects false. SetupCheckedReadBack therefore switches the JS runtime to Strict, where any other call
/// fails the test, and VerifyCheckedReadBack asserts the call that was made.
/// </summary>
internal static class CheckedReadBack
{
    /// <summary>
    /// Switches the bUnit JS runtime to Strict and sets up the interop module so that reading the element's
    /// "checked" property returns the given state; any other JS call fails the test.
    /// </summary>
    /// <param name="jsInterop">The bUnit JS interop of the test context</param>
    /// <param name="elementChecked">The checked state the element reports</param>
    /// <returns>The module interop, for VerifyCheckedReadBack</returns>
    public static BunitJSModuleInterop SetupCheckedReadBack(this BunitJSInterop jsInterop, bool elementChecked)
    {
        jsInterop.Mode = JSRuntimeMode.Strict;
        var module = jsInterop.SetupModule(InteropModulePath);
        module.Setup<bool>(GetPropertyIdentifier, i => Equals(i.Arguments[PropertyNameIndex], CheckedProperty)).SetResult(elementChecked);
        return module;
    }

    /// <summary>
    /// Asserts that the wrapper read the element's "checked" property back exactly once through "getProperty".
    /// </summary>
    /// <param name="module">The module interop returned by SetupCheckedReadBack</param>
    public static void VerifyCheckedReadBack(this BunitJSModuleInterop module)
    {
        var readBack = Assert.Single(module.Invocations, i => i.Identifier == GetPropertyIdentifier);
        Assert.Equal(CheckedProperty, readBack.Arguments[PropertyNameIndex]);
    }

    #region ------ Internals ------

    private const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";
    private const string GetPropertyIdentifier = "getProperty";
    private const string CheckedProperty = "checked";

    // getProperty(element, propertyName)
    private const int PropertyNameIndex = 1;

    #endregion
}
