using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A stepper component that displays the steps of a process. Not a form control.
/// Corresponds to the wa-stepper Web Awesome component.
/// </summary>
public class WaStepper : ComponentBase
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

    // Stepper properties
    /// <summary>
    /// The Web Awesome default of <see cref="Active"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultActive = "";

    /// <summary>
    /// The name of the active step. Falls back to the first step if unset, or if it doesn't match any step's name.
    /// </summary>
    /// <remarks>
    /// The element changes this itself: through <see cref="GoToAsync"/>/<see cref="NextAsync"/>/<see cref="PreviousAsync"/>,
    /// a click on a clickable step, or a <c>data-stepper</c> invoker, like <see cref="WaTabGroup.Active"/>. Re-rendering
    /// this component with the same value after the user has moved on does not move the stepper back; call
    /// <see cref="GoToAsync"/> or track the current step from <see cref="OnStepChange"/> instead.
    /// </remarks>
    [Parameter] public string? Active { get; set; }

    /// <summary>
    /// Allows clicking a step, or focusing it and pressing Enter/Space, to jump straight to it. When unset (the
    /// default), only <see cref="GoToAsync"/>/<see cref="NextAsync"/>/<see cref="PreviousAsync"/> or a <c>data-stepper</c>
    /// invoker change the active step.
    /// </summary>
    [Parameter] public bool Clickable { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Label"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLabel = "";

    /// <summary>
    /// A label that describes the stepper to assistive devices. Especially useful when more than one is on the page.
    /// </summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>
    /// Requires steps to be completed in order. When set, advancing past a step that is not yet completed is blocked,
    /// whether requested through <see cref="GoToAsync"/>/<see cref="NextAsync"/>, a <c>data-stepper</c> invoker or,
    /// when <see cref="Clickable"/> is also set, a click.
    /// </summary>
    [Parameter] public bool Linear { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Orientation"/>, which renders no attribute until the parameter first
    /// differs from it.
    /// </summary>
    public const WaStepperOrientation DefaultOrientation = WaStepperOrientation.Horizontal;

    /// <summary>
    /// The stepper's layout direction.
    /// </summary>
    [Parameter] public WaStepperOrientation? Orientation { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked before the active step changes.
    /// </summary>
    /// <remarks>
    /// Upstream the event is cancelable via <c>event.preventDefault()</c>, but Blazor dispatches custom event
    /// callbacks asynchronously after the DOM event has already run its course, so a .NET handler cannot call
    /// back into the DOM synchronously to prevent the step change. This event is delivered as an informational
    /// notification only.
    /// </remarks>
    [Parameter] public EventCallback<WaStepChangeEventArgs> OnBeforeStepChange { get; set; }

    /// <summary>
    /// Invoked after the active step changes.
    /// </summary>
    [Parameter] public EventCallback<WaStepChangeEventArgs> OnStepChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The stepper's content (one or more WaStep children).
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-stepper");

        // Add common attributes
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttributeIfNotNullOrEmpty(2, "class", GetCombinedCssClass());
        builder.AddAttributeIfNotNullOrEmpty(3, "style", Style);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 4, "active", Active, DefaultActive);
        builder.AddAttribute(5, "clickable", Clickable);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 6, "label", Label, DefaultLabel);
        builder.AddAttribute(7, "linear", Linear);
        builder.AddAttributeIfNotNull(attributes, 8, "orientation", Orientation?.ToHtmlValue(), DefaultOrientation.ToHtmlValue());

        // Add event handlers; own wa-* events are consumed here, so a nested stepper (e.g. inside a
        // step's description) never reaches an outer wrapper's callback of the same name
        builder.AddAttributeIfHasDelegate(11, "onwa-before-step-change", OnBeforeStepChange);
        builder.AddAttributeIfHasDelegate(12, "onwa-step-change", OnStepChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(10, __stepperReference => Element = __stepperReference);

        // Add child content (WaStep children)
        if (ChildContent is not null)
        {
            builder.AddContent(20, ChildContent);
        }

        builder.CloseElement();
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Requests a change to the named step. Emits <see cref="OnBeforeStepChange"/>; if it is not canceled upstream,
    /// updates <c>active</c> and emits <see cref="OnStepChange"/>. No-ops silently if the step doesn't exist, is
    /// disabled, or (in <see cref="Linear"/> mode) isn't reachable yet.
    /// </summary>
    /// <param name="name">The name of the step to activate</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null or empty</exception>
    public async Task GoToAsync(string name)
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot go to step: component has not been rendered yet.");

        if (string.IsNullOrEmpty(name))
            throw new ArgumentNullException(nameof(name));

        await JSInterop.InvokeMethodAsync(Element.Value, "goTo", name);
    }

    /// <summary>
    /// Advances to the step after the active one, if any.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task NextAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot go to next step: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "next");
    }

    /// <summary>
    /// Goes back to the step before the active one, if any.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task PreviousAsync()
    {
        if (Element == null)
            throw new InvalidOperationException("Cannot go to previous step: component has not been rendered yet.");

        await JSInterop.InvokeMethodAsync(Element.Value, "previous");
    }

    #endregion

    #region ------ Internals ------

    [Inject] private WebAwesomeJSInterop JSInterop { get; set; } = default!;

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
