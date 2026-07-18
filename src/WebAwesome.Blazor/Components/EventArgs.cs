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
    /// <summary>
    /// Whether the details element is open after the toggle.
    /// </summary>
    public bool IsOpen { get; set; }
}

#endregion

#region ------ Split Panel Events ------

/// <summary>
/// Event arguments for split panel reposition events
/// </summary>
public class WaSplitPanelRepositionEventArgs : EventArgs
{
    /// <summary>
    /// The new position of the divider from the primary panel's edge, as a percentage between 0 and 100.
    /// </summary>
    public decimal Position { get; set; }

    /// <summary>
    /// The new position of the divider from the primary panel's edge, in pixels.
    /// </summary>
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

#region ------ Utility Events ------

/// <summary>
/// Event arguments for include error events
/// </summary>
public class IncludeErrorEventArgs : EventArgs
{
    /// <summary>
    /// HTTP status code of the failed request (e.g., 404)
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error message describing the failure
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// Event arguments for zoom change events
/// </summary>
public class ZoomChangeEventArgs : EventArgs
{
    /// <summary>
    /// The new zoom level (1.0 = 100%)
    /// </summary>
    public double ZoomLevel { get; set; }

    /// <summary>
    /// The previous zoom level
    /// </summary>
    public double PreviousZoomLevel { get; set; }
}

#endregion

#region ------ Intersection Observer Events ------

/// <summary>
/// Event arguments for intersection observer events
/// </summary>
public class WaIntersectionEventArgs
{
    /// <summary>
    /// Whether the target element is intersecting with the root
    /// </summary>
    public bool IsIntersecting { get; set; }

    /// <summary>
    /// The ratio of intersection between 0.0 and 1.0
    /// </summary>
    public double IntersectionRatio { get; set; }

    /// <summary>
    /// Reference to the target element being observed
    /// </summary>
    public ElementReference Target { get; set; }
}

#endregion