using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// OpenWaElement writes the WA Inspector placement metadata for a wrapper type outside WebAwesome.Blazor as for its own
/// wrappers. The metadata switch is process-wide, so these tests run in a collection of their own that never runs in
/// parallel with other tests.
/// </summary>
[Collection(SisterSetInspectorMetadataCollection.Name)]
public sealed class SisterSetInspectorMetadataTests : BunitContext
{
    [Fact]
    public void OpenWaElement_WritesTheMetadataOfAnOutsideWrapper()
    {
        Services.AddWebAwesome(options => options.EmitInspectorMetadata = true);
        JSInterop.Mode = JSRuntimeMode.Loose;

        var cut = Render<XyzChip>();

        var chip = cut.Find(XyzChip.TagName);
        Assert.Equal(nameof(XyzChip), chip.GetAttribute(ComponentAttribute));
        Assert.EndsWith(nameof(XyzChip), chip.GetAttribute(ChainAttribute));
        Assert.Matches(HashPattern, chip.GetAttribute(PlacementAttribute));
        Assert.Matches(HashPattern, chip.GetAttribute(SiteAttribute));
    }

    #region ------ Internals ------

    private const string ComponentAttribute = "data-wablazor-component";
    private const string ChainAttribute = "data-wablazor-chain";
    private const string PlacementAttribute = "data-wablazor-placement";
    private const string SiteAttribute = "data-wablazor-site";
    private const string HashPattern = "^[0-9a-f]{12}$";

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        // the switch is process-wide: leave it off for the other tests
        new ServiceCollection().AddWebAwesome();
        base.Dispose(disposing);
    }

    #endregion
}

/// <summary>
/// The test collection of <see cref="SisterSetInspectorMetadataTests"/>: it runs alone, because the metadata switch is
/// process-wide and other tests compare the rendered attributes.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class SisterSetInspectorMetadataCollection
{
    /// <summary>
    /// The collection name.
    /// </summary>
    public const string Name = "Sister set inspector metadata";
}
