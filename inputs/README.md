# Inputs

This folder contains _input resources_ used as a reference to build the wrappers.

**`WebAwesome` folder:**
- Contains: the respective version of the documentation of Web Awesome components
- Ingested from: [Web Awesome GitHub repo](https://github.com/shoelace-style/webawesome/tree/v3.0.0-beta.6), contents of the [`packages/webawesome/docs/docs` folder](https://github.com/shoelace-style/webawesome/tree/v3.0.0-beta.6/packages/webawesome/docs/docs)

# Upgrading instructions

When upgrading to a newer WebAwesome version, follow these steps:
- Update `src\Version.props` and `src\README.md` to reflect the new version number.
- Load updated documentation into `inputs\WebAwesome`.
- Make patch diff. In the repo root, run `cm patch cs:<changeset> --tool="c:\Program Files\Git\usr\bin\diff.exe" > inputs\diff_3.0.0-beta.6.patch`, where `<changeset>` denotes the documentation update. As the diff can be reproduced at any time, **do not commit** to the repo.
- Prepare the upgrade instructions. Use the latest as a template, e.g. `docs\prompts\WA-3.0\upgrade-v3-beta-4-to-beta-6.md`.
- Let Claude Code work accordingt to these instructing, producing a plan in e.g. `docs\prompts\WA-3.0\upgrade-v3-beta-4-to-beta-6-plan.md`.
- Let Claude Code work on the plan, implementing changes step-by-step.