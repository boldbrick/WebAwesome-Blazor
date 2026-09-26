using System;
using System.Threading.Tasks;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.ApiParity;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Tests for WaRelativeTime and WaFormatDate, whose Date is a DateTimeOffset? since 3.12.0: it renders as the
/// ECMAScript date-time string with its offset (yyyy-MM-ddTHH:mm:ss.fff+hh:mm), culture-free, so new Date(text)
/// reads the same instant in every browser time zone; an unset Date renders no attribute (the element's "now").
/// </summary>
public class WaRelativeTimeIntegrationTests : BunitContext
{
    public WaRelativeTimeIntegrationTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public async Task UpdateAsync_WithNullElement_ThrowsInvalidOperationException()
    {
        // Arrange
        var component = new WaRelativeTime();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            component.UpdateAsync());

        Assert.Contains("Cannot update relative time: component has not been rendered yet", exception.Message);
    }

    [Theory]
    [InlineData(-5, "2026-01-02T03:04:05.678-05:00")]
    [InlineData(0, "2026-01-02T03:04:05.678+00:00")]
    [InlineData(5.5, "2026-01-02T03:04:05.678+05:30")]
    public void Date_RendersWithItsOffset_InAnyCulture(double offsetHours, string expected)
    {
        using var culture = new CultureScope(RenderedAttributeParityTests.HostileCulture);
        var date = new DateTimeOffset(2026, 1, 2, 3, 4, 5, 678, TimeSpan.FromHours(offsetHours));

        var relative = Render<WaRelativeTime>(p => p.Add(c => c.Date, date));
        var formatted = Render<WaFormatDate>(p => p.Add(c => c.Date, date));

        Assert.Equal(expected, relative.Find("wa-relative-time").GetAttribute("date"));
        Assert.Equal(expected, formatted.Find("wa-format-date").GetAttribute("date"));
    }

    [Fact]
    public void UtcDateTime_ConvertsToItsInstant()
    {
        // the implicit DateTime conversion keeps a UTC value's instant (an unspecified one would take the local offset)
        var relative = Render<WaRelativeTime>(p => p.Add(c => c.Date, new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc)));

        Assert.Equal("2026-01-02T03:04:05.000+00:00", relative.Find("wa-relative-time").GetAttribute("date"));
    }

    [Fact]
    public void UnsetDate_RendersNoAttribute()
    {
        Assert.False(Render<WaRelativeTime>().Find("wa-relative-time").HasAttribute("date"));
        Assert.False(Render<WaFormatDate>().Find("wa-format-date").HasAttribute("date"));
    }

    [Fact]
    public void StringAlternative_IsGone()
    {
        Assert.Null(typeof(WaRelativeTime).GetProperty("DateString"));
        Assert.Equal(typeof(DateTimeOffset?), typeof(WaRelativeTime).GetProperty(nameof(WaRelativeTime.Date))!.PropertyType);
        Assert.Equal(typeof(DateTimeOffset?), typeof(WaFormatDate).GetProperty(nameof(WaFormatDate.Date))!.PropertyType);
    }
}
