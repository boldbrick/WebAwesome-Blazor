using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// WaDataGridRazorTemplates renders Razor templates to static HTML with the HtmlRenderer and puts the HTML into a
/// separate property of each row, which the razorTemplate preset clones into the cell. These tests check the HTML (the
/// wrappers render their elements, text is encoded, event handlers are left out), the row JSON as the interop writes it,
/// the formatter reference and the validation. The browser part is in tools\e2e\tests\cell-requests.spec.js.
/// </summary>
public class RazorTemplateTests : BunitContext
{
    public RazorTemplateTests()
    {
        JSInterop.Mode = JSRuntimeMode.Loose;
        Services.AddScoped<WebAwesomeJSInterop>();
        Services.AddSingleton(new Greeting("Hello"));
    }

    [Fact]
    public async Task RenderAsync_RendersAWrapperToItsElement()
    {
        var rows = await Templates().RenderAsync(Services, [new Order(1, "Ada", "active")], Cancellation);

        var html = HtmlOf(rows[0], StateKey);
        Assert.Equal("<wa-badge variant=\"success\">active</wa-badge>", html);
    }

    [Fact]
    public async Task RenderAsync_KeepsTheActionAttribute_AndLeavesOutEventHandlers()
    {
        var rows = await Templates().RenderAsync(Services, [new Order(7, "Ada", "active")], Cancellation);

        var html = HtmlOf(rows[0], ActionsKey);
        Assert.Contains("<wa-button", html);
        Assert.Contains("data-wablazor-action=\"open\"", html);
        Assert.Contains("data-wablazor-action-payload=\"{&quot;id&quot;:7}\"", html);
        Assert.Contains(">Open 7</wa-button>", html);
        Assert.DoesNotContain("onclick", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RenderAsync_EncodesTheTextOfTheRow()
    {
        var rows = await Templates().RenderAsync(Services, [new Order(1, "<script>alert(1)</script>", "x\" onmouseover=\"alert(1)")], Cancellation);

        Assert.Equal("<span title=\"x&quot; onmouseover=&quot;alert(1)\">&lt;script&gt;alert(1)&lt;/script&gt;</span>", HtmlOf(rows[0], CustomerKey));
    }

    [Fact]
    public async Task RenderAsync_GivesTheComponentsTheServicesOfThePage()
    {
        var templates = new WaDataGridRazorTemplates<Order>()
            .Add("greeting", order => builder =>
            {
                builder.OpenComponent<GreetingComponent>(0);
                builder.AddComponentParameter(1, nameof(GreetingComponent.Name), order.Customer);
                builder.CloseComponent();
            });

        var rows = await templates.RenderAsync(Services, [new Order(1, "Ada", "active")], Cancellation);

        Assert.Equal("<b>Hello Ada</b>", HtmlOf(rows[0], "greeting"));
    }

    [Fact]
    public async Task RenderAsync_WritesTheRowAsItIs_PlusTheHtmlProperty()
    {
        var rows = await Templates().RenderAsync(Services, [new Order(1, "Ada", "active"), new Order(2, "Grace", "closed")], Cancellation);

        var json = JsonSerializer.SerializeToElement(rows, JsonSerializerOptions.Web);

        Assert.Equal(2, json.GetArrayLength());
        var first = json[0];
        Assert.Equal(["id", "customer", "state", HtmlProperty], first.EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, first.GetProperty("id").GetInt32());
        Assert.Equal("Ada", first.GetProperty("customer").GetString());
        Assert.Equal([StateKey, CustomerKey, ActionsKey], first.GetProperty(HtmlProperty).EnumerateObject().Select(property => property.Name));
        Assert.Equal("<wa-badge variant=\"neutral\">closed</wa-badge>", json[1].GetProperty(HtmlProperty).GetProperty(StateKey).GetString());
    }

    [Fact]
    public async Task RowJson_FollowsTheOptionsOfTheSerializer()
    {
        var rows = await Templates().RenderAsync(Services, [new Order(1, "Ada", "active")], Cancellation);

        var json = JsonSerializer.SerializeToElement(rows[0], new JsonSerializerOptions());

        Assert.Equal(["Id", "Customer", "State", HtmlProperty], json.EnumerateObject().Select(property => property.Name));
    }

    [Fact]
    public async Task RowJson_ReplacesARowPropertyWithTheReservedName()
    {
        var templates = new WaDataGridRazorTemplates<ClashingRow>().Add("name", row => builder => builder.AddContent(0, row.Name));

        var rows = await templates.RenderAsync(Services, [new ClashingRow("Ada", "old")], Cancellation);
        var json = JsonSerializer.SerializeToElement(rows[0], JsonSerializerOptions.Web);

        Assert.Equal(["name", HtmlProperty], json.EnumerateObject().Select(property => property.Name));
        Assert.Equal("Ada", json.GetProperty(HtmlProperty).GetProperty("name").GetString());
    }

    [Fact]
    public async Task RowJson_OfARowThatIsNoObject_Throws()
    {
        var templates = new WaDataGridRazorTemplates<string>().Add("text", text => builder => builder.AddContent(0, text));

        var rows = await templates.RenderAsync(Services, ["plain"], Cancellation);

        Assert.Throws<JsonException>(() => { JsonSerializer.Serialize(rows[0], JsonSerializerOptions.Web); });
    }

    [Fact]
    public async Task RenderAsync_KeepsTheOrderOfTheRows_AndRendersEachCell()
    {
        var calls = 0;
        var templates = new WaDataGridRazorTemplates<Order>()
            .Add("a", order => builder => { Interlocked.Increment(ref calls); builder.AddContent(0, order.Id); })
            .Add("b", order => builder => { Interlocked.Increment(ref calls); builder.AddContent(0, -order.Id); });

        var rows = await templates.RenderAsync(Services, Enumerable.Range(1, 5).Select(id => new Order(id, "c", "s")), Cancellation);

        Assert.Equal(10, calls);
        Assert.Equal(["1", "2", "3", "4", "5"], rows.Select(row => HtmlOf(row, "a")));
        Assert.Equal("-3", HtmlOf(rows[2], "b"));
    }

    [Fact]
    public async Task RenderAsync_WithoutRows_ReturnsAnEmptyList()
    {
        Assert.Empty(await Templates().RenderAsync(Services, [], Cancellation));
    }

    [Fact]
    public async Task RenderAsync_WithANullRow_Throws()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(() => Templates().RenderAsync(Services, [new Order(1, "a", "b"), null!], Cancellation));

        Assert.Equal("rows", exception.ParamName);
        Assert.Contains("index 1", exception.Message);
    }

    [Fact]
    public async Task RenderAsync_StopsOnCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Templates().RenderAsync(Services, [new Order(1, "a", "b")], cancellation.Token));
    }

    [Fact]
    public void Formatter_RefersToTheRegisteredPreset_WithTheKeyAndTheTags()
    {
        var formatter = Templates().Formatter(StateKey, "wa-badge");

        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.razorTemplate"},"args":{"key":"state"},"uses":["wa-badge"]}""", formatter.ToString());
        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.razorTemplate"},"args":{"key":"actions"}}""", Templates().Formatter(ActionsKey).ToString());
    }

    [Fact]
    public void Formatter_OfAnUnknownKey_Throws()
    {
        var exception = Assert.Throws<ArgumentException>(() => Templates().Formatter("missing"));

        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void Formatter_WithAnInvalidTag_Throws()
    {
        Assert.Throws<ArgumentException>(() => Templates().Formatter(StateKey, "badge"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" state")]
    public void Add_WithAnInvalidKey_Throws(string key)
    {
        var exception = Assert.Throws<ArgumentException>(() => new WaDataGridRazorTemplates<Order>().Add(key, _ => _ => { }));

        Assert.Equal("key", exception.ParamName);
    }

    [Fact]
    public void Add_WithAUsedKey_OrNoTemplate_Throws()
    {
        Assert.Throws<ArgumentException>(() => Templates().Add(StateKey, _ => _ => { }));
        Assert.Throws<ArgumentNullException>(() => new WaDataGridRazorTemplates<Order>().Add("x", null!));
    }

    [Fact]
    public void Keys_FollowTheOrderOfAdd()
    {
        Assert.Equal([StateKey, CustomerKey, ActionsKey], Templates().Keys);
    }

    #region ------ Internals ------

    private const string StateKey = "state";

    private static CancellationToken Cancellation => Xunit.TestContext.Current.CancellationToken;
    private const string CustomerKey = "customer";
    private const string ActionsKey = "actions";
    private const string HtmlProperty = "$wabTemplateHtml";

    private sealed record Order(int Id, string Customer, string State);

    private sealed record ClashingRow(string Name, [property: System.Text.Json.Serialization.JsonPropertyName(HtmlProperty)] string Clash);

    private sealed record Greeting(string Text);

    private sealed class GreetingComponent : ComponentBase
    {
        [Inject] private Greeting Greeting { get; set; } = default!;

        [Parameter] public string? Name { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "b");
            builder.AddContent(1, $"{Greeting.Text} {Name}");
            builder.CloseElement();
        }
    }

    // a badge for the state, the customer as text with a title, and a button with a declarative action and a click
    // handler (which static HTML cannot keep)
    private WaDataGridRazorTemplates<Order> Templates()
    {
        return new WaDataGridRazorTemplates<Order>()
            .Add(StateKey, order => builder =>
            {
                builder.OpenComponent<WaBadge>(0);
                builder.AddComponentParameter(1, nameof(WaBadge.Variant), order.State == "active" ? WaVariant.Success : WaVariant.Neutral);
                builder.AddComponentParameter(2, nameof(WaBadge.ChildContent), (RenderFragment)(content => content.AddContent(0, order.State)));
                builder.CloseComponent();
            })
            .Add(CustomerKey, order => builder =>
            {
                builder.OpenElement(0, "span");
                builder.AddAttribute(1, "title", order.State);
                builder.AddContent(2, order.Customer);
                builder.CloseElement();
            })
            .Add(ActionsKey, order => builder =>
            {
                builder.OpenComponent<WaButton>(0);
                builder.AddComponentParameter(1, "data-wablazor-action", "open");
                builder.AddComponentParameter(2, "data-wablazor-action-payload", JsonSerializer.Serialize(new { id = order.Id }));
                builder.AddComponentParameter(3, nameof(WaButton.OnClick), EventCallback.Factory.Create<MouseEventArgs>(this, () => { }));
                builder.AddComponentParameter(4, nameof(WaButton.ChildContent), (RenderFragment)(content => content.AddContent(0, $"Open {order.Id}")));
                builder.CloseComponent();
            });
    }

    private static string HtmlOf(object row, string key)
    {
        return JsonSerializer.SerializeToElement(row, JsonSerializerOptions.Web).GetProperty(HtmlProperty).GetProperty(key).GetString()!;
    }

    #endregion
}
