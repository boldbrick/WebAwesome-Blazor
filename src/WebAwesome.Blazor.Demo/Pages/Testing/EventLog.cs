using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace WebAwesome.Blazor.Demo.Pages.Testing;

/// <summary>
/// Records the EventCallback invocations of an e2e harness page: how often each callback ran and the payload it
/// last received, serialized as JSON. EventLogView renders the log, so the browser tests read what reached .NET
/// instead of what the element did.
/// </summary>
public sealed class EventLog
{
    /// <summary>
    /// The recorded callbacks, in the order of their first invocation.
    /// </summary>
    public IReadOnlyList<EventLogEntry> Entries => entries;

    /// <summary>
    /// Records one invocation of a callback.
    /// </summary>
    /// <param name="callbackId">"Wrapper.Callback", e.g. "WaDialog.OnShow"; the tests address the entry by it</param>
    /// <param name="args">The event arguments the callback received, or null for a parameterless callback</param>
    public void Record(string callbackId, object? args = null)
    {
        var entry = entries.FirstOrDefault(e => e.Id == callbackId);
        if (entry == null)
        {
            entry = new EventLogEntry(callbackId);
            entries.Add(entry);
        }

        entry.Count++;
        entry.LastPayload = Serialize(args);
    }

    /// <summary>
    /// Returns how often a callback ran.
    /// </summary>
    /// <param name="callbackId">"Wrapper.Callback"</param>
    /// <returns>The number of recorded invocations</returns>
    public int CountOf(string callbackId) => entries.FirstOrDefault(e => e.Id == callbackId)?.Count ?? 0;

    #region ------ Internals ------

    private const string NoPayload = "(none)";

    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly List<EventLogEntry> entries = [];

    private static string Serialize(object? args)
    {
        if (args == null) return NoPayload;

        try
        {
            return JsonSerializer.Serialize(args, args.GetType(), SerializerOptions);
        }
        catch (Exception ex) when (ex is NotSupportedException or InvalidOperationException)
        {
            return $"({args.GetType().Name}: {ex.Message})";
        }
    }

    #endregion
}

/// <summary>
/// One recorded callback of an EventLog.
/// </summary>
/// <param name="id">"Wrapper.Callback"</param>
public sealed class EventLogEntry(string id)
{
    /// <summary>
    /// "Wrapper.Callback", e.g. "WaDialog.OnShow".
    /// </summary>
    public string Id { get; } = id;

    /// <summary>
    /// Number of recorded invocations.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// The last received payload as JSON, or "(none)" for a parameterless callback.
    /// </summary>
    public string LastPayload { get; set; } = string.Empty;
}
