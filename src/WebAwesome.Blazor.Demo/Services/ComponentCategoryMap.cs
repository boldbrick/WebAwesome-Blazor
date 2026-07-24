using System.Collections.Generic;

namespace WebAwesome.Blazor.Demo.Services;

/// <summary>
/// Maps component tag names to the documentation category Web Awesome itself uses to group
/// components in its own docs navigation (Actions, Forms, Layout, Navigation, Feedback, Media,
/// Data Viz, Helpers — the taxonomy of the webawesome.com components index).
/// </summary>
/// <remarks>
/// This mapping lives here rather than in the generated API surface document because the category is
/// a docs-site concept, not part of the Custom Elements Manifest that tools\upgrade\Export-WaApiSurface.ps1
/// exports (which is scoped to the CEM-derived API for upgrade diffing). Keep this list in sync when new
/// components are added — the upstream source is the category shown on
/// https://webawesome.com/docs/components (the "Experimental" chip there is a status, not a category;
/// experimental components are mapped by function here and the demo marks their status with the flask icon).
/// </remarks>
public static class ComponentCategoryMap
{
    /// <summary>
    /// The category display order used in the demo's navigation, matching Web Awesome's own docs.
    /// </summary>
    public static readonly string[] CategoryOrder =
    [
        "Actions",
        "Forms",
        "Layout",
        "Navigation",
        "Feedback",
        "Media",
        "Data Viz",
        "Helpers"
    ];

    /// <summary>
    /// Gets the documentation category for the given component tag name.
    /// </summary>
    /// <param name="tag">Custom element tag name, e.g. "wa-button"</param>
    /// <returns>The category name, or "Other" when the tag isn't in the map</returns>
    public static string GetCategory(string tag) => tagToCategory.TryGetValue(tag, out var category) ? category : "Other";

    #region ------ Internals ------

    private static readonly Dictionary<string, string> tagToCategory = new()
    {
        ["wa-button"] = "Actions",
        ["wa-button-group"] = "Actions",
        ["wa-copy-button"] = "Actions",
        ["wa-dropdown"] = "Actions",
        ["wa-dropdown-item"] = "Actions",

        ["wa-checkbox"] = "Forms",
        ["wa-color-picker"] = "Forms",
        ["wa-combobox"] = "Forms",
        ["wa-file-input"] = "Forms",
        ["wa-input"] = "Forms",
        ["wa-number-input"] = "Forms",
        ["wa-option"] = "Forms",
        ["wa-radio"] = "Forms",
        ["wa-radio-group"] = "Forms",
        ["wa-rating"] = "Forms",
        ["wa-select"] = "Forms",
        ["wa-slider"] = "Forms",
        ["wa-switch"] = "Forms",
        ["wa-textarea"] = "Forms",

        ["wa-card"] = "Layout",
        ["wa-details"] = "Layout",
        ["wa-dialog"] = "Layout",
        ["wa-divider"] = "Layout",
        ["wa-drawer"] = "Layout",
        ["wa-page"] = "Layout",
        ["wa-scroller"] = "Layout",
        ["wa-split-panel"] = "Layout",

        ["wa-breadcrumb"] = "Navigation",
        ["wa-breadcrumb-item"] = "Navigation",
        ["wa-tab"] = "Navigation",
        ["wa-tab-group"] = "Navigation",
        ["wa-tab-panel"] = "Navigation",
        ["wa-tree"] = "Navigation",
        ["wa-tree-item"] = "Navigation",

        ["wa-badge"] = "Feedback",
        ["wa-callout"] = "Feedback",
        ["wa-popover"] = "Feedback",
        ["wa-popup"] = "Feedback",
        ["wa-progress-bar"] = "Feedback",
        ["wa-progress-ring"] = "Feedback",
        ["wa-skeleton"] = "Feedback",
        ["wa-spinner"] = "Feedback",
        ["wa-tag"] = "Feedback",
        ["wa-toast"] = "Feedback",
        ["wa-toast-item"] = "Feedback",
        ["wa-tooltip"] = "Feedback",

        ["wa-animated-image"] = "Media",
        ["wa-avatar"] = "Media",
        ["wa-carousel"] = "Media",
        ["wa-carousel-item"] = "Media",
        ["wa-comparison"] = "Media",
        ["wa-icon"] = "Media",
        ["wa-zoomable-frame"] = "Media",

        ["wa-bar-chart"] = "Data Viz",
        ["wa-bubble-chart"] = "Data Viz",
        ["wa-chart"] = "Data Viz",
        ["wa-doughnut-chart"] = "Data Viz",
        ["wa-line-chart"] = "Data Viz",
        ["wa-pie-chart"] = "Data Viz",
        ["wa-polar-area-chart"] = "Data Viz",
        ["wa-radar-chart"] = "Data Viz",
        ["wa-scatter-chart"] = "Data Viz",
        ["wa-sparkline"] = "Data Viz",

        // animation is listed under the "Experimental" status chip upstream; by function it is a helper
        ["wa-animation"] = "Helpers",
        ["wa-format-bytes"] = "Helpers",
        ["wa-format-date"] = "Helpers",
        ["wa-format-number"] = "Helpers",
        ["wa-include"] = "Helpers",
        ["wa-intersection-observer"] = "Helpers",
        ["wa-markdown"] = "Helpers",
        ["wa-mutation-observer"] = "Helpers",
        ["wa-qr-code"] = "Helpers",
        ["wa-relative-time"] = "Helpers",
        ["wa-resize-observer"] = "Helpers",
    };

    #endregion
}
