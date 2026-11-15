---
name: install-inspector
description: Install or update the WA Inspector browser extension (Chrome or Edge, Windows) with the plugin wablazor. Uses the extension that the published plugin bundles, or asks for the extension folder. Copies the extension to %LOCALAPPDATA%\WABlazor\wa-inspector-extension, opens the browser, puts the folder path on the clipboard and tells the steps on the extensions page.
argument-hint: "[Chrome | Edge | Both]"
disable-model-invocation: true
---

# Install or update the WA Inspector extension

A published plugin bundles the extension in `${CLAUDE_PLUGIN_ROOT}/assets/wa-inspector-extension`. A plugin that the
user installed from the marketplace of a repository checkout does not have this folder: then the source is the
folder `tools\wa-inspector\extension` of a WebAwesome.Blazor checkout.

The script `${CLAUDE_PLUGIN_ROOT}/skills/install-inspector/Install-WaInspector.ps1` copies the extension to the stable
folder `%LOCALAPPDATA%\WABlazor\wa-inspector-extension`. The stable folder keeps the extension id and the saved
review sessions across updates. The browser lets only the user load an unpacked extension: never try to do it
yourself. The browsers do not open `chrome://extensions` or `edge://extensions` from the command line, so the script
opens a new browser window and the user types the address.

This skill works on Windows only. On another system, tell the user to load the extension folder (the bundled folder,
or `tools/wa-inspector/extension` of a WebAwesome.Blazor checkout) with **Load unpacked** on the extensions page,
and stop.

## Steps

1. Find the extension source (PowerShell):

   ```powershell
   Test-Path "${CLAUDE_PLUGIN_ROOT}/assets/wa-inspector-extension/manifest.json"
   ```

   - `True`: use the bundled extension. Do not give `-Source` in the next steps.
   - `False`: the plugin has no bundled extension. Ask the user for the extension source folder, for example
     `<repository>\tools\wa-inspector\extension` of a WebAwesome.Blazor checkout. Make sure that the folder holds a
     `manifest.json` with `"name": "WA Inspector"`. If it does not, tell the user and ask again, or stop. Give the
     folder as `-Source "<folder>"` in the next steps.
2. Run the script with `-ListBrowsers`, plus `-Source` from step 1 when it applies:

   ```powershell
   & "${CLAUDE_PLUGIN_ROOT}/skills/install-inspector/Install-WaInspector.ps1" -ListBrowsers
   ```

   It writes the installed browsers, the source and the state: `FirstInstall`, `Update` or `UpToDate`. It changes
   nothing.
3. Select the browser:
   - The argument of the skill (`$ARGUMENTS`) is `Chrome`, `Edge` or `Both`: use it.
   - Else, if only one browser is installed, use it.
   - Else, ask the user: Chrome, Edge or both.
   - If neither is installed, tell the user that the extension needs Chrome or Edge 116 or later, and stop.
4. Run the script with the browser, plus `-Source` from step 1 when it applies:

   ```powershell
   & "${CLAUDE_PLUGIN_ROOT}/skills/install-inspector/Install-WaInspector.ps1" -Browser <Chrome|Edge|Both>
   ```

5. Give the user the result of the script in short form: the installed version, the folder path (now on the
   clipboard), the next steps for the state (the address of the extensions page included), the reminder to reload
   the pages that were open before, and the notes (the developer-mode warning of the browser, and how to remove the
   extension).

If the script fails, show the error and do not work around it.
