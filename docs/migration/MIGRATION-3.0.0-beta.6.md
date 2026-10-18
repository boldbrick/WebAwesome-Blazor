# Web Awesome Blazor 3.0.0-beta.6 Migration Guide

This guide helps you migrate from Web Awesome Blazor 3.0.0-beta.4 to 3.0.0-beta.6, covering breaking changes and new features.

## Breaking Changes

### 1. WaIcon Component - Font Awesome 7 Changes (BREAKING)

**Impact**: High - affects Font Awesome icon usage

#### Changes
- **Removed**: `FixedWidth` property (now default behavior in FA7)
- **Added**: `AutoWidth` property to allow variable width icons
- **Added**: `SwapOpacity` property for duotone icons

#### Migration

**Before (beta.4):**
```razor
<WaIcon Name="home" FixedWidth="true" />
<WaIcon Name="user" FixedWidth="false" />
```

**After (beta.6):**
```razor
<!-- Fixed width is now default - no property needed -->
<WaIcon Name="home" />

<!-- Use AutoWidth for variable width -->
<WaIcon Name="user" AutoWidth="true" />

<!-- New: SwapOpacity for duotone icons -->
<WaIcon Name="heart" SwapOpacity="true" />
```

#### Code Search & Replace
1. Remove all `FixedWidth="true"` usages (now default)
2. Replace `FixedWidth="false"` with `AutoWidth="true"`

### 2. WaDetails Component - Property Rename (BREAKING)

**Impact**: Medium - affects details component usage

#### Changes
- **Renamed**: `IconPosition` property to `IconPlacement`
- **Updated**: Enum from `WaIconPosition` to `WaIconPlacement`

#### Migration

**Before (beta.4):**
```razor
<WaDetails Summary="Click me" IconPosition="WaIconPosition.Start">
    Content here
</WaDetails>
```

**After (beta.6):**
```razor
<WaDetails Summary="Click me" IconPlacement="WaIconPlacement.Start">
    Content here
</WaDetails>
```

#### Code Search & Replace
1. Replace `IconPosition` with `IconPlacement`
2. Replace `WaIconPosition` with `WaIconPlacement`

### 3. WaButtonGroup Component - Size Removal (BREAKING)

**Impact**: Low - affects button group sizing

#### Changes
- **Removed**: `Size` property (inconsistent with dynamic button updates)

#### Migration

**Before (beta.4):**
```razor
<WaButtonGroup Size="WaSize.Large">
    <WaButton>Button 1</WaButton>
    <WaButton>Button 2</WaButton>
</WaButtonGroup>
```

**After (beta.6):**
```razor
<!-- Apply size to individual buttons instead -->
<WaButtonGroup>
    <WaButton Size="WaSize.Large">Button 1</WaButton>
    <WaButton Size="WaSize.Large">Button 2</WaButton>
</WaButtonGroup>
```

#### Recommendation
Apply the `Size` property directly to individual `WaButton` components for consistent behavior when buttons are dynamically added or removed.

## New Features

### 1. WaIntersectionObserver Component (NEW)

Monitor when elements enter or leave the viewport for lazy loading and scroll animations.

```razor
<WaIntersectionObserver
    Threshold="0.0 0.5 1.0"
    RootMargin="10px"
    IntersectClass="visible"
    OnIntersect="HandleIntersection">

    <div>Content to observe</div>
    <img src="placeholder.jpg" data-src="actual-image.jpg" />

</WaIntersectionObserver>

@code {
    private void HandleIntersection(WaIntersectionEventArgs args)
    {
        if (args.IsIntersecting)
        {
            // Element is visible
            Console.WriteLine($"Intersection ratio: {args.IntersectionRatio}");
        }
    }
}
```

### 2. WaCard Component Enhancements

New orientation support and action slots for improved card layouts.

```razor
<!-- New horizontal orientation -->
<WaCard Orientation="WaOrientation.Horizontal" Appearance="WaAppearance.Outlined">

    <!-- Enhanced header with actions -->
    <HeaderContent>
        <h3>Card Title</h3>
    </HeaderContent>

    <HeaderActionsContent>
        <WaButton Size="WaSize.Small">Action</WaButton>
    </HeaderActionsContent>

    <!-- Main content -->
    <div>Card body content here</div>

    <!-- Enhanced footer with actions -->
    <FooterContent>
        <span>Footer text</span>
    </FooterContent>

    <FooterActionsContent>
        <WaButton Variant="WaVariant.Success">Save</WaButton>
        <WaButton Variant="WaVariant.Neutral">Cancel</WaButton>
    </FooterActionsContent>

</WaCard>
```

### 3. WaDropdownItem Variant Support

Enhanced styling options for dropdown items.

```razor
<WaDropdown>
    <WaDropdownItem>Regular item</WaDropdownItem>
    <WaDropdownItem Variant="WaVariant.Success">Success item</WaDropdownItem>
    <WaDropdownItem Variant="WaVariant.Danger">Danger item</WaDropdownItem>
</WaDropdown>
```

## Migration Checklist

- [ ] Update WaIcon components:
  - [ ] Remove `FixedWidth="true"` (now default)
  - [ ] Replace `FixedWidth="false"` with `AutoWidth="true"`
  - [ ] Add `SwapOpacity="true"` for duotone icons if needed
- [ ] Update WaDetails components:
  - [ ] Replace `IconPosition` with `IconPlacement`
  - [ ] Update enum references
- [ ] Update WaButtonGroup components:
  - [ ] Remove `Size` property
  - [ ] Apply size to individual buttons
- [ ] Test all changes thoroughly
- [ ] Update unit tests if needed

## Automated Migration

You can use these search/replace patterns in your IDE:

### Find/Replace Patterns

1. **WaIcon FixedWidth removal:**
   - Find: `FixedWidth="true"`
   - Replace: *(delete)*

2. **WaIcon AutoWidth:**
   - Find: `FixedWidth="false"`
   - Replace: `AutoWidth="true"`

3. **WaDetails property rename:**
   - Find: `IconPosition="`
   - Replace: `IconPlacement="`

4. **Enum rename:**
   - Find: `WaIconPosition.`
   - Replace: `WaIconPlacement.`

## Support

If you encounter issues during migration:

1. Check the [Web Awesome documentation](https://webawesome.com)
2. Review component API changes in this guide
3. Test changes in a development environment first
4. Ensure all tests pass after migration

## Version Compatibility

- **Minimum .NET**: .NET 9.0
- **Web Awesome Core**: 3.0.0-beta.6+
- **Breaking Changes**: Yes (see above)
- **New Dependencies**: None