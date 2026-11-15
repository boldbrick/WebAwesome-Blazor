using System;
using WebAwesome.Blazor.Extensions.DataGrid;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The template rules of the text and link presets. A URL template must not let row data choose the scheme of a link:
/// the browser URL-encodes each placeholder value, and the literal parts may name only http, https, mailto or tel.
/// </summary>
public class CellTemplateTests
{
    [Theory]
    [InlineData("{firstName} {lastName}")]
    [InlineData("{customer.address.city}")]
    [InlineData("{amount:number} ({share:percent}) due {due:date}, at {start:time}, created {created:datetime}")]
    [InlineData("{{literal}} braces")]
    [InlineData("no placeholder")]
    [InlineData("{$id} {_private}")]
    public void Template_AcceptsValidTemplates(string template)
    {
        Assert.Equal(template, Assert.IsType<string>(TemplateOf(WaDataGridFormatters.Template(template))));
    }

    [Theory]
    [InlineData("")]
    [InlineData("{unclosed")]
    [InlineData("stray } brace")]
    [InlineData("{}")]
    [InlineData("{first name}")]
    [InlineData("{1st}")]
    [InlineData("{a..b}")]
    [InlineData("{amount:currency}")]
    [InlineData("{amount:}")]
    public void Template_RejectsInvalidTemplates(string template)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Template(template));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.ActionLink("open", template));
    }

    [Theory]
    [InlineData("/orders/{id}")]
    [InlineData("orders/{id}?tab={tab}#top")]
    [InlineData("https://example.com/orders/{id}")]
    [InlineData("HTTP://example.com/{id}")]
    [InlineData("mailto:{email}")]
    [InlineData("tel:{phone}")]
    [InlineData("{path}")]
    [InlineData("//cdn.example.com/{file}")]
    public void Link_AcceptsRelativeUrlsAndTheAllowedSchemes(string template)
    {
        Assert.Equal("wab.dataGrid.link", WaDataGridFormatters.Link(template).Reference);
    }

    [Theory]
    [InlineData("javascript:alert({id})")]
    [InlineData("JavaScript:{x}")]
    [InlineData("data:text/html,{x}")]
    [InlineData("java{x}script:alert(1)")]
    [InlineData("vbscript:{x}")]
    [InlineData("/orders/ {id}")]
    [InlineData("/orders/\t{id}")]
    [InlineData("/orders/{id:number}")]
    [InlineData("/orders/{id")]
    [InlineData("")]
    public void Link_RejectsOtherSchemesWhitespaceAndFormats(string template)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Link(template));
    }

    #region ------ Internals ------

    private static string? TemplateOf(WebAwesome.Blazor.Models.WaJsFunction function)
        => function.Args!.Value.GetProperty("template").GetString();

    #endregion
}
