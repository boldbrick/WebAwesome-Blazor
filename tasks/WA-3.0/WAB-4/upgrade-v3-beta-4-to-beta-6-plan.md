# Web Awesome 3.0.0-beta.4 to beta.6 Upgrade Implementation Plan

## Overview
This document provides a comprehensive implementation plan for upgrading the Web Awesome Blazor wrapper library from version 3.0.0-beta.4 to 3.0.0-beta.6.

## Analysis Summary
Based on the documentation diff analysis (`diff_3.0.0-beta.6.patch`) and review of the beta.6 documentation, the following changes have been identified as impacting the Blazor wrappers.

## Breaking Changes to Address

### 1. WaIcon Component - Font Awesome 7 Upgrade (BREAKING)
**Impact**: Major breaking change due to Font Awesome 7 upgrade
**File**: `src/WebAwesome.Blazor/Components/WaIcon.cs`

**Changes Required**:
- **Remove**: `FixedWidth` property (line 49) - this is now default behavior in FA7
- **Add**: `AutoWidth` property to allow variable width icons
- **Add**: `SwapOpacity` property for duotone icons
- **Update**: HTML attribute from `"fixed-width"` to `"auto-width"` in BuildRenderTree method
- **Update**: Component documentation to reflect FA7 changes and new duotone features

**Diff Reference**: Lines 315-363 show the removal of fixed-width examples and addition of auto-width functionality

### 2. WaDetails Component - Attribute Rename (BREAKING)
**Impact**: Breaking change in property naming
**File**: `src/WebAwesome.Blazor/Components/WaDetails.cs`

**Changes Required**:
- **Rename**: `IconPosition` property to `IconPlacement` (line 47)
- **Update**: HTML attribute from `"icon-position"` to `"icon-placement"` (line 97)
- **Update**: Enum usage in ToHtmlValue() method

**Diff Reference**: Lines 159-188 show the attribute rename from `icon-position` to `icon-placement`

### 3. WaButtonGroup Component - Size Removal (BREAKING)
**Impact**: Breaking change removing inconsistent functionality
**File**: `src/WebAwesome.Blazor/Components/WaButtonGroup.cs`

**Changes Required**:
- **Remove**: `Size` property (line 37) as it's inconsistent with dynamic button updates
- **Remove**: Associated HTML attribute rendering (line 64)
- **Update**: Documentation to recommend applying size to individual buttons instead

**Diff Reference**: Lines 17-51 show the removal of Button Sizes example section

## New Components to Add

### 4. WaIntersectionObserver Component
**Impact**: New component introduction
**File**: `src/WebAwesome.Blazor/Components/WaIntersectionObserver.cs` (new file)

**Implementation Requirements**:
- **Properties**:
  - `Threshold` (string) - space-separated threshold values
  - `Root` (string) - ID of root element
  - `RootMargin` (string) - margin around root
  - `IntersectClass` (string) - class to toggle on intersection
- **Events**:
  - `OnIntersect` event with `WaIntersectionEventArgs` containing intersection details
- **Content**:
  - `ChildContent` for elements to observe
- **HTML Element**: `<wa-intersection-observer>`

**Diff Reference**: Lines 677-974 introduce the new intersection-observer component

### 5. Event Args Class for Intersection Observer
**File**: `src/WebAwesome.Blazor/Components/EventArgs.cs`

**Add**: `WaIntersectionEventArgs` class with properties for intersection observer entry details

## Component Enhancements

### 6. WaCard Component - New Orientation Support
**Impact**: Enhancement adding new functionality
**File**: `src/WebAwesome.Blazor/Components/WaCard.cs`

**Changes Required**:
- **Add**: `Orientation` property with `WaOrientation` enum support
- **Add**: Support for new slots: `header-actions` and `footer-actions`
- **Update**: Slot rendering logic for improved header/footer structure
- **Add**: Properties for `HeaderActionsContent` and `FooterActionsContent` RenderFragments

**Diff Reference**: Lines 109-139 show the new horizontal orientation and improved slot structure

### 7. WaDropdownItem Component - Variant Support
**Impact**: Enhancement for styling consistency
**File**: `src/WebAwesome.Blazor/Components/WaDropdownItem.cs`

**Changes Required**:
- **Add**: `Variant` property with `WaVariant` enum support for danger styling
- **Update**: HTML attribute rendering to include variant

**Diff Reference**: Lines 198-202 show the addition of variant="danger" support

## Enum Updates

### 8. Enums.cs Updates
**File**: `src/WebAwesome.Blazor/Components/Enums.cs`

**Changes Required**:
- Verify all existing enum values are present and correctly mapped
- No new enum values identified in the diff, but validate existing mappings

## Testing Updates

### 9. Update Existing Tests
**Files**: Various test files in `src/WebAwesome.Blazor.Tests/Components/`

**Changes Required**:
- **WaIcon tests**: Update for FixedWidth removal and AutoWidth/SwapOpacity addition
- **WaDetails tests**: Update for IconPosition to IconPlacement rename
- **WaButtonGroup tests**: Update for Size property removal
- **Add**: New test file for WaIntersectionObserver component

## Implementation Priority

### Phase 1: Breaking Changes (Critical)
1. Update WaIcon component (remove FixedWidth, add AutoWidth/SwapOpacity)
2. Update WaDetails component (rename IconPosition to IconPlacement)
3. Update WaButtonGroup component (remove Size property)

### Phase 2: New Features
4. Create WaIntersectionObserver component
5. Add intersection observer event args
6. Update WaCard component (add Orientation support)
7. Update WaDropdownItem component (add Variant support)

### Phase 3: Testing and Validation
8. Update all affected tests
9. Build and validate changes

## Validation Checklist
- [ ] All breaking changes implemented
- [ ] New WaIntersectionObserver component created and functional
- [ ] All existing tests updated and passing
- [ ] Build succeeds without warnings
- [ ] New functionality tested
- [ ] Documentation updated to reflect API changes

## Risk Assessment
- **High Risk**: WaIcon changes due to Font Awesome 7 upgrade affecting many consumers
- **Medium Risk**: WaDetails IconPosition rename may break existing code
- **Low Risk**: WaButtonGroup Size removal, new component additions, and enhancements

## Notes for Implementation
- Ensure proper XML documentation for all new properties and components
- Follow existing code patterns and region organization
- Use existing enum extension methods for HTML value conversion
- Test with both simple and complex scenarios for new intersection observer component
- Consider migration guide for consumers using breaking change properties