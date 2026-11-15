using System;
using System.Collections.Generic;
using System.Text.Json;

namespace WebAwesome.Blazor.Extensions.DataGrid;

/// <summary>
/// Event arguments of a cell action: content in a data grid cell asks the app to do something with its row. The
/// interactive presets raise it (<see cref="WaDataGridFormatters.ActionLink"/>, <c>Button</c>, <c>Actions</c>,
/// <c>Menu</c>, <c>Switch</c>, <c>Checkbox</c>), and so do custom functions: <c>ctx.emit(action, payload)</c>,
/// <c>ctx.h(tag, { action })</c>, or an element with <c>data-wablazor-action="id"</c> (and optionally
/// <c>data-wablazor-action-payload</c> with JSON) that is clicked. Bind the event with <c>@onwablazor-cell-action</c>
/// on <c>WaDataGrid</c>; the grid passes the attribute to its element.
/// </summary>
/// <remarks>
/// <para>
/// The event is a notification: the browser does not wait for the handler, so the handler cannot cancel the action or
/// give a value back to the cell. When the handler changes the row, update the data of the grid.
/// </para>
/// <para>
/// A click that raises an action, or a click on a control of a preset (a menu trigger, a switch, a copy button), does
/// not raise <c>OnCellClick</c> of the grid: the event bridge stops that <c>wa-cell-click</c> before Blazor sees it.
/// JavaScript listeners on the grid element still receive it.
/// </para>
/// </remarks>
public class WaDataGridCellActionEventArgs : EventArgs
{
    /// <summary>
    /// The id of the column of the cell; empty for the content of a detail panel.
    /// </summary>
    public string Column { get; set; } = string.Empty;

    /// <summary>
    /// The action id, for example the <c>action</c> of <see cref="WaDataGridFormatters.ActionLink"/>.
    /// </summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// The key of the row: the value of the field that the <c>RowKey</c> of the grid names, a JSON string or number as
    /// in the row data; null when the grid has no row key or the row has no value for it.
    /// </summary>
    public JsonElement? RowKey { get; set; }

    /// <summary>
    /// The row object of the cell, as the browser holds it (camelCase field names). Use <see cref="GetRow{T}"/> to read
    /// it as the row type of the app.
    /// </summary>
    public IReadOnlyDictionary<string, object>? Row { get; set; }

    /// <summary>
    /// The raw value of the cell.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// The data that the content of the cell sends with the action, for example <c>{ "checked": true }</c> of a switch;
    /// null when there is none. Use <see cref="GetPayload{T}"/> to read it as a type.
    /// </summary>
    public JsonElement? Payload { get; set; }

    /// <summary>
    /// The display index of the row (after sort, filter and pagination); -1 when it is not known.
    /// </summary>
    public int RowIndex { get; set; } = UnknownRowIndex;

    /// <summary>
    /// The value of <see cref="RowIndex"/> when the index is not known.
    /// </summary>
    public const int UnknownRowIndex = -1;

    /// <summary>
    /// Reads the row as a type of the app, for example the record of the grid data.
    /// </summary>
    /// <typeparam name="T">The row type</typeparam>
    /// <param name="options">The serializer options; the web defaults (camelCase, case-insensitive) when null</param>
    /// <returns>The row, or the default of <typeparamref name="T"/> when there is no row</returns>
    /// <exception cref="JsonException">Thrown when the row does not match the type</exception>
    public T? GetRow<T>(JsonSerializerOptions? options = null)
    {
        if (Row is null) return default;

        var json = JsonSerializer.SerializeToElement(Row, JsonSerializerOptions.Web);
        return json.Deserialize<T>(options ?? JsonSerializerOptions.Web);
    }

    /// <summary>
    /// Reads the payload as a type.
    /// </summary>
    /// <typeparam name="T">The payload type</typeparam>
    /// <param name="options">The serializer options; the web defaults (camelCase, case-insensitive) when null</param>
    /// <returns>The payload, or the default of <typeparamref name="T"/> when there is no payload</returns>
    /// <exception cref="JsonException">Thrown when the payload does not match the type</exception>
    public T? GetPayload<T>(JsonSerializerOptions? options = null)
    {
        if (Payload is not { ValueKind: not JsonValueKind.Null and not JsonValueKind.Undefined } payload) return default;

        return payload.Deserialize<T>(options ?? JsonSerializerOptions.Web);
    }
}
