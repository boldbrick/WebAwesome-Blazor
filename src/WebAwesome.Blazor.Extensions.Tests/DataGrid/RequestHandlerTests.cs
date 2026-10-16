using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebAwesome.Blazor.Extensions.DataGrid;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// WaDataGridRequestHandler answers ctx.invoke: it wraps its grids in one element without a layout box, registers a
/// DotNetObjectReference for that element in the browser after its first render, answers each request with the value of
/// OnRequest (or the message of a WaDataGridRequestException), and releases the reference on disposal, also when the
/// circuit is gone. bUnit observes the interop calls; the browser part is in tools\e2e\tests\cell-requests.spec.js.
/// </summary>
public class RequestHandlerTests : BunitContext
{
    public RequestHandlerTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        module = JSInterop.SetupModule(ModulePath);
    }

    [Fact]
    public void Render_WrapsTheContentInOneElementWithoutALayoutBox()
    {
        var cut = Render<WaDataGridRequestHandler>(parameters => parameters
            .AddChildContent("<p class=\"grid\">grid</p>"));

        var scope = cut.Find("div");
        Assert.Equal("display: contents", scope.GetAttribute("style"));
        Assert.Single(scope.Children);
        Assert.Equal("grid", scope.QuerySelector("p.grid")!.TextContent);
    }

    [Fact]
    public void FirstRender_RegistersOneReferenceForItsElement()
    {
        var cut = Render<WaDataGridRequestHandler>();
        cut.Render();

        var registration = Assert.Single(module.Invocations[RegisterFunctionName]);
        Assert.IsType<ElementReference>(registration.Arguments[0]);
        Assert.NotNull(ReceiverOf(registration.Arguments[1]!));
    }

    [Fact]
    public async Task Request_ResolvesWithTheValueOfTheHandler()
    {
        WaDataGridCellActionEventArgs? received = null;
        Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, request =>
            {
                received = request;
                return Task.FromResult<object?>(new { price = 12.5m, currency = "EUR" });
            }));

        var response = await InvokeAsync(Request("price", """{"id":7}"""));

        Assert.Equal("""{"value":{"price":12.5,"currency":"EUR"},"error":null}""", response);
        Assert.Equal("price", received!.Action);
        Assert.Equal("total", received.Column);
        Assert.Equal(7, received.RowKey!.Value.GetInt32());
        Assert.Equal(7, received.GetPayload<JsonElement>().GetProperty("id").GetInt32());
    }

    [Fact]
    public async Task Request_WithNullResult_ResolvesWithNull()
    {
        Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, _ => Task.FromResult<object?>(null)));

        Assert.Equal("""{"value":null,"error":null}""", await InvokeAsync(Request("noop")));
    }

    [Fact]
    public async Task Request_WithRequestException_RejectsWithItsMessage()
    {
        Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, _ => throw new WaDataGridRequestException("Out of stock", new InvalidOperationException("internal detail"))));

        Assert.Equal("""{"value":null,"error":"Out of stock"}""", await InvokeAsync(Request("reserve")));
    }

    [Fact]
    public async Task Request_WithAnotherException_LeavesItToTheBlazorInterop()
    {
        // Blazor rejects the call with its own message and logs the exception; the browser gets no internal detail
        Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, _ => throw new InvalidOperationException("internal detail")));

        await Assert.ThrowsAsync<InvalidOperationException>(() => InvokeAsync(Request("reserve")));
    }

    [Fact]
    public async Task Request_WithoutOnRequest_RejectsWithAClearMessage()
    {
        Render<WaDataGridRequestHandler>();

        var response = JsonDocument.Parse(await InvokeAsync(Request("price"))).RootElement;

        Assert.Equal(JsonValueKind.Null, response.GetProperty("value").ValueKind);
        Assert.Equal("WebAwesome.Blazor.Extensions: the request 'price' reached a WaDataGridRequestHandler without OnRequest.", response.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Request_UsesTheCurrentHandler()
    {
        var cut = Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, _ => Task.FromResult<object?>("first")));
        cut.Render(parameters => parameters.Add(p => p.OnRequest, _ => Task.FromResult<object?>("second")));

        Assert.Equal("""{"value":"second","error":null}""", await InvokeAsync(Request("which")));
        Assert.Single(module.Invocations[RegisterFunctionName]);
    }

    [Fact]
    public async Task ConcurrentRequests_GetTheirOwnResults()
    {
        var first = new TaskCompletionSource<object?>();
        var second = new TaskCompletionSource<object?>();
        Render<WaDataGridRequestHandler>(parameters => parameters
            .Add(p => p.OnRequest, request => request.Action == "first" ? first.Task : second.Task));

        var firstResponse = InvokeAsync(Request("first"));
        var secondResponse = InvokeAsync(Request("second"));
        second.SetResult(2);
        first.SetResult(1);

        Assert.Equal("""{"value":1,"error":null}""", await firstResponse);
        Assert.Equal("""{"value":2,"error":null}""", await secondResponse);
    }

    [Fact]
    public async Task Dispose_UnregistersTheElementAndReleasesTheReference()
    {
        Render<WaDataGridRequestHandler>();
        var registration = Assert.Single(module.Invocations[RegisterFunctionName]);

        await DisposeComponentsAsync();

        var unregistration = Assert.Single(module.Invocations[UnregisterFunctionName]);
        Assert.Equal(registration.Arguments[0], unregistration.Arguments[0]);
        Assert.Same(registration.Arguments[1], unregistration.Arguments[1]);
        AssertReleased(registration.Arguments[1]!);
    }

    [Fact]
    public async Task Dispose_AfterADisconnect_ReleasesTheReferenceWithoutAnError()
    {
        module.SetupVoid(UnregisterFunctionName, _ => true).SetException(new JSDisconnectedException("The circuit is gone."));
        Render<WaDataGridRequestHandler>();
        var registration = Assert.Single(module.Invocations[RegisterFunctionName]);

        await DisposeComponentsAsync();

        AssertReleased(registration.Arguments[1]!);
    }

    [Fact]
    public async Task Dispose_WhenTheRegistrationFailedOnADisconnect_DoesNotThrow()
    {
        module.SetupVoid(RegisterFunctionName, _ => true).SetException(new JSDisconnectedException("The circuit is gone."));
        Render<WaDataGridRequestHandler>();

        await DisposeComponentsAsync();

        Assert.Empty(module.Invocations[UnregisterFunctionName]);
    }

    #region ------ Internals ------

    private const string ModulePath = "./_content/WebAwesome.Blazor.Extensions/data-grid-requests.js";
    private const string RegisterFunctionName = "registerRequestHandler";
    private const string UnregisterFunctionName = "unregisterRequestHandler";
    private const string InvokeMethodName = "InvokeCellRequestAsync";

    private readonly BunitJSModuleInterop module;

    private static WaDataGridCellActionEventArgs Request(string action, string? payload = null)
    {
        return new WaDataGridCellActionEventArgs
        {
            Column = "total",
            Action = action,
            RowKey = JsonSerializer.SerializeToElement(7),
            Payload = payload is null ? null : JsonDocument.Parse(payload).RootElement.Clone(),
            RowIndex = 0,
        };
    }

    // the receiver behind the registered DotNetObjectReference (an internal type, read by reflection: the tests also run
    // against the Release build, which has no InternalsVisibleTo)
    private static object ReceiverOf(object reference)
    {
        return reference.GetType().GetProperty("Value")!.GetValue(reference)!;
    }

    private static void AssertReleased(object reference)
    {
        var exception = Assert.Throws<TargetInvocationException>(() => ReceiverOf(reference));
        Assert.IsType<ObjectDisposedException>(exception.InnerException);
    }

    // calls the [JSInvokable] method as the browser does and returns the response as the interop writes it
    private async Task<string> InvokeAsync(WaDataGridCellActionEventArgs request)
    {
        var registration = module.Invocations[RegisterFunctionName].Single();
        var receiver = ReceiverOf(registration.Arguments[1]!);
        var method = receiver.GetType().GetMethods()
            .Single(m => m.GetCustomAttribute<JSInvokableAttribute>()?.Identifier == InvokeMethodName);

        var task = (Task)method.Invoke(receiver, [request])!;
        await task;
        var response = task.GetType().GetProperty("Result")!.GetValue(task);
        return JsonSerializer.Serialize(response, JsonSerializerOptions.Web);
    }

    #endregion
}
