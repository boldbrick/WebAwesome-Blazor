using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using Xunit;

// BL0006: the stopPropagation directives exist only as render tree frames, which the tests read on purpose
#pragma warning disable BL0006

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// Proves the public extension API of WebAwesome.Blazor with a test-only sister wrapper set: the wrappers in this folder
/// use only public members, and this test project has no InternalsVisibleTo from WebAwesome.Blazor (it has one from
/// the Extensions package only, in Debug builds), so the Release build proves that nothing internal is needed.
/// </summary>
public class SisterSetTests : BunitContext
{
    public SisterSetTests()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void StickyAttribute_NeverSet_RendersNothing()
    {
        var cut = Render<XyzChip>();

        Assert.False(cut.Find(XyzChip.TagName).HasAttribute("variant"));
    }

    [Fact]
    public void StickyAttribute_SetThenNull_RendersTheElementDefault()
    {
        var cut = Render<XyzChip>(p => p.Add(x => x.Variant, WaVariant.Brand));
        Assert.Equal("brand", cut.Find(XyzChip.TagName).GetAttribute("variant"));

        cut.Render(p => p.Add(x => x.Variant, null));

        // never removed again: a removed attribute would read as null in the element
        Assert.Equal(XyzChip.DefaultVariant.ToHtmlValue(), cut.Find(XyzChip.TagName).GetAttribute("variant"));
        Assert.Equal(XyzChip.DefaultVariant.ToHtmlValue(), WaAttributeMemory.Of(cut.Instance).PassedDefaults["variant"]);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void OwnEvent_StopsPropagation_WithAndWithoutADelegate(bool withDelegate)
    {
        var cut = withDelegate
            ? Render<XyzChip>(p => p
                .Add(x => x.OnRemove, (XyzRemoveEventArgs _) => { })
                .Add(x => x.OnInvalid, () => { }))
            : Render<XyzChip>();

        var attributes = RootAttributeNames(cut.Instance);

        Assert.Contains(StopPropagationPrefix + XyzChip.RemoveEventAttribute, attributes);
        Assert.Contains(StopPropagationPrefix + XyzChip.InvalidEventAttribute, attributes);
        Assert.Equal(withDelegate, attributes.Contains(XyzChip.RemoveEventAttribute));
        Assert.Equal(withDelegate, attributes.Contains(XyzChip.InvalidEventAttribute));
    }

    [Fact]
    public async Task OwnEvent_DeliversTheTypedArgs()
    {
        XyzRemoveEventArgs? received = null;
        var cut = Render<XyzChip>(p => p.Add(x => x.OnRemove, (XyzRemoveEventArgs args) => received = args));

        await cut.Find(XyzChip.TagName).TriggerEventAsync(XyzChip.RemoveEventAttribute, new XyzRemoveEventArgs { Key = "k1" });

        Assert.Equal("k1", received?.Key);
    }

    [Fact]
    public void AttributeIfHasDelegate_StopsOnlyWaEvents()
    {
        // the sister set's events need AddOwnEventIfHasDelegate: the onwa- helper leaves other prefixes alone
        var builder = new RenderTreeBuilder();
        builder.OpenElement(0, XyzChip.TagName);
        builder.AddAttributeIfHasDelegate(1, XyzChip.RemoveEventAttribute, EventCallback.Factory.Create<XyzRemoveEventArgs>(this, () => { }));
        builder.AddAttributeIfHasDelegate(2, "onwa-remove", EventCallback.Factory.Create<EventArgs>(this, () => { }));
        builder.CloseElement();

        var attributes = AttributeNames(builder);

        Assert.DoesNotContain(StopPropagationPrefix + XyzChip.RemoveEventAttribute, attributes);
        Assert.Contains(StopPropagationPrefix + "onwa-remove", attributes);
    }

    [Fact]
    public void BooleanHelpers_RenderTheirWireForms()
    {
        var cut = Render<XyzChip>(p => p
            .Add(x => x.Removable, true)
            .Add(x => x.Editable, false)
            .Add(x => x.Autocorrect, true));

        var element = cut.Find(XyzChip.TagName);
        Assert.Equal(string.Empty, element.GetAttribute("removable"));
        Assert.Equal("false", element.GetAttribute("editable"));
        Assert.Equal("on", element.GetAttribute("autocorrect"));

        cut.Render(p => p
            .Add(x => x.Removable, false)
            .Add(x => x.Editable, null)
            .Add(x => x.Autocorrect, false));

        element = cut.Find(XyzChip.TagName);
        Assert.False(element.HasAttribute("removable"));
        Assert.False(element.HasAttribute("editable"));
        Assert.Equal("off", element.GetAttribute("autocorrect"));
    }

    [Fact]
    public void DateHelpers_RenderTheirWireForms()
    {
        var cut = Render<XyzChip>(p => p
            .Add(x => x.Since, new DateOnly(2026, 2, 1))
            .Add(x => x.Start, new TimeOnly(13, 4, 5))
            .Add(x => x.Stamp, new DateTimeOffset(2026, 2, 1, 3, 4, 5, 678, TimeSpan.FromHours(1)))
            .Add(x => x.Dates, new HashSet<DateOnly> { new(2026, 2, 3), new(2026, 2, 1) })
            .Add(x => x.Days, new HashSet<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Sunday }));

        var element = cut.Find(XyzChip.TagName);
        Assert.Equal("2026-02-01", element.GetAttribute("since"));
        Assert.Equal("13:04:05", element.GetAttribute("start"));
        Assert.Equal("2026-02-01T03:04:05.678+01:00", element.GetAttribute("stamp"));
        Assert.Equal("2026-02-01 2026-02-03", element.GetAttribute("dates"));
        Assert.Equal("sun mon", element.GetAttribute("days"));
    }

