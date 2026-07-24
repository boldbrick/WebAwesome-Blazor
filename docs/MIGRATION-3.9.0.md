# Web Awesome Blazor 3.9.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.8.0 to 3.9.0. The upgrade is **additive** — no wrapper APIs were removed or renamed. There is one new component (`WaCheckboxGroup`) and one new enum value on `WaTree`; existing code compiles and behaves unchanged.

## Behavioral Changes

None. No existing parameter, enum value, event, or default was changed.

### WaTree — `selection` gains a `leaf-multiple` option (no action)

Web Awesome 3.9.0 widened the tree's `selection` attribute type from `'single' | 'multiple' | 'leaf'` to add `'leaf-multiple'`. This surfaces as a new `WaTreeSelection.LeafMultiple` enum member. The existing `Single`/`Multiple`/`Leaf` values and the `Single` default are unchanged, so no migration is required — the new value is purely opt-in:

```razor
<!-- new in 3.9.0: multiple leaf nodes selectable, parents only expand/collapse -->
<WaTree Selection="WaTreeSelection.LeafMultiple">
    <WaTreeItem>Fruits
        <WaTreeItem>Apple</WaTreeItem>
        <WaTreeItem>Banana</WaTreeItem>
    </WaTreeItem>
</WaTree>
```

## New Components

### Checkbox group — `WaCheckboxGroup`

`WaCheckboxGroup` gives a set of related `WaCheckbox` (or `WaSwitch`) items a shared label, hint, sizing, and accessible grouping. It is a **grouping wrapper, not a form control**: each grouped checkbox remains an independent control with its own `Name`/value/validation, so `WaCheckboxGroup` itself has **no value binding and no events** — do not attempt to `@bind-Value` it.

```razor
<WaCheckboxGroup Label="Toppings" Hint="Choose as many as you like." Orientation="WaOrientation.Vertical">
    <WaCheckbox Name="pepperoni">Pepperoni</WaCheckbox>
    <WaCheckbox Name="mushrooms">Mushrooms</WaCheckbox>
    <WaCheckbox Name="onions">Onions</WaCheckbox>
</WaCheckboxGroup>
```

- `Label` / `Hint` — plain-text label and hint (use the `MarkupLabel` / `MarkupHint` slots for HTML content). `Label` is required for accessibility.
- `Orientation` (`WaOrientation`) — `Vertical` (default) or `Horizontal`.
- `Size` (`WaSize`) — applied to **all** grouped checkboxes and switches, overriding any size set on the individual items.
- `Required` — adds a visual indicator to the group label only; because each checkbox is independent, the group does not enforce the requirement. Set `Required` on the individual `WaCheckbox` (or use its custom-validity API) to enforce it.
- `WithHint` / `WithLabel` — SSR hydration hints; set them when slotting `MarkupHint` / `MarkupLabel` so the server-rendered markup includes them before hydration.

The group also works with switches:

```razor
<WaCheckboxGroup Label="Notifications" Hint="Pick at least one channel.">
    <WaSwitch Name="email">Email</WaSwitch>
    <WaSwitch Name="sms">SMS</WaSwitch>
    <WaSwitch Name="push">Push</WaSwitch>
</WaCheckboxGroup>
```

## New Attributes on Existing Components

None (the only change to an existing component is the additive `WaTreeSelection.LeafMultiple` enum value described above).

## Migration Checklist

- [ ] (Optional) Adopt `WaCheckboxGroup` to give related checkboxes/switches a shared label and hint
- [ ] (Optional) Use `WaTreeSelection.LeafMultiple` where multiple leaf selection is desired
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.9.0+
- **Breaking Changes**: None
- **New Dependencies**: None
