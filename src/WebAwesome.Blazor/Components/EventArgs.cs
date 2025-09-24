using Microsoft.AspNetCore.Components;
using System;

namespace WebAwesome.Blazor.Components;

#region ------ Carousel Events ------

/// <summary>
/// Event arguments for carousel slide change events
/// </summary>
public class WaSlideChangeEventArgs
{
    /// <summary>
    /// Zero-based index of the active slide
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Reference to the active slide element
    /// </summary>
    public ElementReference Slide { get; set; }
}

#endregion

#region ------ Tab Events ------

/// <summary>
/// Event arguments for tab change events
/// </summary>
public class WaTabChangeEventArgs
{
    /// <summary>
    /// Name of the tab panel that was shown or hidden
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Event arguments for tab close events
/// </summary>
public class WaTabCloseEventArgs
{
    /// <summary>
    /// Name of the tab panel that was closed
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

#endregion

#region ------ Rating Events ------

/// <summary>
/// Event arguments for rating hover events
/// </summary>
public class WaRatingHoverEventArgs
{
    /// <summary>
    /// Hover phase: 'start', 'move', or 'end'
    /// </summary>
    public string Phase { get; set; } = string.Empty;

    /// <summary>
    /// The potential rating value during hover
    /// </summary>
    public decimal Value { get; set; }
}

#endregion

#region ------ Details Events ------

/// <summary>
/// Event arguments for details toggle events
/// </summary>
public class WaDetailsToggleEventArgs : EventArgs
{
    public bool IsOpen { get; set; }
}

#endregion

#region ------ Split Panel Events ------

/// <summary>
/// Event arguments for split panel reposition events
/// </summary>
public class WaSplitPanelRepositionEventArgs : EventArgs
{
    public decimal Position { get; set; }
    public int PositionInPixels { get; set; }
}

#endregion

#region ------ Observer Events ------

/// <summary>
/// Event arguments for mutation events
/// </summary>
public class MutationEventArgs : EventArgs
{
    /// <summary>
    /// Array of MutationRecord objects describing the mutations
    /// </summary>
    public object[]? MutationRecords { get; set; }
}

/// <summary>
/// Event arguments for resize events
/// </summary>
public class ResizeEventArgs : EventArgs
{
    /// <summary>
    /// Array of ResizeObserverEntry objects describing the size changes
    /// </summary>
    public object[]? ResizeObserverEntries { get; set; }
}

#endregion