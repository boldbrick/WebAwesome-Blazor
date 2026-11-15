// The tag function of the "Custom Tags with JavaScript" example on the select page. Blazor serves this collocated file
// at ./Pages/Components/SelectPage.razor.js, and GetTagFunction refers to its export with WaJsFunction.Module. The
// function gets the option, the tag index and a trailing ctx (ctx.args holds the WithArgs value, ctx.element the select).

// a tag with the topping's emoji (ctx.args.emoji) before its label; ShadowStyles styles the emoji's class
export function toppingTag(option, index, ctx) {
  const emoji = document.createElement('span');
  emoji.className = 'topping-emoji';
  emoji.textContent = ctx.args.emoji[option.value] ?? '';

  const tag = document.createElement('wa-tag');
  tag.size = ctx.element.size;
  tag.append(emoji, option.label);
  return tag;
}
