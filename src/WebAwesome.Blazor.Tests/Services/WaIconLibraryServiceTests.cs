using System;
using System.Threading.Tasks;
using WebAwesome.Blazor.Services;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Models;
using Xunit;
using Moq;

namespace WebAwesome.Blazor.Tests.Services;

/// <summary>
/// Unit tests for WaIconLibraryService
/// </summary>
public class WaIconLibraryServiceTests
{
    private readonly Mock<WebAwesomeJSInterop> mockJSInterop;
    private readonly WaIconLibraryService service;

    public WaIconLibraryServiceTests()
    {
        mockJSInterop = new Mock<WebAwesomeJSInterop>(new Mock<Microsoft.JSInterop.IJSRuntime>().Object);
        service = new WaIconLibraryService(mockJSInterop.Object);
    }

    [Fact]
    public void Constructor_WithNullJSInterop_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new WaIconLibraryService(null!));
    }

    [Fact]
    public async Task RegisterFontAwesomeProAsync_WithValidKitCode_CallsJSInterop()
    {
        // Arrange
        const string kitCode = "test-kit-123";

        // Act
        await service.RegisterFontAwesomeProAsync(kitCode);

        // Assert
        mockJSInterop.Verify(x => x.RegisterIconLibraryAsync("fa-pro", It.Is<IconLibraryOptions>(
            opts => opts.Resolver == $"https://kit.fontawesome.com/{kitCode}/{{family}}/{{variant}}/{{name}}.svg")), Times.Once);
    }

    [Fact]
    public async Task RegisterFontAwesomeProAsync_WithNullKitCode_ThrowsArgumentNullException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            service.RegisterFontAwesomeProAsync(null!));
    }

    [Fact]
    public async Task RegisterHeroiconsAsync_WithDefaultVersion_CallsJSInterop()
    {
        // Act
        await service.RegisterHeroiconsAsync();

        // Assert
        mockJSInterop.Verify(x => x.RegisterIconLibraryAsync("heroicons", It.Is<IconLibraryOptions>(
            opts => opts.Resolver == "https://cdn.jsdelivr.net/npm/heroicons@2.0.18/24/{variant}/{name}.svg")), Times.Once);
    }

    [Fact]
    public async Task RegisterLucideAsync_WithCustomVersion_CallsJSInterop()
    {
        // Arrange
        const string version = "0.294.0";

        // Act
        await service.RegisterLucideAsync(version);

        // Assert
        mockJSInterop.Verify(x => x.RegisterIconLibraryAsync("lucide", It.Is<IconLibraryOptions>(
            opts => opts.Resolver == $"https://cdn.jsdelivr.net/npm/lucide@{version}/icons/{{name}}.svg")), Times.Once);
    }

    [Fact]
    public async Task RegisterIconLibraryAsync_CallsUnderlyingJSInterop()
    {
        // Arrange
        const string name = "custom-lib";
        var options = new IconLibraryOptions { Resolver = "https://example.com/{name}.svg" };

        // Act
        await service.RegisterIconLibraryAsync(name, options);

        // Assert
        mockJSInterop.Verify(x => x.RegisterIconLibraryAsync(name, options), Times.Once);
    }

    [Fact]
    public async Task UnregisterIconLibraryAsync_CallsUnderlyingJSInterop()
    {
        // Arrange
        const string name = "custom-lib";

        // Act
        await service.UnregisterIconLibraryAsync(name);

        // Assert
        mockJSInterop.Verify(x => x.UnregisterIconLibraryAsync(name), Times.Once);
    }

    [Fact]
    public async Task SetDefaultIconFamilyAsync_CallsUnderlyingJSInterop()
    {
        // Arrange
        const string family = "sharp";

        // Act
        await service.SetDefaultIconFamilyAsync(family);

        // Assert
        mockJSInterop.Verify(x => x.SetDefaultIconFamilyAsync(family), Times.Once);
    }

    [Fact]
    public async Task GetDefaultIconFamilyAsync_CallsUnderlyingJSInterop()
    {
        // Arrange
        const string expectedFamily = "classic";
        mockJSInterop.Setup(x => x.GetDefaultIconFamilyAsync()).ReturnsAsync(expectedFamily);

        // Act
        var result = await service.GetDefaultIconFamilyAsync();

        // Assert
        Assert.Equal(expectedFamily, result);
        mockJSInterop.Verify(x => x.GetDefaultIconFamilyAsync(), Times.Once);
    }
}