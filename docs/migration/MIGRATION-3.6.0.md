# Web Awesome Blazor 3.6.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.5.0 to 3.6.0, covering the one breaking change and the additive features.

## Breaking Changes

### 1. WaFileInput — `file-icon` slot removed (BREAKING)

**Impact**: Low — affects only file inputs that supplied a custom file icon.

Web Awesome 3.6.0 removed the `file-icon` slot from `wa-file-input`, so the wrapper's `FileIconContent` render fragment has been removed.

#### Migration

**Before (3.5.0):**
```razor
<WaFileInput Label="Upload">
    <FileIconContent>
        <WaIcon Name="cloud-arrow-up" />
    </FileIconContent>
</WaFileInput>
```

**After (3.6.0):**
```razor
<!-- The file-icon slot no longer exists upstream; remove the FileIconContent fragment. -->
<WaFileInput Label="Upload" />
```

If you need custom dropzone visuals, use the still-supported `DropzoneContent` slot instead.

#### Code Search & Replace
1. Remove all `<FileIconContent>…</FileIconContent>` fragments from `WaFileInput` usages.

## New Features

### 1. Expanded size scale — `WaSize.ExtraSmall` / `WaSize.ExtraLarge`

Web Awesome 3.6.0 widened the size scale on the sized components (buttons, form controls, callouts, tags, dropdowns, toast items, …) to `xs | s | m | l | xl` (the legacy `small | medium | large` values remain valid aliases). The `WaSize` enum gained two members:

- `WaSize.ExtraSmall` → renders `size="xs"`
- `WaSize.ExtraLarge` → renders `size="xl"`

```razor
<WaButton Size="WaSize.ExtraSmall">Tiny</WaButton>
<WaButton Size="WaSize.Small">Small</WaButton>
<WaButton Size="WaSize.Large">Large</WaButton>
<WaButton Size="WaSize.ExtraLarge">Huge</WaButton>
```

The existing `WaSize.Small` / `WaSize.Medium` / `WaSize.Large` members are unchanged and continue to render `small` / `medium` / `large`. Upstream also renamed the default size string from `medium` to `m`; this is behavior-preserving (the same rendered size) and requires no action — the wrapper still omits the attribute when `Size` is not set.

### 2. WaNumberInput — `OnBeforeInput` event

`wa-number-input` gained a cancelable `beforeinput` event, exposed as the `OnBeforeInput` callback. It fires before the value changes.

```razor
<WaNumberInput @bind-Value="quantity" OnBeforeInput="HandleBeforeInput" />

@code {
    private decimal? quantity;

    private void HandleBeforeInput(EventArgs args)
    {
        // inspect / react before the value changes
    }
}
```

## Migration Checklist

- [ ] Update `WaFileInput` usages: remove `FileIconContent` fragments
- [ ] (Optional) Adopt `WaSize.ExtraSmall` / `WaSize.ExtraLarge` where the extra sizes help
- [ ] (Optional) Wire `WaNumberInput.OnBeforeInput` where a pre-change hook is useful
- [ ] Test all changes thoroughly; update unit tests if needed

## Automated Migration

### Find/Replace Patterns

1. **WaFileInput file-icon removal** (multi-line): remove any `<FileIconContent> … </FileIconContent>` block inside a `<WaFileInput>`.

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.6.0+
- **Breaking Changes**: Yes — `WaFileInput.FileIconContent` removed
- **New Dependencies**: None
