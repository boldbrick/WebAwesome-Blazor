using WebAwesome.Blazor.Components;

namespace WebAwesome.Blazor.Base;

/// <summary>
/// The element wrapper a <see cref="WaDayContent"/> renders its day slot into: <see cref="WaDateInputBase{TValue}"/>
/// and <see cref="WaDatePickerBase{TValue}"/> cascade themselves under this type (a fixed cascading value) around
/// their ChildContent, so a day content finds its host, and fails clearly outside one. The pattern of every
/// dynamically named slot (docs\technical.md, "Dynamic slots").
/// </summary>
internal interface IWaDayContentHost
{
    /// <summary>
    /// Tells the host that its set of day slots changed: a <see cref="WaDayContent"/> was added or removed, or
    /// moved to another date. Called from the day content's own lifecycle, on the renderer's synchronization
    /// context.
    /// </summary>
    void DayContentChanged();
}
