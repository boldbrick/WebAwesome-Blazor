# WA Inspector

The maintenance tools of the WA Inspector, a browser extension for the UI review of Web Awesome elements.

| Folder or file | Content |
|---|---|
| `Update-WaInspectorMetadata.ps1` | The generator of the element metadata of the extension |

The extension itself ships with the Claude Code plugin `wablazor` of the marketplace
[`tools/wablazor-marketplace`](../wablazor-marketplace/README.md):

| Folder or file | Content |
|---|---|
| `plugins/wablazor/assets/wa-inspector-extension/` | The Chrome and Edge extension (Manifest V3, no build step) |
| `plugins/wablazor/assets/wa-inspector-extension/metadata/wa-metadata.json` | The generated element metadata of the panel forms |
| `plugins/wablazor/skills/install-inspector/` | The skill `/wablazor:install-inspector`, which installs or updates the extension |
| `plugins/wablazor/skills/wa-review-apply/` | The skill `/wablazor:wa-review-apply`, which applies the exported change list to the source code |

Documentation:

- [Overview, installation and maintenance](../../docs/inspector/README.md)
- [User guide](../../docs/inspector/user-guide.md)

After each Web Awesome upgrade, make the element metadata again and commit it:

```powershell
.\tools\wa-inspector\Update-WaInspectorMetadata.ps1
```

The script writes the metadata into the extension folder of the plugin. Raise the plugin version and publish the
plugin, so that the users get the new metadata.
