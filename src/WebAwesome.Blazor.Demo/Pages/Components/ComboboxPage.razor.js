// The filter function of the "Custom Filter with JavaScript" example on the combobox page. Blazor serves this collocated
// file at ./Pages/Components/ComboboxPage.razor.js, and FilterFunction refers to its export with WaJsFunction.Module.
// The function gets the option, the typed text and a trailing ctx.

// true when a word of the option's label starts with the typed text
export function wordStart(option, query) {
  const text = query.trim().toLowerCase();
  return option.label.toLowerCase().split(/\s+/).some(word => word.startsWith(text));
}
