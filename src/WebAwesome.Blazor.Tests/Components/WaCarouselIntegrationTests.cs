using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Interop tests for the WaCarousel navigation methods: before the first render they throw, afterwards each
/// invokes its element method with its argument through the interop module (recorded, see RecordingJSRuntime).
/// </summary>
public class WaCarouselIntegrationTests : IDisposable
{
    [Fact]
    public async Task GoToSlideAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaCarousel>().GoToSlideAsync(1));

        Assert.Contains("Cannot navigate to slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task PreviousAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaCarousel>().PreviousAsync());

        Assert.Contains("Cannot navigate to previous slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task NextAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            runtime.CreateUnrendered<WaCarousel>().NextAsync());

        Assert.Contains("Cannot navigate to next slide: component has not been rendered yet", exception.Message);
    }

    [Fact]
    public async Task GoToSlideAsync_WithValidElement_InvokesGoToSlideWithTheIndex()
    {
        await runtime.CreateRendered<WaCarousel>().GoToSlideAsync(SlideIndex);

        Assert.Equal(new object[] { SlideIndex }, runtime.Module.AssertInvokedMethod("goToSlide"));
    }

    [Fact]
    public async Task PreviousAsync_WithValidElement_InvokesPrevious()
    {
        await runtime.CreateRendered<WaCarousel>().PreviousAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("previous"));
    }

    [Fact]
    public async Task NextAsync_WithValidElement_InvokesNext()
    {
        await runtime.CreateRendered<WaCarousel>().NextAsync();

        Assert.Empty(runtime.Module.AssertInvokedMethod("next"));
    }

    #region ------ Implementation of IDisposable ------

    public void Dispose()
    {
        runtime.Dispose();
    }

    #endregion

    #region ------ Internals ------

    private const int SlideIndex = 2;

    private readonly RecordingJSRuntime runtime = new();

    #endregion
}
