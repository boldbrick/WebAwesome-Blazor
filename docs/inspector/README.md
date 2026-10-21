# WA Inspector

The WA Inspector is a tool for the UI review of pages that use Web Awesome. A developer or a tester opens a running
page in Chrome or Edge and uses the inspector to:

- see the tree of the `wa-` elements and of the plain elements with Web Awesome utility classes;
- change attributes, CSS custom properties, CSS part styles, classes and inline style, and see the result live;
- change the theme tokens (`--wa-*`) of the whole page;
- write a comment on any element;
- export the change list with screenshots for a coding agent.

The skill `wa-review-apply` of the Claude Code plugin `wablazor` applies the exported change list to the source code.

For the steps of a review, read the [user guide](user-guide.md).

## Parts

| Part | Location | Function |
|---|---|---|
| Browser extension | `tools/wablazor-marketplace/plugins/wablazor/assets/wa-inspector-extension/` | Manifest V3, plain JavaScript, no build step. It ships with the plugin `wablazor`. |
| Element metadata | `tools/wablazor-marketplace/plugins/wablazor/assets/wa-inspector-extension/metadata/wa-metadata.json` | The forms of the panel: attributes, CSS properties, parts, slots, utility classes, theme tokens |
| Metadata generator | `tools/wa-inspector/Update-WaInspectorMetadata.ps1` | Makes the element metadata from the API surface and the Web Awesome release |
| Claude Code plugin | `tools/wablazor-marketplace/plugins/wablazor/` | The plugin `wablazor` with the extension and the skills `install-inspector` and `wa-review-apply` |
| Plugin marketplace | `tools/wablazor-marketplace/` | The local marketplace `wablazor-marketplace` and its publish script ([README](../../tools/wablazor-marketplace/README.md)) |
| Placement metadata | `WebAwesomeOptions.EmitInspectorMetadata` in WebAwesome.Blazor | The `data-wablazor-*` attributes on the wrapper elements |
| Browser tests | `tools/e2e/inspector/` | Playwright tests with the extension in Chromium |

## Install the extension

