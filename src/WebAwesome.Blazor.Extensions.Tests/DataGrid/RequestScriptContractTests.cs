using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.JSInterop;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// ctx.invoke needs four coordinated pieces, and a mismatch fails only in the browser: the module that
/// WaDataGridRequestHandler imports and calls (data-grid-requests.js with registerRequestHandler and
/// unregisterRequestHandler), the name of the [JSInvokable] method that the module calls, the shared state of the
/// handlers, and the ctx decorator that sends the request with the payload of a cell action.
/// </summary>
public class RequestScriptContractTests
{
    [Fact]
    public void Module_IsAtThePathTheComponentImports()
    {
        var component = ExtensionsPackage.Assembly.GetType(ComponentTypeName, throwOnError: true)!;
        var path = (string)component.GetField("ModulePath", BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;

        Assert.Equal($"./_content/{ExtensionsPackage.AssemblyName}/{PresetScripts.RequestsFileName}", path);
        Assert.True(File.Exists(Path.Combine(ExtensionsPackage.ProjectDirectory(), "wwwroot", PresetScripts.RequestsFileName)));
    }

    [Theory]
    [InlineData("RegisterFunctionName", "registerRequestHandler(element, handler)")]
    [InlineData("UnregisterFunctionName", "unregisterRequestHandler(element, handler)")]
    public void Module_ExportsTheFunctionsTheComponentCalls(string constantName, string signature)
    {
        var component = ExtensionsPackage.Assembly.GetType(ComponentTypeName, throwOnError: true)!;
        var name = (string)component.GetField(constantName, BindingFlags.NonPublic | BindingFlags.Static)!.GetRawConstantValue()!;

        Assert.StartsWith(name + "(", signature);
        Assert.Contains($"export function {signature} {{", PresetScripts.Read(PresetScripts.RequestsFileName));
    }

    [Fact]
    public void Module_CallsTheInvokableMethodOfTheReceiver()
    {
        var receiver = ExtensionsPackage.Assembly.GetType(ReceiverTypeName, throwOnError: true)!;
        var invokable = Assert.Single(receiver.GetMethods(), method => method.GetCustomAttribute<JSInvokableAttribute>() is not null);
        var requests = PresetScripts.Read(PresetScripts.RequestsFileName);
        var name = Regex.Match(requests, @"export const invokeMethodName = '([^']+)';");

        Assert.True(name.Success, "data-grid-requests.js does not export invokeMethodName");
        Assert.Equal(invokable.GetCustomAttribute<JSInvokableAttribute>()!.Identifier, name.Groups[1].Value);
        Assert.Contains("handler.invokeMethodAsync(invokeMethodName, request)", requests);
    }

    [Fact]
    public void Handlers_AreSharedStateOnAGlobalSymbol()
    {
        var requests = PresetScripts.Read(PresetScripts.RequestsFileName);

        Assert.Contains("Symbol.for('WebAwesome.Blazor.Extensions.cellRequests')", requests);
    }

    [Fact]
    public void Context_InvokeSendsThePayloadOfACellAction()
    {
        var context = PresetScripts.Read(PresetScripts.ContextFileName);

        Assert.Matches(@"import \{ invokeCellRequest \} from '\./data-grid-requests\.js';", context);
        Assert.Matches(@"invoke: \{\s*value: function invoke\(action, payload, source\) \{", context);
        Assert.Contains("return invokeCellRequest(grid, cellActionPayload({", context);
    }

    #region ------ Internals ------

    private const string ComponentTypeName = "WebAwesome.Blazor.Extensions.DataGrid.WaDataGridRequestHandler";
    private const string ReceiverTypeName = "WebAwesome.Blazor.Extensions.DataGrid.CellRequestReceiver";

    #endregion
}
