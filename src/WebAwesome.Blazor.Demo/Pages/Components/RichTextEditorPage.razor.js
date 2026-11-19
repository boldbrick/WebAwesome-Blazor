// The image provider of the "Image Provider" example on the rich text editor page. Blazor serves this collocated file
// at ./Pages/Components/RichTextEditorPage.razor.js, and ImageProviderFunction refers to its export with
// WaJsFunction.Module. The editor calls it with the pasted or dropped image file, or with no file for the image button,
// and a trailing ctx.

// the picture the image button inserts, standing in for a choice from the app's media library
const LIBRARY_IMAGE = { src: 'https://uploads.webawesome.com/kits.jpg', alt: 'Downloading kits' };

// the image button inserts the library picture; a pasted or dropped file is shown from memory (upload it to your
// server instead and return its permanent URL)
export async function mediaLibrary(file) {
  return file ? URL.createObjectURL(file) : LIBRARY_IMAGE;
}
