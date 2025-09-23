using Microsoft.AspNetCore.Components;

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