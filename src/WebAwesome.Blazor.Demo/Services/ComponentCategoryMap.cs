using System.Collections.Generic;

namespace WebAwesome.Blazor.Demo.Services;

/// <summary>
/// Maps component tag names to the documentation category Web Awesome itself uses to group
/// components in its own docs navigation (Actions, Feedback &amp; Status, Form Controls, Imagery,
/// Navigation, Organization, Utilities). Sourced from the "category" front matter of each
/// component's page under inputs\WebAwesome\components\*.md.
/// </summary>
/// <remarks>
/// This mapping lives here rather than in the generated API surface document because "category" is
/// a docs-site concept from the component markdown front matter, not part of the Custom Elements
/// Manifest that tools\upgrade\Export-WaApiSurface.ps1 exports (which is scoped to the CEM-derived
/// API for upgrade diffing). Keep this list in sync when new components are added; there is no
/// front matter for wa-intersection-observer, so it is classified as Utilities alongside its sibling
/// observer components.
/// </remarks>
public static class ComponentCategoryMap
{
    /// <summary>
    /// The category display order used in the demo's navigation, matching Web Awesome's own docs.
    /// </summary>
    public static readonly string[] CategoryOrder =
    [
        "Actions",
        "Feedback & Status",
        "Form Controls",
        "Imagery",
        "Navigation",
        "Organization",
        "Utilities"
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
        ["wa-qr-code"] = "Actions",

        ["wa-badge"] = "Feedback & Status",
        ["wa-callout"] = "Feedback & Status",
        ["wa-progress-bar"] = "Feedback & Status",
        ["wa-progress-ring"] = "Feedback & Status",
        ["wa-skeleton"] = "Feedback & Status",
        ["wa-spinner"] = "Feedback & Status",
        ["wa-tag"] = "Feedback & Status",
        ["wa-tooltip"] = "Feedback & Status",

        ["wa-checkbox"] = "Form Controls",
        ["wa-color-picker"] = "Form Controls",
        ["wa-combobox"] = "Form Controls",
        ["wa-input"] = "Form Controls",
        ["wa-option"] = "Form Controls",
        ["wa-radio"] = "Form Controls",
        ["wa-radio-group"] = "Form Controls",
        ["wa-rating"] = "Form Controls",
        ["wa-select"] = "Form Controls",
        ["wa-slider"] = "Form Controls",
        ["wa-switch"] = "Form Controls",
        ["wa-textarea"] = "Form Controls",

        ["wa-animated-image"] = "Imagery",
        ["wa-avatar"] = "Imagery",
        ["wa-carousel"] = "Imagery",
        ["wa-carousel-item"] = "Imagery",
        ["wa-comparison"] = "Imagery",
        ["wa-icon"] = "Imagery",
        ["wa-zoomable-frame"] = "Imagery",

        ["wa-breadcrumb"] = "Navigation",
        ["wa-breadcrumb-item"] = "Navigation",
        ["wa-tab"] = "Navigation",
        ["wa-tab-group"] = "Navigation",
        ["wa-tab-panel"] = "Navigation",
        ["wa-tree"] = "Navigation",
        ["wa-tree-item"] = "Navigation",

        ["wa-card"] = "Organization",
        ["wa-details"] = "Organization",
        ["wa-dialog"] = "Organization",
        ["wa-divider"] = "Organization",
        ["wa-drawer"] = "Organization",
        ["wa-scroller"] = "Organization",
        ["wa-split-panel"] = "Organization",

        ["wa-animation"] = "Utilities",
        ["wa-format-bytes"] = "Utilities",
        ["wa-format-date"] = "Utilities",
        ["wa-format-number"] = "Utilities",
        ["wa-include"] = "Utilities",
        ["wa-intersection-observer"] = "Utilities",
        ["wa-mutation-observer"] = "Utilities",
        ["wa-popover"] = "Utilities",
        ["wa-popup"] = "Utilities",
        ["wa-relative-time"] = "Utilities",
        ["wa-resize-observer"] = "Utilities",
    };

    #endregion
}
