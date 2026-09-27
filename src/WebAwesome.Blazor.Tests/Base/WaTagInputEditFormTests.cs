using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Tests.Forms;
using Xunit;

namespace WebAwesome.Blazor.Tests.Base;

/// <summary>
/// EditForm integration tests for the new WaTagInput component (Web Awesome 3.13.0): two-way binding of its
/// IReadOnlyList&lt;string&gt;? Value through Value/ValueChanged/ValueExpression, change propagation from the
/// element's array-valued "change" event to the model and EditContext (field modified), the DataAnnotations
/// validation lifecycle ([Required]/[MinLength(2)]), and the setCustomValidity/resetValidity JS interop
/// round-trip. Mirrors the pattern of Base/EditFormIntegrationTests.cs and Forms/WaSelectEditFormTests.cs.
/// </summary>
public class WaTagInputEditFormTests : FormControlTestBase
{
    [Fact]
    public void RendersBoundValueAndValidClass()
    {
        // Arrange & Act
        var model = new TagsModel { Tags = ["urgent", "review"] };
        var cut = RenderForm(model);

        // Assert - no "value" attribute is rendered (the live property carries the tags instead)
        var element = cut.Find("wa-tag-input");
        Assert.False(element.HasAttribute("value"));

        var cssClass = element.GetAttribute("class");
        Assert.Contains("user-class", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void UserChange_UpdatesModelThroughBinding_AndMarksFieldModified()
    {
        // Arrange
        var model = new TagsModel { Tags = ["urgent", "review"] };
        EditContext? editContext = null;
        var cut = RenderForm(model, context => editContext = context);

        // Act - the element reports its current tag array
        cut.Find("wa-tag-input").Change(new[] { "urgent", "review", "later" });

        // Assert
        Assert.Equal(new[] { "urgent", "review", "later" }, model.Tags);
        Assert.NotNull(editContext);
        Assert.True(editContext!.IsModified(() => model.Tags));
    }

    [Fact]
    public void InvalidUserInput_BelowMinLength_GetsModifiedInvalidCssClasses()
    {
        // Arrange
        var model = new TagsModel { Tags = ["urgent", "review"] };
        var cut = RenderForm(model);

        // Act - MinLength(2) violated by leaving a single tag
        cut.Find("wa-tag-input").Change(new[] { "urgent" });

        // Assert
        var cssClass = cut.Find("wa-tag-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("invalid", cssClass);
    }

    [Fact]
    public void CorrectedUserInput_ReturnsToValidCssClass()
    {
        // Arrange
        var model = new TagsModel { Tags = ["urgent", "review"] };
        var cut = RenderForm(model);

        // Act
        cut.Find("wa-tag-input").Change(new[] { "urgent" });
        cut.Find("wa-tag-input").Change(new[] { "urgent", "review" });

        // Assert
        var cssClass = cut.Find("wa-tag-input").GetAttribute("class");
        Assert.Contains("modified", cssClass);
        Assert.Contains("valid", cssClass);
        Assert.DoesNotContain("invalid", cssClass);
    }

    [Fact]
    public void FailedSubmit_WithNullValue_ProducesValidationMessages()
    {
        // Arrange - Required is violated by an unset model value
        var model = new TagsModel { Tags = null };
        EditContext? capturedContext = null;
        var cut = RenderForm(model, editContext => capturedContext = editContext);

        // Act
        cut.Find("form").Submit();

        // Assert
        Assert.NotNull(capturedContext);
        var messages = capturedContext!.GetValidationMessages().ToList();
        Assert.NotEmpty(messages);
        Assert.Contains("invalid", cut.Find("wa-tag-input").GetAttribute("class"));
    }

    [Fact]
    public async Task SetCustomValidityAsync_ReachesInteropModule()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("setCustomValidity", _ => true).SetVoidResult();

        var model = new TagsModel { Tags = ["urgent", "review"] };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaTagInput>().Instance;

        // Act
        await cut.InvokeAsync(() => component.SetCustomValidityAsync("No duplicate tags allowed"));

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "setCustomValidity");
        Assert.Equal("No duplicate tags allowed", invocation.Arguments[1]);
    }

    [Fact]
    public async Task ResetValidityAsync_ReachesInteropModule()
    {
        // Arrange
        var module = JSInterop.SetupModule(InteropModulePath);
        module.SetupVoid("invokeMethod", i => Equals(i.Arguments[1], "resetValidity")).SetVoidResult();

        var model = new TagsModel { Tags = ["urgent", "review"] };
        var cut = RenderForm(model);
        var component = cut.FindComponent<WaTagInput>().Instance;

        // Act
        await cut.InvokeAsync(() => component.ResetValidityAsync());

        // Assert
        var invocation = Assert.Single(module.Invocations, i => i.Identifier == "invokeMethod");
        Assert.Equal("resetValidity", invocation.Arguments[1]);
    }

    #region ------ Internals ------

    private class TagsModel
    {
        [Required]
        [MinLength(2)]
        public IReadOnlyList<string>? Tags { get; set; }
    }

    private IRenderedComponent<EditForm> RenderForm(TagsModel model, Action<EditContext>? onEditContext = null)
    {
        return RenderControlForm<WaTagInput, IReadOnlyList<string>?>(
            model,
            model.Tags,
            value => model.Tags = value,
            () => model.Tags,
            onEditContext: onEditContext);
    }

    #endregion
}
