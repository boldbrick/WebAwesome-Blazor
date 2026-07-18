# Inputs

This folder contains _input resources_ used as a reference to build the wrappers.

**`WebAwesome` folder:**
- Contains: the respective version of the documentation of Web Awesome components
- Ingested from: [Web Awesome GitHub repo](https://github.com/shoelace-style/webawesome/tree/v3.0.0-beta.6), contents of the [`packages/webawesome/docs/docs` folder](https://github.com/shoelace-style/webawesome/tree/v3.0.0-beta.6/packages/webawesome/docs/docs)
- Pro components (e.g. `combobox`, `page`) are not documented in the public GitHub repo; their docs are ingested from the public pages at `https://webawesome.com/docs/components/<name>` instead (source URL noted at the top of each such file)

# Upgrading instructions

Upgrades are automated: run the `/wa-upgrade` skill, which refreshes this folder as part of its pipeline. See `docs\UPGRADE-PROCESS.md` for the full process. (The former manual flow — version bump, doc load, `cm patch` diff, prompt-template planning — is superseded; its historical artifacts remain under `docs\prompts\WA-3.0\`.)