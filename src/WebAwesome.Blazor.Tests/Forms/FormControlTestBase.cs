using System;
using System.Linq.Expressions;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;

namespace WebAwesome.Blazor.Tests.Forms;

/// <summary>
/// Shared bUnit scaffolding for EditForm-based form control integration tests: registers the
/// <see cref="WebAwesomeJSInterop"/> service and exposes the interop module path used to mock JS
/// interop calls in derived tests, plus a generic single-control EditForm renderer reused across
/// the per-control test classes in this namespace. Mirrors the pattern established by
/// Base/EditFormIntegrationTests.cs for WaInput and WaCheckbox.
/// </summary>
public abstract class FormControlTestBase : TestContext
{
    /// <summary>
    /// The module path registered by <see cref="WebAwesomeJSInterop"/>, mocked via <see cref="Bunit.JSInterop"/>
    /// in derived tests that need to observe or stub interop calls.
    /// </summary>
    protected const string InteropModulePath = "./_content/WebAwesome.Blazor/webawesome-interop.js";

    protected FormControlTestBase()
    {
        Services.AddScoped<WebAwesomeJSInterop>();
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    /// <summary>
    /// Renders an EditForm wrapping a single form control bound through Value/ValueChanged/ValueExpression,
    /// with a DataAnnotationsValidator and a "user-class" CSS class applied so validation-state CSS class
    /// merging can be asserted alongside the caller's own class.
    /// </summary>
    /// <typeparam name="TComponent">The form control component type</typeparam>
    /// <typeparam name="TValue">The bound value type</typeparam>
    /// <param name="model">The model instance passed to the EditForm</param>
    /// <param name="initialValue">The initial value bound to the control</param>
    /// <param name="onChanged">Callback invoked when the control's ValueChanged fires</param>
    /// <param name="valueExpression">Expression identifying the bound model member</param>
    /// <param name="configureComponent">Optional callback to add further component parameters (e.g. Label, ChildContent)</param>
    /// <param name="onEditContext">Optional callback invoked with the EditContext captured during render</param>
    /// <returns>The rendered EditForm component</returns>
    protected IRenderedComponent<EditForm> RenderControlForm<TComponent, TValue>(
        object model,
        TValue initialValue,
        Action<TValue> onChanged,
        Expression<Func<TValue>> valueExpression,
        Action<RenderTreeBuilder>? configureComponent = null,
        Action<EditContext>? onEditContext = null)
        where TComponent : IComponent
    {
        return RenderComponent<EditForm>(parameters => parameters
            .Add(p => p.Model, model)
            .Add(p => p.ChildContent, (EditContext editContext) => builder =>
            {
                onEditContext?.Invoke(editContext);

                builder.OpenComponent<DataAnnotationsValidator>(0);
                builder.CloseComponent();

                builder.OpenComponent<TComponent>(1);
                builder.AddComponentParameter(2, "Value", initialValue);
                builder.AddComponentParameter(3, "ValueChanged", EventCallback.Factory.Create<TValue>(this, onChanged));
                builder.AddComponentParameter(4, "ValueExpression", valueExpression);
                builder.AddComponentParameter(5, "Class", "user-class");
                configureComponent?.Invoke(builder);
                builder.CloseComponent();
            }));
    }
}
