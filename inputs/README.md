# Inputs

This folder contains _input resources_ used as a reference to build the wrappers.

**`WebAwesome` folder:**
- Contains: the respective version of the documentation of Web Awesome components, refreshed by `tools\upgrade\Sync-WaDocs.ps1 -Version <version>` on every upgrade
- Primary source: the [Web Awesome GitHub repo](https://github.com/shoelace-style/webawesome) at tag `v<version>`, contents of the `packages/webawesome/docs/docs` folder. This is the full docs site source, including the non-component pages (tokens, utilities, frameworks, theming).
- Gap fill for components missing from that tree (the Pro components): the component reference docs bundled in the release zip (`dist/skills/webawesome/references/components/<name>.md`, present since WA 3.3.0), marked with a source comment at the top of each file. Only when a component has neither falls back to carrying the previous version's doc forward, or to a manual capture from `https://webawesome.com/docs/components/<name>` (reported by the script as NEEDS CAPTURE).
- When a release is published before its public GitHub tag, `-DocsTagVersion <older>` takes the non-component pages from an older tag and makes the bundled references take precedence over that tree's component docs (`-PreferBundledRefs`).

# Upgrading instructions

Upgrades are automated: run the `/wa-upgrade` skill, which refreshes this folder as part of its pipeline. See `docs\UPGRADE-PROCESS.md` for the full process. (The former manual flow — version bump, doc load, `cm patch` diff, prompt-template planning — is superseded; its historical artifacts remain under `docs\prompts\WA-3.0\`.)