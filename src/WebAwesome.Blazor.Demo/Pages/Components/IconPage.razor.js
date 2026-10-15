// The icon library functions of the "Resolver & Mutator Functions" example on the icon page. Blazor serves this
// collocated file at ./Pages/Components/IconPage.razor.js, and IconLibraryOptions.ResolverFunction and MutatorFunction
// refer to its exports with WaJsFunction.Module. Each function gets wa-icon's arguments and a trailing ctx (ctx.args
// holds the WithArgs value, ctx.library the library name).

// the URL of a Bootstrap icon: the "fill" variant is a separate file with a "-fill" suffix, which a URL template with
// {name} and {variant} cannot express for icons without a variant
export function bootstrapIconUrl(name, family, variant, autoWidth, ctx) {
  const file = variant === 'fill' ? `${name}-fill` : name;
  return `https://cdn.jsdelivr.net/npm/bootstrap-icons@${ctx.args.version}/icons/${file}.svg`;
}

// colors the filled icons with the color from C#; wa-icon calls it with the svg and itself after each load and update
export function colorFilled(svg, icon, ctx) {
  if (icon.variant === 'fill') svg.setAttribute('fill', ctx.args.color);
}
