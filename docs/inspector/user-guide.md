# WA Inspector user guide

This guide is for a developer or a tester who reviews the Web Awesome elements of a page. For the installation and
the background, read the [overview](README.md).

## Before you start

1. Install the extension (see [Install the extension](README.md#install-the-extension)).
2. For a WebAwesome.Blazor app, start the app with the inspector metadata on (see
   [Turn on the Blazor metadata](README.md#turn-on-the-blazor-metadata)). The inspector also works without the
   metadata, but the locators are less exact.

## 1. Open the inspector

1. Open the page in Chrome or Edge.
2. Open DevTools (F12).
3. Select the **Web Awesome** tab. If you do not see it, open the `»` menu of the DevTools tabs.

You can also click the **WA Inspector** button in the browser toolbar (pin it from the extensions menu first). The
inspector then opens in the side panel.

The status line under the toolbar shows the page title, the Web Awesome version and whether the page has Blazor
placement metadata.

## 2. Select an element

Use one of these methods:

- Click an element in the tree. The tree shows the `wa-` elements and the plain elements with `wa-` utility
  classes. Type in the filter box to find a tag, a class, a text or a component.
- Click **Pick**, then click the element on the page. Press Esc to cancel.
- In the DevTools panel, keep **Follow $0** on and select the element in the Elements panel.

The details pane shows the element: the tag, the text, the ancestors, and the Blazor component and chain. **Reveal**
scrolls the page to the element. `×3` in the tree means that the same placement renders three instances.

## 3. Choose the scope

Under **Apply to**, select which elements the change is for. The number shows the elements on the current page.

| Scope | Use it when |
|---|---|
| This placement | The change is for all instances of this position in the source, for example all rows of one list |
| Same source site | The change is for this markup line wherever its component is used (Blazor metadata only) |
| All of this tag | The change is for every element with this tag, on every page |
| This instance | The change is for one element only |

The first change makes a numbered item with a color. More changes to the same element in the same scope go to the
same item. **Add marker** makes an item without a change, for a comment.

## 4. Change values

- **Attributes**: select a value, type a value, or set a check box. A changed attribute name is marked. The hint
  shows the page value. Click `×` to remove the change.
- **CSS custom properties**: type a value. The placeholder shows the default.
- **Part styles**: select a part, then type CSS declarations, for example `padding: 0; gap: 4px`.
- **Slots**: the list shows the slots of the element. Slot content is read-only.

The page shows each change at once. The change stays while you navigate and after a reload, until you delete the
item.

## 5. Remap a data-driven value

If the instances of a placement have different values, the attribute is data-driven. The panel then shows
**remap** and one row for each value, with the number of instances. Select a new value for each value that must
change. Keep **(keep)** for the other values. For example, map `warning` to `danger` and keep `neutral`.

## 6. Change classes and style

- **Classes**: click `×` on a class to remove it. Type a class name and click **Add** (or press Enter) to add one.
  The list proposes the Web Awesome utility classes, for example `wa-gap-l`.
- **Style**: type CSS declarations for the element, for example `margin-top: 0; max-width: 40rem`.

## 7. Write a comment

Type in **Comment** what must change and why. The coding agent reads the comment as the intent. A comment can be the
only content of an item. You can write a comment on any element, also on a plain element.

Write the intent, not only the value. For example: "The delete button must look dangerous in all lists, also in
the dark theme."

## 8. Show the markers

Set **Markers** on to show the numbered outlines of the items on the page. Set it off to see the page without them.
The exported screenshots always show the markers.

## 9. Change the theme

1. Open the **Theme** tab.
2. Type in the filter box, for example `color-brand` or `space`.
3. Type a new value for a token, or select a color for a `--wa-color-*` token.

A theme change applies to the whole page. Click `×` to remove it.

## 10. Check the Changes tab

The **Changes** tab lists all items with their number, color, target, scope, changes and comment. The status of
each item:

| Status | Meaning |
|---|---|
| `pending` | The page does not have the values on its own yet |
| `applied` | The page has the values on its own: the code has the change |
| `unchecked` | The item has no attribute or class change that the inspector can compare (CSS, parts, style, remaps, comments) |
| `not on this page` | No element on this page matches the item |

- **Show** selects the element of the item.
- **Delete** removes the item.
- **Remove applied items** removes all `applied` items.
- **Clear session** removes all items and theme changes of this site.

## 11. Export or copy

- Click **Export**. The inspector saves the change list and the screenshots to `Downloads/wa-review/<date>_<time>/`.
  A message shows the folder.
- Click **Copy** to copy the change list as Markdown, for example for a ticket. Copy has no screenshots.

## 12. Give the export to the coding agent

1. Open Claude Code in the repository of the app.
2. Run `/wa-inspector:wa-review-apply` with the export folder (see
   [Hand-off to the coding agent](README.md#hand-off-to-the-coding-agent)).
3. Read the report: done, changed differently, or not done, for each item.
4. Restart or reload the app. Reload the page with the inspector open.
5. In the **Changes** tab, make sure that the items show `applied`. Look at the `unchecked` items on the page.
6. Remove the finished items.

## Troubleshooting

| Problem | Remedy |
|---|---|
| "The inspector is not active on this page" | Reload the page. The extension loads with the page, so pages that were open before the installation have no inspector. |
| The **Web Awesome** tab is not in DevTools | Close and open DevTools. Make sure that the extension is on, on the extensions page. |
| The status line shows a warning "Web Awesome X (forms from Y)" | The page uses another Web Awesome version than the forms of the inspector. The forms can miss new attributes or show old ones. Update the extension metadata (see [Maintenance](README.md#maintenance)). |
| The status line shows "no Blazor placement metadata" | The page has no `data-wablazor-*` attributes. Turn on `EmitInspectorMetadata` and restart the app. Without the metadata, the scope **Same source site** is not available. |
| An item shows `not on this page` | The item comes from another page of the site, or the page changed. Open the page where you made the item. |
| A change does not show | Check the scope and the match count. A property-only setting or slot content cannot be changed in the inspector: write a comment. |
| Export fails for the screenshots | Keep the inspected tab visible during the export. The export also writes the files without screenshots. |
