# WebAwesome.Blazor plugin marketplace

A local Claude Code plugin marketplace with the name `wablazor-marketplace`. It holds the Claude Code plugins of this
repository.

| Folder or file | Content |
|---|---|
| `.claude-plugin/marketplace.json` | The marketplace manifest |
| `plugins/wablazor/` | The plugin `wablazor`: the developer skills of WebAwesome.Blazor |
| `bundles.json` | The bundle manifest: the files from outside a plugin folder that the published plugin gets |
| `Publish-WaBlazorPlugin.ps1` | Publishes the plugins, with their bundles, into another marketplace folder |

The plugin `wablazor` has these skills:

| Skill | Invocation | Function |
|---|---|---|
| `install-inspector` | `/wablazor:install-inspector [Chrome \| Edge \| Both]` | Installs or updates the WA Inspector extension for Chrome or Edge on Windows. Only the user can run it. |
| `wa-review-apply` | `/wablazor:wa-review-apply <export folder>` | Applies a WA Inspector change list to the source code of the reviewed app ([WA Inspector](../../docs/inspector/README.md)) |

More skills will follow.

## Install from the repository

Add the marketplace once, and install the plugin:

```
claude plugin marketplace add <repository>/tools/wablazor-marketplace
claude plugin install wablazor@wablazor-marketplace
```

Then run `/wablazor:install-inspector` to install the WA Inspector extension. A plugin from this marketplace has no
bundled extension, so the skill asks you for the extension folder: give `tools/wa-inspector/extension` of the
repository.

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
2. Stages each plugin in a temporary folder: a copy of the plugin folder plus its bundles from `bundles.json`.
3. Validates the staged copy with `claude plugin validate --strict` (`-SkipValidation` skips this step).
4. Copies the staged copy into the subfolder `<name>_v<version>` of the target. An existing subfolder with the same
   files stays. An existing subfolder with other files is replaced, with a warning: the users get the change only
   with a new version.
5. Writes one entry for each plugin into `.claude-plugin/marketplace.json` of the target: `name`, `source`
   (`./<name>_v<version>`), `version`, and `description` and `category` from the entry in this marketplace. The
   entry of an older version is replaced. If the target has no `marketplace.json`, the script makes one.
6. Validates the target marketplace with `claude plugin validate`.

`-WhatIf` also stages and validates, and shows the bundle copies, but changes no file in the repository or in the
target.

### Bundles

`bundles.json` adds files from outside a plugin folder to the published plugin. The repository keeps one source of
each file, and the published plugin is complete without a repository checkout:

```json
{
  "wablazor": [
    { "source": "../wa-inspector/extension", "target": "assets/wa-inspector-extension" }
  ]
}
```

- The key is the plugin name.
- `source` is a file or a folder, relative to this marketplace folder. A missing source stops the script.
- `target` is relative to the plugin root. It must stay inside the plugin and must not exist in the plugin folder.

The bundle of `wablazor` is the WA Inspector extension (`tools/wa-inspector/extension`). The skill
`install-inspector` installs it from `assets/wa-inspector-extension` of the published plugin. A change of a bundle
source changes the published plugin: when the extension changes, raise the plugin version before the next publish.

The users of the target marketplace then run `claude plugin marketplace update <target marketplace name>` and
install or update `wablazor@<target marketplace name>`.

The script runs in Windows PowerShell 5.1 and in PowerShell 7. Keep it ASCII-only.
