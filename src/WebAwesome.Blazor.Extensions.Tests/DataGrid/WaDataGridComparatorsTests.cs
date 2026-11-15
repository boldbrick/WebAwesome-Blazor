using System;
using System.Collections.Generic;
using WebAwesome.Blazor.Extensions.DataGrid;
using Xunit;

namespace WebAwesome.Blazor.Extensions.Tests.DataGrid;

/// <summary>
/// The descriptor of the ordinal comparator: a Registered reference to the comparator of the Extensions initializer,
/// with the sort order as ctx.args.values.
/// </summary>
public class WaDataGridComparatorsTests
{
    [Fact]
    public void Ordinal_WritesTheValuesInTheirOrder()
    {
        var comparator = WaDataGridComparators.Ordinal("new", "active", "done");

        Assert.Equal("""{"$wab":"fn","ref":{"kind":"registered","name":"wab.dataGrid.ordinal"},"args":{"values":["new","active","done"]}}""",
            comparator.ToString());
        Assert.Empty(comparator.Tags);
    }

    [Fact]
    public void Ordinal_OfASequence_EqualsTheOrdinalOfAnArray()
    {
        IEnumerable<string> order = new List<string> { "b", "a" };

        Assert.Equal(WaDataGridComparators.Ordinal("b", "a"), WaDataGridComparators.Ordinal(order));
    }

    [Fact]
    public void Ordinal_RejectsNullAndDuplicates()
    {
        Assert.Throws<ArgumentNullException>(() => WaDataGridComparators.Ordinal((string[])null!));
        Assert.Throws<ArgumentException>(() => WaDataGridComparators.Ordinal("a", null!));
        Assert.Throws<ArgumentException>(() => WaDataGridComparators.Ordinal("a", "b", "a"));
    }
}