    [Theory]
    [InlineData(typeof(bool?), "AddBooleanAttribute")]
    [InlineData(typeof(bool), "AddTrueFalseAttribute")]
    [InlineData(typeof(DateOnly?), "AddDateAttribute")]
    [InlineData(typeof(TimeOnly), "AddTimeAttribute")]
    [InlineData(typeof(DateTime?), "AddDateTimeOffsetAttribute")]
    [InlineData(typeof(DateTimeOffset), "AddDateTimeOffsetAttribute")]
    public void GenericPath_RefusesBooleansAndDatesAtCompileTime(Type valueType, string advisedHelper)
    {
        // the CS0619 guard is a public overload with an error-level Obsolete whose message names the helper to use
        var guard = typeof(RenderTreeBuilderExtensions).GetMethod(nameof(RenderTreeBuilderExtensions.AddAttributeIfNotNull),
            [typeof(RenderTreeBuilder), typeof(int), typeof(string), valueType]);

        var obsolete = guard?.GetCustomAttribute<ObsoleteAttribute>();
        Assert.NotNull(obsolete);
        Assert.True(obsolete.IsError);
        Assert.Contains(advisedHelper, obsolete.Message);
    }

    [Fact]
    public void SizeDefault_NonNull_RendersStickyWithIt()
    {
        var cut = Render<XyzField>(p => p.Add(x => x.ValueExpression, () => Value));
        Assert.False(cut.Find("xyz-field").HasAttribute("size"));

        cut.Render(p => p.Add(x => x.Size, WaSize.Large));
        Assert.Equal("l", cut.Find("xyz-field").GetAttribute("size"));

        cut.Render(p => p.Add(x => x.Size, null));
        Assert.Equal(XyzField.DefaultElementSize.ToHtmlValue(), cut.Find("xyz-field").GetAttribute("size"));
    }

    [Fact]
    public void SizeDefault_Null_RendersPlainly()
    {
        var cut = Render<XyzChoiceGroup>(p => p.Add(x => x.ValueExpression, () => Value));
        Assert.False(cut.Find("xyz-choice-group").HasAttribute("size"));

        cut.Render(p => p.Add(x => x.Size, WaSize.Large));
        Assert.Equal("l", cut.Find("xyz-choice-group").GetAttribute("size"));

        // no literal element default: the attribute is removed, which restores the unset state
        cut.Render(p => p.Add(x => x.Size, null));
        Assert.False(cut.Find("xyz-choice-group").HasAttribute("size"));
        Assert.False(WaAttributeMemory.Of(cut.Instance).PassedDefaults.ContainsKey("size"));
    }

    [Fact]
    public void StringArrayValue_ReadsAJsonArray()
    {
        // a custom event type delivers the array value of a multi-value control as a JSON array
        var args = new ChangeEventArgs { Value = JsonSerializer.Deserialize<JsonElement>("""["a","b",3]""") };

        Assert.Equal(["a", "b", "3"], args.GetStringArrayValue());
        Assert.Empty(new ChangeEventArgs { Value = null }.GetStringArrayValue());
    }

    #region ------ Internals ------

    // Blazor renders @onX:stopPropagation as an attribute frame with this name prefix
    private const string StopPropagationPrefix = "__internal_stopPropagation_";

    // the bound value of the form control tests
    private string? Value { get; set; }

    private static HashSet<string> RootAttributeNames(XyzChip chip)
    {
        var builder = new RenderTreeBuilder();
        chip.BuildInto(builder);
        return AttributeNames(builder);
    }

    // the attribute names of the first element in the builder's frames
    private static HashSet<string> AttributeNames(RenderTreeBuilder builder)
    {
        var frames = builder.GetFrames();
        return frames.Array.Take(frames.Count)
            .SkipWhile(frame => frame.FrameType != RenderTreeFrameType.Element)
            .Skip(1)
            .TakeWhile(frame => frame.FrameType == RenderTreeFrameType.Attribute)
            .Select(frame => frame.AttributeName)
            .ToHashSet(StringComparer.Ordinal);
    }

    #endregion
}
