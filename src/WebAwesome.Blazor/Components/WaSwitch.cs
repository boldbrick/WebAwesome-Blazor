using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A switch component that allows the user to toggle an option on or off.
/// Corresponds to the wa-switch Web Awesome component.
/// </summary>
public class WaSwitch : WaInputBase<bool>
{
    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// Gets or sets whether the switch is checked (mirrors CurrentValue for convenience)
    /// </summary>
    [Parameter]
    public bool Checked
    {
        get => CurrentValue;
        set => CurrentValue = value;
    }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Invoked when the switch's checked state changes.
    /// </summary>
    [Parameter] public EventCallback<bool> OnCheckedChange { get; set; }

    #endregion

    #region ------ Content ------

    /// <summary>
    /// The content to display next to the switch (typically the label text)
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "wa-switch");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add switch-specific attributes
        builder.AddAttribute(20, "checked", BindConverter.FormatValue(CurrentValue));

        // Include the "value" attribute so that when this is posted by a form, "true"
        // is included in the form fields. That's how <input type="checkbox"> works normally.
        builder.AddAttribute(21, "value", bool.TrueString);

        // Add value binding
        builder.AddAttribute(22, "onchange", EventCallback.Factory.CreateBinder<bool>(this, __value => CurrentValue = __value, CurrentValue));
        builder.SetUpdatesAttributeName("checked");

        // Add common event handlers
        AddCommonEventHandlers(builder, 30);

        // Add switch-specific event handlers
        if (OnCheckedChange.HasDelegate)
            builder.AddAttribute(40, "wa-change", OnCheckedChange);

        // Add element reference capture
        builder.AddElementReferenceCapture(41, __switchReference => Element = __switchReference);

        // Add child content (label)
        if (ChildContent is not null)
        {
            builder.AddContent(50, ChildContent);
        }

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 60);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out bool result, [NotNullWhen(false)] out string? validationErrorMessage)
        => throw new NotSupportedException($"This component does not parse string inputs. Bind to the '{nameof(CurrentValue)}' property, not '{nameof(CurrentValueAsString)}'.");

    #endregion

}