Install the Claude Code plugin `wablazor` first (see [Hand-off to the coding agent](#hand-off-to-the-coding-agent),
step 2). Then run `/wablazor:install-inspector` in Claude Code on Windows. The skill copies the extension to
`%LOCALAPPDATA%\WABlazor\wa-inspector-extension`, opens Chrome or Edge, puts the folder path on the clipboard and
tells you the next steps on the extensions page. Run it again after a plugin update to update the extension.

To install manually:

1. Open `chrome://extensions` (Chrome) or `edge://extensions` (Edge).
2. Turn on **Developer mode**.
3. Click **Load unpacked**.
4. Select the folder `tools/wablazor-marketplace/plugins/wablazor/assets/wa-inspector-extension`, or the folder
   that `/wablazor:install-inspector` made.
5. Reload the pages that were open before. The extension loads its content script when a page loads.

The extension needs Chrome or Edge 116 or later. After an update of the extension files, click the reload button of
the extension on the extensions page, and then reload the inspected page.

## Turn on the Blazor metadata

The inspector works on every page with Web Awesome elements. In a WebAwesome.Blazor app, turn on the placement
metadata. Then the inspector knows which component and which source position rendered each element, and the coding
agent finds the source more easily.

Use one of these settings:

- the option `WebAwesomeOptions.EmitInspectorMetadata = true` in the `AddWebAwesome` delegate;
- the configuration key `WebAwesome:EmitInspectorMetadata` set to `true`, when the app calls
  `AddWebAwesome(builder.Configuration)`;
- the environment variable `WebAwesome__EmitInspectorMetadata=true`, with the same `AddWebAwesome(builder.Configuration)` call.

Example:

```csharp
builder.Services.AddWebAwesome(options =>
    options.EmitInspectorMetadata = builder.Environment.IsDevelopment());
```

> **Caution:** Turn on the metadata in development and test environments only. The attributes show the names of the
> application's components, and the metadata costs some render time.

The setting is process-wide. In a Blazor Web App with WebAssembly or Auto render modes, the client project calls
`AddWebAwesome` too: set the option there also (for example in `wwwroot/appsettings.Development.json`). The
default is `false`, and the demo apps keep that default.

## Panel modes

One panel page runs in three hosts:

| Mode | How to open it | Notes |
|---|---|---|
| DevTools panel | Open DevTools (F12), then the **Web Awesome** tab | **Follow $0** selects the element that the Elements panel selects. **Inspect in Elements** shows the selected element in the Elements panel. |
| Side panel | Click the **WA Inspector** button in the browser toolbar | Follows the active tab of its window |
| Tab | Open `chrome-extension://<extension id>/panel.html?tabId=<tab id>` | Inspects one tab. The browser tests use this mode. |

On the Elements tab, the tree and the details are stacked in the side panel and in a narrow panel, and side
by side in a wide panel. A divider between them sets their size (see the [user guide](user-guide.md#2-select-an-element)).

## Hand-off to the coding agent

1. Click **Export** in the panel. The inspector writes a folder `Downloads/wa-review/<yyyy-MM-dd_HHmmss>/` with:
   - `changes.json`: the machine-readable change list, schema `wa-inspector/change-list@1`;
   - `changes.md`: the same change list for people;
   - `page.png`: a screenshot of the visible page with the numbered outlines;
   - `item-NN.png`: one screenshot for each item that matches an element on the page.
2. Install the Claude Code plugin once:

   ```
   claude plugin marketplace add <repository>/tools/wablazor-marketplace
   claude plugin install wablazor@wablazor-marketplace
   ```

   If your team publishes the plugin into a shared marketplace folder, install it from there
   (`tools/wablazor-marketplace/README.md`, "Publish into another marketplace").
3. Open Claude Code in the repository of the reviewed app (the plugin works in every repository).
4. Run the skill with the export folder, for example `/wablazor:wa-review-apply Downloads/wa-review/2026-10-20_143012`.
5. Read the report of the agent. It tells for each item: done, changed differently and why, or not done and why.
6. Restart or reload the app. Reload the page with the inspector open. Items whose attribute and class changes are
   now in the code show `applied` in the **Changes** tab.

## Metadata attributes

When `EmitInspectorMetadata` is on, the root element of every wrapper (components and layout wrappers) gets these
attributes:

| Attribute | Content |
|---|---|
| `data-wablazor-component` | The wrapper type name, for example `WaButton` |
| `data-wablazor-chain` | The readable chain of the application components from the render root, without framework types, for example `MainLayout>OrderPage>WaCard>WaButton` |
| `data-wablazor-placement` | A hash of the full path from the render root: the same for all instances that one loop renders, different for the same markup at another position |
| `data-wablazor-site` | A hash of the last level only: the source position in the rendering parent, the same wherever that parent is used |

If the renderer internals that the metadata reads are not available (another .NET version, trimmed members), a
wrapper writes `data-wablazor-component` only.

## How matching works

An item stores a locator of the element: the tag, the Blazor metadata, a structural key, a CSS path, a text
snippet, the classes, the id, the slot and the CSS isolation scope attributes (`b-xxxxxxxxxx`). Each item has a
scope:

| Scope | Matches |
|---|---|
| `placement` | All elements with the same `data-wablazor-placement`. Without Blazor metadata: all elements with the same structural key, which is made from the tag, the stable id, the classes, the slot and the scope attributes of the element and its ancestors, without positions. |
| `site` | All elements with the same `data-wablazor-site` (Blazor metadata only) |
| `tag` | All elements with the tag (`wa-` elements only) |
| `instance` | One element: the same text snippet first, else the same position in the placement group |

The review session of each origin is in the storage of the extension. The content script applies the session to
every page of the origin, and a mutation observer applies it again after the page changes. So the changes stay
visible while the reviewer navigates and after a reload. The page's own attribute values are kept: the panel shows
them, and the export has them as `from`.

When Lit sees a non-boolean attribute removed, it sets the property to null, not to its default. The inspector
follows the sticky attribute rule of the wrappers (`docs/technical/technical.md`, "Render tree discipline"): when the page value
"not set" comes back after you delete an override, or when an override is "(not set)", and the attribute is on a
`wa-` element now, the inspector writes the element default from the metadata instead of a removal. The panel shows
such a value as "not set". Boolean attributes are present or absent. Attributes without a literal default (for
example `new Date()`) are removed. An override from an earlier version of the extension has no default and is
removed too.

An attribute is data-driven when the instances of one placement have different values, or when the page changes
the value. For a data-driven attribute, the panel offers a value remap ("value X becomes Y") instead of one value.

The status of an item compares the page's own values with the item: `pending` (the page does not have the values),
`applied` (the page has them on its own), `unchecked` (the item has no attribute or class change to compare: CSS,
parts, style, remaps and comments), `missing` (no element on this page matches).

### Unsupported

- A data-driven value that is the same on all instances on the current page looks like a literal value.
- One instance of a loop cannot get its own literal value. Use a comment or a value remap.
- Property-only settings (no attribute) and slot content are read-only.
- `wa-` elements inside the shadow root of another `wa-` element are shown as internal and are not edited.
- Placements are stable for one build. A change of the Razor markup can change them. Old exports stay readable,
  because the locator also has the chain, the classes and a text snippet.

## Maintenance

After each Web Awesome upgrade, when `src/WebAwesome.Blazor.Tests/ApiParity/expected-api-surface.json` has the new
version, make the element metadata again and commit the result:

```powershell
.\tools\wa-inspector\Update-WaInspectorMetadata.ps1
```

The script reads the API surface and, by default, the extracted release under `temp/wa-src/<version>/dist`
(`custom-elements.json`, `styles/utilities/*.css`, `styles/themes/default.css`). Use `-DistPath` for another
release folder. It writes `tools/wablazor-marketplace/plugins/wablazor/assets/wa-inspector-extension/metadata/wa-metadata.json`.
Keep the script ASCII-only. The upgrade process (`docs/contributing/UPGRADE-PROCESS.md`, Phase 3) includes this step.
Raise the version of the plugin `wablazor` and publish it, so that the users get the new metadata.

## Browser tests

The browser tests (`tools/e2e/inspector/`) load the extension in Chromium and run against the server demo app with
the metadata on. They have their own Playwright configuration (`tools/e2e/playwright.inspector.config.js`) and are
not part of the standard e2e sweep or the release preflight. The tests use `http://localhost:5100`; the
environment variable `DEMO_BASE_URL` sets another address. Install the browser once with `npm run install-browsers`
in `tools/e2e`.

1. Start the server demo app with the metadata on (it listens on `http://localhost:5100`):

   ```powershell
   $env:WebAwesome__EmitInspectorMetadata = 'true'
   dotnet run --project src\WebAwesome.Blazor.Demo.Server\WebAwesome.Blazor.Demo.Server.csproj --launch-profile http
   ```

2. In `tools/e2e`, run the tests:

   ```powershell
   npm run test:inspector
   ```
