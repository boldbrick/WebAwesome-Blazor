# Releasing and Publishing

How a finished version leaves the building: from a released changeset in Plastic to a package on nuget.org and the demo app on GitHub Pages. Publication is always **owner-triggered** — no pipeline or agent publishes anything.

## Release flow (per release)

1. **Promote in Plastic**: merge the train's subtrunk (e.g. `/main/WA-3.0`) into `/main` (release gate and patch rules: see the promotion model in `README.md` and `CONTRIBUTING.md`). Label the released changeset `wa-blazor-<version>` — the version must equal `src\Version.props`.
2. **Sync the GitHub mirror**: push `/main` and the label to the `boldbrick/WebAwesome-Blazor` mirror, so the git tag `wa-blazor-<version>` exists there.
3. **CI takes over** (`.github\workflows\build.yml` on the tag):
   - `build` job: tag-guard (tag version must equal `Version.props`), restore, build, **test**, pack (`.nupkg` + `.snupkg`).
   - `publish` job (gated, see below): authenticates to nuget.org via **Trusted Publishing**, pushes the packages, and creates the GitHub Release with the packages attached.
4. **Demo app**: the same push to `main` triggers `.github\workflows\demo.yml`, which publishes the Blazor WebAssembly demo to GitHub Pages (<https://boldbrick.github.io/WebAwesome-Blazor/>).

## One-time setup on nuget.org and GitHub (owner actions)

The publish job authenticates with **nuget.org Trusted Publishing** (OIDC): nuget.org trusts this repository's workflow directly and issues a short-lived API key per run via the `NuGet/login` action. No long-lived API key is created or stored.

1. **nuget.org account**: enable two-factor authentication (required by nuget.org for publishing). An organization is not needed to start — the package is created under the personal account on first push; a BoldBrick organization can be created later and added as package co-owner without republishing.
2. **Trusted Publishing policy**: on nuget.org, profile menu → *Trusted Publishing* → add a policy:
   - Package owner: the account (or, later, the organization)
   - Repository owner: `boldbrick`
   - Repository: `WebAwesome-Blazor`
   - Workflow file: `build.yml`
   - Environment: leave empty (the workflow does not use one)
3. **GitHub repository variables** (mirror repo → Settings → Secrets and variables → Actions → Variables):
   - `NUGET_USER` = the nuget.org profile name that owns the policy
   - `NUGET_PUBLISH_ENABLED` = `true` — this is the go-live switch; while unset/false the publish job is skipped entirely
   - The former `NUGET_API_KEY` secret is not used and must not be created.
4. **GitHub Pages**: repo → Settings → Pages → Source = *GitHub Actions* (required by `demo.yml`).
5. **After the first successful publish**: activate the commented-out NuGet badge in `README.md`.

### Package identity and signing

- The `WebAwesome.Blazor` package id comes into existence on the first push and is owned by the pushing account. Reserving the `WebAwesome.*` **id prefix** is optional, subject to nuget.org's reservation criteria, and can be requested any time later.
- **Signing**: nuget.org repository-signs every accepted package automatically. Author signing is deliberately not set up — it requires a purchased code-signing certificate and adds no acceptance requirement on nuget.org; revisit only if a consumer policy demands author signatures.

### Fallback: classic API key

If Trusted Publishing is unavailable, create an API key on nuget.org (scope *Push new packages and package versions*, glob `WebAwesome.*`, maximum 365-day expiry), store it as the `NUGET_API_KEY` repository secret, and change the publish job to pass `${{ secrets.NUGET_API_KEY }}` to `dotnet nuget push` instead of the `NuGet/login` output. Trusted Publishing remains the preferred setup — keys expire and leak; OIDC policies do neither.
