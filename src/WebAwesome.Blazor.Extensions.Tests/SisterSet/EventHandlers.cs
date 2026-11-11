using System;
using Microsoft.AspNetCore.Components;

namespace WebAwesome.Blazor.Extensions.Tests.SisterSet;

/// <summary>
/// Declares the Razor event attributes of the test sister set, as a sister set declares its own: the Razor compiler
/// reads <see cref="EventHandlerAttribute"/> declarations only from a public static class named <c>EventHandlers</c>.
/// </summary>
[EventHandler(XyzChip.RemoveEventAttribute, typeof(XyzRemoveEventArgs), enableStopPropagation: true, enablePreventDefault: true)]
[EventHandler(XyzChip.InvalidEventAttribute, typeof(EventArgs), enableStopPropagation: true, enablePreventDefault: true)]
public static class EventHandlers
{
}
