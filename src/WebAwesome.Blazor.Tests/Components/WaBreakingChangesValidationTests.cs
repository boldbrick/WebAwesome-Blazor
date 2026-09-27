using WebAwesome.Blazor.Components;
using Xunit;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Removal pins for the breaking changes of the Web Awesome upgrades 3.0.0-beta.6 through 3.9.0 that took a
/// wrapper member away: re-adding one fails here even when it would render nothing (a member that rendered an
/// attribute the element no longer declares is caught by RenderedAttributeParityTests as well). The attributes
/// these upgrades added or changed (wa-icon auto-width/swap-opacity/rotate, wa-details icon-placement, wa-qr-code
/// fill/background, wa-tree selection) are covered by the CEM-driven RenderedAttributeParityTests, including
/// their defaults (BaselineRender_EmitsOnlyElementDefaults).
/// </summary>
public class WaBreakingChangesValidationTests
{
    [Fact]
    public void WaButtonGroup_DoesNotHaveSizeProperty()
    {
        Assert.Null(typeof(WaButtonGroup).GetProperty("Size"));
    }

    [Fact]
    public void WaButtonGroup_DoesNotHaveVariantProperty()
    {
        // the variant attribute was removed in Web Awesome 3.1.0
        Assert.Null(typeof(WaButtonGroup).GetProperty("Variant"));
    }

    [Fact]
    public void WaFileInput_DoesNotHaveFileIconContentProperty()
    {
        // the upstream "file-icon" slot was removed in Web Awesome 3.6.0
        Assert.Null(typeof(WaFileInput).GetProperty("FileIconContent"));
    }
}
