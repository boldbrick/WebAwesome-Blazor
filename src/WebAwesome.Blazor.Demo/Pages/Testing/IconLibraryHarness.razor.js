// Functions of the icon library harness (/testing/icon-library). Blazor serves this collocated file at
// ./Pages/Testing/IconLibraryHarness.razor.js, and IconLibraryOptions.ResolverFunction and MutatorFunction refer to its
// exports with WaJsFunction.Module. wa-icon calls a resolver with (name, family, variant, autoWidth) and a mutator with
// (svg, icon); ctx follows them (ctx.args holds the WithArgs value, ctx.library the library name). The icons are data
// URLs, so the harness needs no network, and each SVG shows what the resolver received, so the spec can read it.

// a data URL of a circle in the color of ctx.args, with the resolver's arguments as data attributes
export function dataUrl(name, family, variant, autoWidth, ctx) {
  const svg = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 16 16" fill="${ctx.args.color}"`
    + ` data-name="${name}" data-family="${family}" data-variant="${variant}" data-auto-width="${autoWidth}"`
    + ` data-library="${ctx.library}" data-element="${typeof ctx.element}"><circle cx="8" cy="8" r="7"/></svg>`;
  return `data:image/svg+xml,${encodeURIComponent(svg)}`;
}

// a sprite reference for a library with spriteSheet: wa-icon renders it in a <use> element
export function spriteReference(name, family, variant, autoWidth, ctx) {
  return `${ctx.args.sprite}#${name}`;
}

// a mutator that marks the SVG with the mark of ctx.args, the host element and the library name
export function markSvg(svg, icon, ctx) {
  svg.setAttribute('data-mutated', `${ctx.args.mark}|${icon.localName}|${ctx.library}`);
}
