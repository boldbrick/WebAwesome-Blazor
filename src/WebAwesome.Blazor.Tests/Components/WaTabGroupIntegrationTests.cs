using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for WaTabGroup.ShowTabAsync: it rejects a missing panel name, throws before the first render,
/// and afterwards sets the element's active property to the panel through the interop module (recorded, see
/// RecordingJSRuntime).
/// </summary>
public class WaTabGroupIntegrationTests : IDisposable
{
    [Fact]
    public async Task ShowTabAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaTabGroup>().ShowTabAsync(PanelName));

        Assert.Contains("Cannot show tab: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task ShowTabAsync_WithNullPanelName_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaTabGroup>().ShowTabAsync(null!));
    }

    [Fact]
    public async Task ShowTabAsync_WithEmptyPanelName_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            runtime.CreateRendered<WaTabGroup>().ShowTabAsync(string.Empty));
    }

    [Fact]
    public async Task ShowTabAsync_WithValidElement_SetsTheActiveProperty()
    {
        await runtime.CreateRendered<WaTabGroup>().ShowTabAsync(PanelName);

        Assert.Equal(PanelName, runtime.Module.AssertSetProperty("active"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const string PanelName = "panel1";

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
