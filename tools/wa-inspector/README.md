# WA Inspector

A browser extension for the UI review of Web Awesome elements.

| Folder or file | Content |
|---|---|
| `extension/` | The Chrome and Edge extension (Manifest V3, no build step). Load it with **Load unpacked**. |
| `extension/metadata/wa-metadata.json` | The generated element metadata of the panel forms |
| `Update-WaInspectorMetadata.ps1` | The generator of the element metadata |

The skill `wa-review-apply` applies the exported change list to the source code. It is in the Claude Code plugin
`wablazor` of the marketplace [`tools/wablazor-marketplace`](../wablazor-marketplace/README.md).

Documentation:

- [Overview, installation and maintenance](../../docs/inspector/README.md)
- [User guide](../../docs/inspector/user-guide.md)

After each Web Awesome upgrade, make the element metadata again and commit it:

```powershell
.\tools\wa-inspector\Update-WaInspectorMetadata.ps1
```
