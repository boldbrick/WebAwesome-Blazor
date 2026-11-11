using System;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// The payload of the test element's remove event (<c>xyz-remove</c>); event args inherit <see cref="EventArgs"/>.
/// </summary>
public sealed class XyzRemoveEventArgs : EventArgs
{
    /// <summary>
    /// The key of the removed chip.
    /// </summary>
    public string? Key { get; set; }
}
