using System;
using System.Collections.Generic;
using System.Globalization;
using WebAwesome.Blazor.Components;
using WebAwesome.Blazor.Extensions.DataGrid;
using WebAwesome.Blazor.Models;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The descriptors of the cell presets: each factory returns a Registered reference to its preset function, with the
/// options that are set (and nothing else) as ctx.args, and declares the elements the preset renders. The preset
/// functions in data-grid-presets.js read exactly these option names.
/// </summary>
public class WaDataGridFormattersTests
{
    [Fact]
    public void Number_WritesAllOptionsUnderTheirJavaScriptNames()
    {
        var formatter = WaDataGridFormatters.Number(1, 2, false, WaDataGridNumberNotation.Compact, WaDataGridSignDisplay.ExceptZero,
            "kilometer-per-hour", WaDisplay.Long, "n/a", Czech);

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.number"},"args":{"culture":"cs-CZ","nullText":"n/a","minFractionDigits":1,"maxFractionDigits":2,"signDisplay":"exceptZero","useGrouping":false,"notation":"compact","unit":"kilometer-per-hour","unitDisplay":"long"}}""",
            formatter.ToString());
    }

    [Fact]
    public void Number_WithoutOptions_WritesOnlyTheCulture()
    {
        Assert.Equal("""{"culture":"cs-CZ"}""", ArgsOf(WaDataGridFormatters.Number(culture: Czech)));
    }

    [Fact]
    public void Currency_WritesTheCodeTheFieldAndTheDisplay()
    {
        var formatter = WaDataGridFormatters.Currency("eur", "currency", 0, 0, WaCurrencyDisplay.Code, culture: English);

        Assert.Equal("wab.dataGrid.currency", formatter.Reference);
        Assert.Equal(
            """{"culture":"en-US","minFractionDigits":0,"maxFractionDigits":0,"currency":"EUR","currencyField":"currency","currencyDisplay":"code"}""",
            ArgsOf(formatter));
    }

    [Fact]
    public void Currency_FromAFieldOnly_HasNoFixedCode()
    {
        Assert.Equal("""{"culture":"en-US","currencyField":"price.currency"}""",
            ArgsOf(WaDataGridFormatters.Currency(currencyField: "price.currency", culture: English)));
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("EURO", null)]
    [InlineData("E1R", null)]
    [InlineData(null, "price currency")]
    [InlineData(null, "")]
    public void Currency_RejectsAMissingOrInvalidCurrency(string? currency, string? field)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Currency(currency, field));
    }

    [Fact]
    public void Percent_WritesTheScale()
    {
        Assert.Equal("""{"culture":"en-US","maxFractionDigits":1,"scale":"percent"}""",
            ArgsOf(WaDataGridFormatters.Percent(WaDataGridPercentScale.Percent, maxFractionDigits: 1, culture: English)));
    }

    [Theory]
    [InlineData(-1, null)]
    [InlineData(null, 101)]
    [InlineData(3, 2)]
    public void NumberPresets_RejectFractionDigitsOutOfRange(int? min, int? max)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Number(min, max));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Percent(minFractionDigits: min, maxFractionDigits: max));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Currency("EUR", minFractionDigits: min, maxFractionDigits: max));
    }

    [Theory]
    [InlineData("Kilometer")]
    [InlineData("kilo meter")]
    [InlineData("-meter")]
    public void Number_RejectsAnInvalidUnit(string unit)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Number(unit: unit));
    }

    [Fact]
    public void DatePresets_WriteTheirStylesAndTheTimeZone()
    {
        Assert.Equal("wab.dataGrid.date", WaDataGridFormatters.Date().Reference);
        Assert.Equal("""{"culture":"en-US","dateStyle":"long","timeZone":"Europe/Prague"}""",
            ArgsOf(WaDataGridFormatters.Date(WaDataGridDateStyle.Long, "Europe/Prague", culture: English)));

        var dateTime = WaDataGridFormatters.DateTime(WaDataGridDateStyle.Medium, WaDataGridDateStyle.Short, "UTC", "-", English);
        Assert.Equal("wab.dataGrid.dateTime", dateTime.Reference);
        Assert.Equal("""{"culture":"en-US","nullText":"-","dateStyle":"medium","timeStyle":"short","timeZone":"UTC"}""", ArgsOf(dateTime));

        var time = WaDataGridFormatters.Time(WaDataGridDateStyle.Full, culture: English);
        Assert.Equal("wab.dataGrid.time", time.Reference);
        Assert.Equal("""{"culture":"en-US","timeStyle":"full"}""", ArgsOf(time));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Europe/ Prague")]
    [InlineData("UTC\n")]
    public void DatePresets_RejectAnInvalidTimeZone(string timeZone)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Date(timeZone: timeZone));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.DateTime(timeZone: timeZone));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Time(timeZone: timeZone));
    }

    [Fact]
    public void RelativeTimeBytesAndDuration_WriteTheirOptions()
    {
        Assert.Equal("""{"culture":"en-US","numeric":"auto","style":"narrow"}""",
            ArgsOf(WaDataGridFormatters.RelativeTime(WaRelativeTimeNumeric.Auto, WaRelativeTimeFormat.Narrow, culture: English)));
        Assert.Equal("""{"culture":"en-US","unit":"bit","display":"long"}""",
            ArgsOf(WaDataGridFormatters.Bytes(WaByteUnit.Bit, WaDisplay.Long, culture: English)));
        Assert.Equal("""{"culture":"en-US","unit":"milliseconds","style":"digital"}""",
            ArgsOf(WaDataGridFormatters.Duration(WaDataGridDurationUnit.Milliseconds, WaDataGridDurationStyle.Digital, culture: English)));

        Assert.Equal("wab.dataGrid.relativeTime", WaDataGridFormatters.RelativeTime().Reference);
        Assert.Equal("wab.dataGrid.bytes", WaDataGridFormatters.Bytes().Reference);
        Assert.Equal("wab.dataGrid.duration", WaDataGridFormatters.Duration().Reference);
    }

    [Fact]
    public void Lookup_KeepsTheKeysVerbatim()
    {
        var formatter = WaDataGridFormatters.Lookup(new Dictionary<string, string> { ["ACT"] = "Active", ["inProgress"] = "In progress" }, "Unknown", "-");

        Assert.Equal("wab.dataGrid.lookup", formatter.Reference);
        Assert.Equal("""{"nullText":"-","labels":{"ACT":"Active","inProgress":"In progress"},"fallback":"Unknown"}""", ArgsOf(formatter));
        Assert.Empty(formatter.Tags);
    }

    [Fact]
    public void Lookup_RejectsANullMapOrLabel()
    {
        Assert.Throws<ArgumentNullException>(() => WaDataGridFormatters.Lookup(null!));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Lookup(new Dictionary<string, string> { ["a"] = null! }));
    }

    [Fact]
    public void BooleanTemplateAndList_WriteTheirOptions()
    {
        Assert.Equal("""{"nullText":"?","trueText":"Yes","falseText":"No"}""", ArgsOf(WaDataGridFormatters.Boolean("Yes", "No", "?")));
        Assert.Equal("""{"culture":"en-US","template":"{firstName} {lastName} ({amount:number})"}""",
            ArgsOf(WaDataGridFormatters.Template("{firstName} {lastName} ({amount:number})", culture: English)));
        Assert.Equal("""{"separator":" / "}""", ArgsOf(WaDataGridFormatters.List(" / ")));
        Assert.Equal("{}", ArgsOf(WaDataGridFormatters.List()));

        Assert.Equal("wab.dataGrid.boolean", WaDataGridFormatters.Boolean("Yes", "No").Reference);
        Assert.Equal("wab.dataGrid.template", WaDataGridFormatters.Template("{a}").Reference);
        Assert.Equal("wab.dataGrid.list", WaDataGridFormatters.List().Reference);
        Assert.Throws<ArgumentNullException>(() => WaDataGridFormatters.Boolean(null!, "No"));
        Assert.Throws<ArgumentNullException>(() => WaDataGridFormatters.Boolean("Yes", null!));
    }

    [Fact]
    public void Badge_WritesItsMapsAndDeclaresWaBadge()
    {
        var formatter = WaDataGridFormatters.Badge(
            new Dictionary<string, WaVariant> { ["active"] = WaVariant.Success, ["failed"] = WaVariant.Danger },
            WaVariant.Neutral, WaBadgeAppearance.FilledOutlined, true,
            new Dictionary<string, string> { ["active"] = "Active" });

        Assert.Equal(
            """{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.badge"},"args":{"variants":{"active":"success","failed":"danger"},"fallback":"neutral","appearance":"filled-outlined","pill":true,"labels":{"active":"Active"}},"uses":["wa-badge"]}""",
            formatter.ToString());
    }

    [Fact]
    public void Badge_WithIcons_AlsoDeclaresWaIcon()
    {
        var formatter = WaDataGridFormatters.Badge(
            new Dictionary<string, WaVariant> { ["active"] = WaVariant.Success },
            icons: new Dictionary<string, WaDataGridIcon> { ["active"] = new("circle-check") { Family = "sharp", Variant = "solid" } });

        Assert.Equal("""{"variants":{"active":"success"},"icons":{"active":{"name":"circle-check","family":"sharp","variant":"solid"}}}""", ArgsOf(formatter));
        Assert.Equal(["wa-badge", "wa-icon"], formatter.Tags);
    }

    [Fact]
    public void TagAndTags_WriteTheSizeAndDeclareWaTag()
    {
        var tag = WaDataGridFormatters.Tag(new Dictionary<string, WaVariant> { ["bug"] = WaVariant.Danger }, size: WaSize.Small, pill: true);
        Assert.Equal("wab.dataGrid.tag", tag.Reference);
        Assert.Equal("""{"variants":{"bug":"danger"},"pill":true,"size":"s"}""", ArgsOf(tag));
        Assert.Equal(["wa-tag"], tag.Tags);

        var tags = WaDataGridFormatters.Tags(appearance: WaBadgeAppearance.Outlined, nullText: "none");
        Assert.Equal("wab.dataGrid.tags", tags.Reference);
        Assert.Equal("""{"nullText":"none","appearance":"outlined"}""", ArgsOf(tags));
        Assert.Equal(["wa-tag"], tags.Tags);
    }

    [Fact]
    public void Icon_WritesIconsLabelsColorsAndTheFallback()
    {
        var formatter = WaDataGridFormatters.Icon(
            new Dictionary<string, WaDataGridIcon> { ["ok"] = "circle-check", ["error"] = new("circle-xmark") { Library = "custom" } },
            new Dictionary<string, string> { ["ok"] = "Healthy" },
            new Dictionary<string, WaVariant> { ["error"] = WaVariant.Danger },
            "circle-question");

        Assert.Equal("wab.dataGrid.icon", formatter.Reference);
        Assert.Equal(
            """{"icons":{"ok":{"name":"circle-check"},"error":{"name":"circle-xmark","library":"custom"}},"labels":{"ok":"Healthy"},"colors":{"error":"danger"},"fallback":{"name":"circle-question"}}""",
            ArgsOf(formatter));
        Assert.Equal(["wa-icon"], formatter.Tags);
    }

    [Fact]
    public void BooleanIcon_IsAnIconPresetKeyedByTrueAndFalse()
    {
        var formatter = WaDataGridFormatters.BooleanIcon("Active", "Inactive", trueColor: WaVariant.Success, nullText: "-");

        Assert.Equal("wab.dataGrid.icon", formatter.Reference);
        Assert.Equal(
            """{"nullText":"-","icons":{"true":{"name":"check"},"false":{"name":"xmark"}},"labels":{"true":"Active","false":"Inactive"},"colors":{"true":"success"}}""",
            ArgsOf(formatter));
        Assert.Throws<ArgumentNullException>(() => WaDataGridFormatters.BooleanIcon(null!, "No"));
    }

    [Fact]
    public void Icon_RejectsAnEmptyIconName()
    {
        Assert.Throws<ArgumentException>(() => new WaDataGridIcon(" "));
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.Icon(new Dictionary<string, WaDataGridIcon> { ["a"] = null! }));
    }

    [Fact]
    public void Link_WritesTheTemplatesAndTheTarget()
    {
        var formatter = WaDataGridFormatters.Link("/orders/{id}", "Order {id}", WaLinkTarget.Blank, culture: English);

        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.link"},"args":{"culture":"en-US","text":"Order {id}","href":"/orders/{id}","target":"_blank"}}""",
            formatter.ToString());
    }

    [Fact]
    public void ActionLinkEmailAndPhone_WriteTheirOptions()
    {
        Assert.Equal("""{"culture":"en-US","text":"Open","action":"open"}""",
            ArgsOf(WaDataGridFormatters.ActionLink("open", "Open", culture: English)));
        Assert.Equal("wab.dataGrid.actionLink", WaDataGridFormatters.ActionLink("open").Reference);
        Assert.Equal("wab.dataGrid.email", WaDataGridFormatters.Email().Reference);
        Assert.Equal("wab.dataGrid.phone", WaDataGridFormatters.Phone(nullText: "-").Reference);
        Assert.Equal("""{"culture":"en-US","nullText":"-"}""", ArgsOf(WaDataGridFormatters.Phone(nullText: "-", culture: English)));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" open")]
    [InlineData("open\t")]
    [InlineData("op\u0001en")]
    public void ActionLink_RejectsAnInvalidActionId(string action)
    {
        Assert.Throws<ArgumentException>(() => WaDataGridFormatters.ActionLink(action));
    }

    [Fact]
    public void TextPresets_DeclareNoElements()
    {
        foreach (var formatter in new[]
                 {
                     WaDataGridFormatters.Number(), WaDataGridFormatters.Currency("EUR"), WaDataGridFormatters.Percent(),
                     WaDataGridFormatters.Date(), WaDataGridFormatters.Lookup(new Dictionary<string, string>()),
                     WaDataGridFormatters.Link("/a/{id}"), WaDataGridFormatters.ActionLink("open"),
                 })
        {
            Assert.Empty(formatter.Tags);
        }
    }

    [Fact]
    public void Culture_DefaultsToTheCurrentCultureWhenTheFactoryRuns()
    {
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("de-DE");
            var formatter = WaDataGridFormatters.Number();

            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-FR");
            Assert.Equal("""{"culture":"de-DE"}""", ArgsOf(formatter));
            Assert.Equal("""{"culture":"fr-FR"}""", ArgsOf(WaDataGridFormatters.Date()));
            Assert.Equal("""{"culture":"cs-CZ"}""", ArgsOf(WaDataGridFormatters.Date(culture: Czech)));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Culture_TheInvariantCultureIsWrittenAsAmericanEnglish()
    {
        Assert.Equal("""{"culture":"en-US"}""", ArgsOf(WaDataGridFormatters.Number(culture: CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void SameOptions_GiveEqualReferences()
    {
        // equal references give equal column JSON, so the grid does not push the columns again
        Assert.Equal(WaDataGridFormatters.Currency("EUR", culture: English), WaDataGridFormatters.Currency("EUR", culture: English));
        Assert.NotEqual(WaDataGridFormatters.Currency("EUR", culture: English), WaDataGridFormatters.Currency("USD", culture: English));
    }

    #region ------ Internals ------

    private static readonly CultureInfo Czech = CultureInfo.GetCultureInfo("cs-CZ");
    private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

    private static string ArgsOf(WaJsFunction function) => function.Args!.Value.GetRawText();

    #endregion
}
