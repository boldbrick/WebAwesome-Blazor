using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A single step of a WaStepper. Corresponds to the wa-step Web Awesome component.
/// </summary>
/// <remarks>
/// The CEM class name of wa-step is "WaStep", but <see cref="WaStep"/> is already the public
/// <c>number | 'any'</c> step value used by <c>WaInput</c>, <c>WaNumberInput</c>, <c>WaTimeInput</c> and the
/// sliders. Renaming that struct would break every consumer spelling it, so the wrapper of wa-step is named
/// <c>WaStepperStep</c> instead; it reads naturally inside its only valid parent, <c>&lt;WaStepper&gt;</c>.
/// </remarks>
public class WaStepperStep : ComponentBase
{
    #region ------ Public Properties ------

    /// <summary>
    /// The associated <see cref="ElementReference"/>.
    /// <para>
    /// May be null if accessed before the component is rendered.
    /// </para>
    /// </summary>
    [DisallowNull] public ElementReference? Element { get; protected set; }

    /// <summary>
    /// A collection of additional attributes that will be applied to the created element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    /// <summary>
    /// Additional CSS class names applied to the rendered element.
    /// </summary>
    [Parameter] public string? Class { get; set; }

    /// <summary>
    /// Inline CSS style applied to the rendered element.
    /// </summary>
    [Parameter] public string? Style { get; set; }

    // Step properties
    /// <summary>
    /// The Web Awesome default of <see cref="Name"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultName = "";

    /// <summary>
    /// Identifies the step. Matched against the parent stepper's active step and used in events.
    /// </summary>
    [Parameter] public string? Name { get; set; }

    /// <summary>
    /// Draws the step as the stepper's current step. The parent WaStepper sets this itself from its own
    /// <see cref="WaStepper.Active"/>, so you only need to set it yourself for server-side pre-rendering.
    /// </summary>
    [Parameter] public bool Active { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Attention"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaAttention DefaultAttention = WaAttention.None;

    /// <summary>
    /// Adds an animation to the step's marker to draw attention to it, e.g. the step the user should do next.
    /// </summary>
    [Parameter] public WaAttention? Attention { get; set; }

    /// <summary>
    /// Marks the step done. Shows a checkmark instead of the step number.
    /// </summary>
    [Parameter] public bool Completed { get; set; }

    /// <summary>
    /// Makes the step non-interactive. It can't be clicked or reached with next()/goTo(), and it renders as a
    /// disabled button when the stepper is clickable.
    /// </summary>
    [Parameter] public bool Disabled { get; set; }

    /// <summary>
    /// Shows a loading indicator instead of the step number, e.g. while an async transition is in progress.
    /// </summary>
    [Parameter] public bool Loading { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Variant"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaVariant DefaultVariant = WaVariant.Brand;

    /// <summary>
    /// Colors the step's marker with a semantic color. Upcoming brand steps keep a neutral outline so a default
    /// stepper reads quietly. The color is cosmetic; pair it with an icon in <see cref="IconContent"/> and a clear
    /// label when a step needs to read as failed or flagged.
    /// </summary>
    [Parameter] public WaVariant? Variant { get; set; }

    /// <summary>
    /// Only required for server-side pre-rendering. Set to true if you're slotting in <see cref="DescriptionContent"/>,
    /// so the server-rendered markup includes it before the component hydrates on the client.
    /// </summary>
    [Parameter] public bool WithDescription { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The step's label.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Optional text shown under the label.
    /// </summary>
    [Parameter] public RenderFragment? DescriptionContent { get; set; }

    /// <summary>
    /// An element, such as a wa-icon, that replaces the step number, checkmark, or loading indicator.
    /// </summary>
    [Parameter] public RenderFragment? IconContent { get; set; }

    /// <summary>
    /// Convenience alternative to <see cref="IconContent"/>; ignored when the fragment is set.
    /// </summary>
    [Parameter] public string? IconName { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-step");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 4, "name", Name, DefaultName);
        builder.AddAttribute(5, "active", Active);
        builder.AddAttributeIfNotNull(attributes, 6, "attention", Attention?.ToHtmlValue(), DefaultAttention.ToHtmlValue());
        builder.AddAttribute(7, "completed", Completed);
        builder.AddAttribute(8, "disabled", Disabled);
        builder.AddAttribute(9, "loading", Loading);
        builder.AddAttributeIfNotNull(attributes, 10, "variant", Variant?.ToHtmlValue(), DefaultVariant.ToHtmlValue());
        builder.AddAttribute(11, "with-description", WithDescription);

        // Add element reference capture
        builder.AddElementReferenceCapture(15, __stepReference => Element = __stepReference);

        // Add icon slot content (icon-shaped slot: fragment wins, icon name is the convenience fallback)
        if (IconContent is not null)
        {
            builder.OpenElement(20, "span");
            builder.AddAttribute(21, "slot", "icon");
            builder.AddContent(22, IconContent);
            builder.CloseElement();
        }
        else
        {
            builder.AddIconSlot(23, "icon", IconName);
        }

        // Add description slot content
        if (DescriptionContent is not null)
        {
            builder.OpenElement(25, "span");
            builder.AddAttribute(26, "slot", "description");
            builder.AddContent(27, DescriptionContent);
            builder.CloseElement();
        }

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(30, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Gets the CSS class string combining user classes
    /// </summary>
    private string GetCombinedCssClass()
    {
        var classes = new List<string>();

        if (!string.IsNullOrEmpty(Class))
            classes.Add(Class);

        return string.Join(' ', classes);
    }

    #endregion
}
