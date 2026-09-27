# Web Awesome Blazor 3.14.0 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.13.0 to 3.14.0.

The Web Awesome 3.14.0 release is additive: a new stepper (`wa-stepper` with its `wa-step` children), a server mode for the combobox, a divider label and a few new attributes. The bindings make **one breaking change**: the step value type `WaStep` is renamed to `WaValueStep`. That frees the name `WaStep` for the wrapper of `wa-step`, so it follows the same rule as every other component (`wa-<name>` is wrapped by `Wa<Name>`), next to its parent `WaStepper`.

## Breaking Changes

### 1. The step value `WaStep` is renamed `WaValueStep` (BREAKING)

**Impact**: Low. You're only affected if your code spells out the type name: `WaStep.Any`, a `WaStep`/`WaStep?` field, variable, parameter or cast, or a `DefaultStep` you store in a typed variable. A numeric `Step` value in Razor (`Step="5"`, `Step="0.5"`) converts implicitly, as before, and needs no change.

`WaValueStep` (introduced as `WaStep` in 3.12.0, see [MIGRATION-3.12.0.md](MIGRATION-3.12.0.md) section 12) is the `number | 'any'` step of `WaInput`, `WaNumberInput` and `WaTimeInput`: a positive number, or `WaValueStep.Any`. Only the name changes: its members, conversions, validation, wire form and equality are unchanged, and so are the `Step` parameters and the `WaNumberInput.DefaultStep`/`WaTimeInput.DefaultStep` constants, which are now typed `WaValueStep`.

In 3.14.0 `WaStep` is the new component wrapping `wa-step`. Code that still means the value type doesn't compile, so the compiler finds every place to change. Typical errors:
- `'WaStep' does not contain a definition for 'Any'` for `Step="WaStep.Any"`
- `Cannot implicitly convert type 'double' to 'WebAwesome.Blazor.Components.WaStep'` for `WaStep? step = 0.5;`
- `Cannot implicitly convert type 'WebAwesome.Blazor.Components.WaValueStep' to 'WebAwesome.Blazor.Components.WaStep'` where a `Step` or `DefaultStep` value is assigned to a `WaStep` variable

**Before (3.13.0):**
```razor
<WaNumberInput Step="WaStep.Any" @bind-Value="amount" />
<WaTimeInput Step="@withSeconds" @bind-Value="start" />

@code {
    private decimal? amount;
    private TimeOnly? start;
    private WaStep? withSeconds = 1;
}
```

**After (3.14.0):**
```razor
<WaNumberInput Step="WaValueStep.Any" @bind-Value="amount" />
<WaTimeInput Step="@withSeconds" @bind-Value="start" />

@code {
    private decimal? amount;
    private TimeOnly? start;
    private WaValueStep? withSeconds = 1;
}
```

**Mechanical fix.** Before 3.14.0, the name `WaStep` meant only the value type. So as long as you haven't added any stepper markup yet, every whole-word `WaStep` in your code (`.cs` and `.razor`) refers to it, and one replacement fixes them all. With a regular expression, replace `\bWaStep\b` with `WaValueStep`. The word boundary leaves `WaStepper`, `WaStepperOrientation` and `WaStepChangeEventArgs` alone, and none of them existed before 3.14.0 anyway. Do the rename first, then start using `<WaStep>` inside `<WaStepper>`.

## New Components

### Stepper — `WaStepper` and `WaStep`

`WaStepper` (`wa-stepper`, free/experimental) shows the stages of a process; each stage is a `WaStep` (`wa-step`) child. `Active` names the current step by its `Name`. A stepper is a display, not a form control: drive it with your own buttons calling `NextAsync`, `PreviousAsync` or `GoToAsync`, or set `Clickable` to let users click a step. `Linear` blocks advancing past a step that isn't completed yet, whether by a method call, a `data-stepper` invoker or a click.

```razor
<WaStepper @ref="stepper" Active="shipping" Label="Checkout progress">
    <WaStep Name="cart" Completed="true">Cart</WaStep>
    <WaStep Name="shipping">Shipping</WaStep>
    <WaStep Name="payment">Payment</WaStep>
</WaStepper>

<WaButton OnClick="() => stepper!.PreviousAsync()">Back</WaButton>
<WaButton OnClick="() => stepper!.NextAsync()">Next</WaButton>

@code {
    private WaStepper? stepper;
}
```

- `WaStepper`: `Active`, `Clickable`, `Linear`, `Orientation` (`WaStepperOrientation`), `Label`; `OnBeforeStepChange` and `OnStepChange` carry a `WaStepChangeEventArgs` with the `Name` of the step becoming active and the `PreviousName`. `OnBeforeStepChange` can't cancel the change from .NET, like the other cancelable Web Awesome events.
- `WaStep`: `Name`, `Completed`, `Loading`, `Disabled`, `Variant`, `Attention`, `DescriptionContent`, and an icon through `IconContent` or the `IconName` shortcut.

The other 3.14.0 additions (`WaCombobox` server mode, the `WaDivider` label, `WithLabel` on `WaDialog`/`WaDrawer`/`WaDivider`, `WaPage.Nonce`, and `Allow`/`Label`/`Name` on `WaZoomableFrame`) are opt-in and don't change existing behaviour; see the [changelog](CHANGELOG.md).

## Migration Checklist

- [ ] Replace every whole-word `WaStep` with `WaValueStep` (`Step="WaValueStep.Any"`, `WaValueStep?` fields and variables) before adding stepper markup (section 1)
- [ ] Build: any remaining `WaStep` that means the value type fails to compile
- [ ] (Optional) Adopt `WaStepper` with `WaStep` children for multi-step flows
- [ ] Test all changes thoroughly; update unit tests if needed

## Version Compatibility

- **Minimum .NET**: .NET 9.0 (primary target .NET 10.0)
- **Web Awesome Core**: 3.14.0+
- **Breaking Changes**: Yes (section 1, a rename of the step value type)
- **New Dependencies**: None
