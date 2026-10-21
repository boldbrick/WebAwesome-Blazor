# WebAwesome.Blazor plugin marketplace

A local Claude Code plugin marketplace with the name `wablazor-marketplace`. It holds the Claude Code plugins of this
repository.

| Folder or file | Content |
|---|---|
| `.claude-plugin/marketplace.json` | The marketplace manifest |
| `plugins/wablazor/` | The plugin `wablazor`: the developer skills of WebAwesome.Blazor |
| `Publish-WaBlazorPlugin.ps1` | Publishes the plugins into another marketplace folder |

The plugin `wablazor` has these skills:

| Skill | Invocation | Function |
|---|---|---|
| `wa-review-apply` | `/wablazor:wa-review-apply <export folder>` | Applies a WA Inspector change list to the source code of the reviewed app ([WA Inspector](../../docs/inspector/README.md)) |

More skills will follow.

## Install from the repository

Add the marketplace once, and install the plugin:

```
claude plugin marketplace add <repository>/tools/wablazor-marketplace
claude plugin install wablazor@wablazor-marketplace
```

After a change of the plugin files, run `claude plugin marketplace update wablazor-marketplace` and
`claude plugin update wablazor@wablazor-marketplace`. Claude Code updates an installed plugin only when its version
changes. Raise `version` in `plugins/wablazor/.claude-plugin/plugin.json` for each change that the users must get.

Validate the plugin and the marketplace after each change:

```
claude plugin validate tools/wablazor-marketplace/plugins/wablazor --strict
claude plugin validate tools/wablazor-marketplace --strict
```

## Publish into another marketplace

`Publish-WaBlazorPlugin.ps1` copies the plugins into a file-based marketplace folder, for example a folder that a team
shares. The target folder is a required parameter. The script has no default target.

```powershell
# show what the script does, and change no file
.\tools\wablazor-marketplace\Publish-WaBlazorPlugin.ps1 -Destination <marketplace folder> -WhatIf

# raise the patch version of wablazor and publish it
.\tools\wablazor-marketplace\Publish-WaBlazorPlugin.ps1 -Destination <marketplace folder> -PluginName wablazor -BumpVersion Patch
```

Instead of `-Destination`, you can set the environment variable `WABLAZOR_PLUGIN_MARKETPLACE`.

The script:

1. Raises the version in `plugin.json` when you give `-BumpVersion Major`, `Minor` or `Patch`.
2. Validates each plugin with `claude plugin validate --strict` (`-SkipValidation` skips this step).
3. Copies each plugin into the subfolder `<name>_v<version>` of the target. An existing subfolder with other files is
   replaced, with a warning: the users get the change only with a new version.
4. Writes one entry for each plugin into `.claude-plugin/marketplace.json` of the target: `name`, `source`
   (`./<name>_v<version>`), `version`, and `description` and `category` from the entry in this marketplace. The
   entry of an older version is replaced. If the target has no `marketplace.json`, the script makes one.
5. Validates the target marketplace with `claude plugin validate`.

The users of the target marketplace then run `claude plugin marketplace update <target marketplace name>` and
install or update `wablazor@<target marketplace name>`.

The script runs in Windows PowerShell 5.1 and in PowerShell 7. Keep it ASCII-only.
