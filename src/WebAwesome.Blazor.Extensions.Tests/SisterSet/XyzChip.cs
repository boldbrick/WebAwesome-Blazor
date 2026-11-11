using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// A test-only wrapper of a sister wrapper set: it renders the custom element <c>xyz-chip</c> only through the public
/// extension API of WebAwesome.Blazor (this test project has no InternalsVisibleTo from WebAwesome.Blazor).
/// </summary>
public sealed class XyzChip : ComponentBase
{
    /// <summary>
    /// The tag of the wrapped element.
    /// </summary>
    public const string TagName = "xyz-chip";

    /// <summary>
    /// The event attribute of the element's own remove event.
    /// </summary>
    public const string RemoveEventAttribute = "onxyz-remove";

    /// <summary>
    /// The event attribute of the element's own invalid event, which carries no payload.
    /// </summary>
    public const string InvalidEventAttribute = "onxyz-invalid";

    /// <summary>
    /// The element default of <see cref="Variant"/>.
    /// </summary>
    public const WaVariant DefaultVariant = WaVariant.Neutral;

    /// <summary>
    /// The reference of the rendered element.
    /// </summary>
    public ElementReference? Element { get; private set; }

    /// <summary>
    /// The variant; sticky with <see cref="DefaultVariant"/>.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// A plain boolean attribute (present or absent).
    /// </summary>
    [Parameter] public bool? Removable { get; set; }

    /// <summary>
    /// An attribute that the element reads as "true"/"false".
    /// </summary>
    [Parameter] public bool? Editable { get; set; }

    /// <summary>
    /// An attribute that the element reads as "on"/"off".
    /// </summary>
    [Parameter] public bool? Autocorrect { get; set; }

    /// <summary>
    /// A date attribute.
    /// </summary>
    [Parameter] public DateOnly? Since { get; set; }

    /// <summary>
    /// A time attribute.
    /// </summary>
    [Parameter] public TimeOnly? Start { get; set; }

    /// <summary>
    /// An instant attribute.
    /// </summary>
    [Parameter] public DateTimeOffset? Stamp { get; set; }

    /// <summary>
    /// A date-list attribute.
    /// </summary>
    [Parameter] public IReadOnlySet<DateOnly>? Dates { get; set; }

    /// <summary>
    /// A weekday-list attribute.
    /// </summary>
    [Parameter] public IReadOnlySet<DayOfWeek>? Days { get; set; }

    /// <summary>
    /// Invoked by the element's own remove event.
    /// </summary>
    [Parameter] public EventCallback<XyzRemoveEventArgs> OnRemove { get; set; }

    /// <summary>
    /// Invoked by the element's own invalid event.
    /// </summary>
    [Parameter] public EventCallback OnInvalid { get; set; }

    /// <summary>
    /// The content of the default slot.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Renders the component's current render tree into a builder, so a test can read the frames, including the
    /// stopPropagation directives that the markup does not show.
    /// </summary>
    /// <param name="builder">The builder to render into</param>
    public void BuildInto(RenderTreeBuilder builder) => BuildRenderTree(builder);

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, TagName);
        builder.AddAttributeIfNotNull(attributes, 1, VariantAttribute, Variant?.ToHtmlValue(), DefaultVariant.ToHtmlValue());
        builder.AddBooleanAttribute(2, RemovableAttribute, Removable);
        builder.AddTrueFalseAttribute(3, EditableAttribute, Editable);
        builder.AddOnOffAttribute(4, AutocorrectAttribute, Autocorrect);
        builder.AddDateAttribute(5, SinceAttribute, Since);
        builder.AddTimeAttribute(6, StartAttribute, Start);
        builder.AddDateTimeOffsetAttribute(7, StampAttribute, Stamp);
        builder.AddDateSetAttribute(8, DatesAttribute, Dates);
        builder.AddDaysOfWeekAttribute(9, DaysAttribute, Days);

        builder.AddOwnEventIfHasDelegate(20, RemoveEventAttribute, OnRemove);
        builder.AddOwnEventIfHasDelegate(21, InvalidEventAttribute, OnInvalid);

        builder.AddElementReferenceCapture(30, reference => Element = reference);
        builder.AddContent(40, ChildContent);
        builder.CloseElement();
    }

    #region ------ Internals ------

    private const string VariantAttribute = "variant";
    private const string RemovableAttribute = "removable";
    private const string EditableAttribute = "editable";
    private const string AutocorrectAttribute = "autocorrect";
    private const string SinceAttribute = "since";
    private const string StartAttribute = "start";
    private const string StampAttribute = "stamp";
    private const string DatesAttribute = "dates";
    private const string DaysAttribute = "days";

    #endregion
}
