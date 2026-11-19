using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Infrastructure;
using WebAwesome.Blazor.Models;

namespace WebAwesome.Blazor.Components;

/// <summary>
/// A rich text editor with a toolbar and keyboard shortcuts, bound to a <see cref="string"/> value (HTML, or Markdown
/// with <see cref="Format"/>). Corresponds to the wa-rich-text-editor Web Awesome component (Pro, experimental).
/// </summary>
/// <remarks>
/// <see cref="WaInputBase{TValue}.OnFocus"/> and <see cref="WaInputBase{TValue}.OnBlur"/> are bound to the element's own
/// focus and blur events, not to focusin and focusout: the editor dispatches them on the host only when the focus enters
/// or leaves the editor as a whole, so a move between the content and the toolbar raises neither. The keydown of the
/// editor is relayed, because the element stops the propagation of Escape inside its shadow root.
/// The element's <c>suggestions</c> (typeahead sources with a data source function) and the array forms of the colors
/// and the toolbar's custom swatch labels are JavaScript-only properties and are not wrapped.
/// </remarks>
public class WaRichTextEditor : WaLabeledInputBase<string?>
{
    #region ------ Form Control Properties ------

    /// <summary>
    /// Makes the content read-only. It can still be focused, selected and copied, but the toolbar is disabled.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>
    /// Marks the editor as required for form validation; a blank editor is invalid.
    /// </summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>
    /// Minimum number of characters of text, not counting markup, for a valid value.
    /// </summary>
    [Parameter] public int? MinLength { get; set; }

    /// <summary>
    /// Maximum number of characters of text, not counting markup, for a valid value. Typing beyond it is not blocked;
    /// the editor becomes invalid instead.
    /// </summary>
    [Parameter] public int? MaxLength { get; set; }

    #endregion

    #region ------ Visual & Behavior Properties ------

    /// <summary>
    /// The Web Awesome default of <see cref="Placeholder"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultPlaceholder = "";

    /// <summary>
    /// Placeholder text to show when the editor is empty.
    /// </summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Appearance"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaInputAppearance DefaultAppearance = WaInputAppearance.Outlined;

    /// <summary>
    /// The editor's visual appearance.
    /// </summary>
    [Parameter] public WaInputAppearance? Appearance { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Format"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaRichTextEditorFormat DefaultFormat = WaRichTextEditorFormat.Html;

    /// <summary>
    /// The format of the editor's value. With Markdown, the value, the initial content and the submitted form value are
    /// Markdown instead of HTML; what Markdown cannot express stays inline HTML.
    /// </summary>
    [Parameter] public WaRichTextEditorFormat? Format { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Toolbar"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultToolbar = "block | bold italic format-menu | link image table | lists | undo redo";

    /// <summary>
    /// The tools to show in the toolbar, as space-separated tokens (block, bold, italic, underline, strikethrough,
    /// highlight, code, subscript, superscript, color, format-menu, align, lists, bullet-list, ordered-list, indent,
    /// outdent, blockquote, code-block, link, image, embed, table, horizontal-rule, clear-formatting,
    /// paste-plain-text, undo, redo, actions). A <c>|</c> draws a divider between groups and <c>actions</c> positions
    /// the <see cref="ActionsContent"/> slot. An empty string removes the toolbar; unknown tokens are ignored.
    /// </summary>
    [Parameter] public string? Toolbar { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="ToolbarPlacement"/>: what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const WaRichTextEditorToolbarPlacement DefaultToolbarPlacement = WaRichTextEditorToolbarPlacement.Top;

    /// <summary>
    /// Where the toolbar sits.
    /// </summary>
    [Parameter] public WaRichTextEditorToolbarPlacement? ToolbarPlacement { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="IconLibrary"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultIconLibrary = "system";

    /// <summary>
    /// The icon library used for the toolbar and popover icons. Set it to "default" (or any registered library) to draw
    /// the icons from that library, e.g. Font Awesome Pro icons with a kit code.
    /// </summary>
    [Parameter] public string? IconLibrary { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="DisabledFeatures"/>: no feature is removed.
    /// </summary>
    public const WaRichTextEditorFeatures DefaultDisabledFeatures = WaRichTextEditorFeatures.None;

    /// <summary>
    /// The features to remove from the editor entirely: their tools, shortcuts and input rules are left out, and they
    /// are stripped from the value and from pasted content. <see cref="WaRichTextEditorFeatures.Lists"/> removes both
    /// list types.
    /// </summary>
    [Parameter] public WaRichTextEditorFeatures? DisabledFeatures { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="HeadingLevels"/> (levels 1 to 3): what the element holds while the
    /// parameter is null, and what is rendered in its place once the attribute has been rendered.
    /// </summary>
    public static readonly IReadOnlyList<WaRichTextEditorHeadingLevel> DefaultHeadingLevels =
        [WaRichTextEditorHeadingLevel.H1, WaRichTextEditorHeadingLevel.H2, WaRichTextEditorHeadingLevel.H3];

    /// <summary>
    /// The heading levels users can apply; other headings are converted to the nearest allowed level. Null or an empty
    /// list renders no attribute, which leaves the element's default of levels 1 to 3.
    /// </summary>
    [Parameter] public IReadOnlyList<WaRichTextEditorHeadingLevel>? HeadingLevels { get; set; }

    /// <summary>
    /// The text colors offered in the color menu, as CSS colors (rendered semicolon-separated, so a color holds no
    /// semicolon). Null or an empty list renders no attribute, which leaves the element's palette (it adapts to light and
    /// dark color schemes). Labeled swatches are a JavaScript-only form and are not wrapped.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? TextColors { get; set; }

    /// <summary>
    /// The highlight colors offered in the color menu, in the same form as <see cref="TextColors"/>. Null or an empty
    /// list renders no attribute, which leaves the element's translucent tints.
    /// </summary>
    [Parameter] public IReadOnlyList<string>? HighlightColors { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="LinkTarget"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLinkTarget = "";

    /// <summary>
    /// The target given to links created in the editor. "_blank" also sets rel to "noopener noreferrer".
    /// </summary>
    [Parameter] public string? LinkTarget { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="LinkRel"/>: what the element holds while the parameter is null, and what
    /// is rendered in its place once the attribute has been rendered.
    /// </summary>
    public const string DefaultLinkRel = "";

    /// <summary>
    /// The rel given to links created in the editor. Overrides the default rel that "_blank" adds.
    /// </summary>
    [Parameter] public string? LinkRel { get; set; }

    /// <summary>
    /// The Web Awesome default of <see cref="Spellcheck"/> (on): what the element holds while the parameter is null, and
    /// what is rendered in its place once the attribute has been rendered (a removed attribute would read as off).
    /// </summary>
    public const bool DefaultSpellcheck = true;

    /// <summary>
    /// Enables spell checking on the editor.
    /// </summary>
    [Parameter] public bool? Spellcheck { get; set; }

    /// <summary>
    /// Controls whether and how text input is automatically capitalized as it is entered by the user.
    /// </summary>
    [Parameter] public WaAutoCapitalize? AutoCapitalize { get; set; }

    /// <summary>
    /// Turns the browser's autocorrect feature on (true, rendered "on") or off (false, rendered "off"); null leaves
    /// the element's default.
    /// </summary>
    [Parameter] public bool? AutoCorrect { get; set; }

    /// <summary>
    /// Used to customize the label or icon of the Enter key on virtual keyboards.
    /// </summary>
    [Parameter] public WaEnterKeyHint? EnterKeyHint { get; set; }

    /// <summary>
    /// Tells the browser what type of data will be entered by the user, allowing it to display the appropriate
    /// virtual keyboard on supportive devices.
    /// </summary>
    [Parameter] public WaInputMode? InputMode { get; set; }

    /// <summary>
    /// Shows a character count below the editor. When <see cref="MaxLength"/> is set, shows the remaining characters
    /// instead.
    /// </summary>
    [Parameter] public bool WithCount { get; set; }

    /// <summary>
    /// Turns off smart quotes and the replacement of "..." and "--" with an ellipsis and an em dash as users type.
    /// </summary>
    [Parameter] public bool WithoutSmartPunctuation { get; set; }

    /// <summary>
    /// Turns off the Markdown-style shortcuts that format content as users type, such as "# " for a heading. Keyboard
    /// shortcuts still work.
    /// </summary>
    [Parameter] public bool WithoutMarkdownShortcuts { get; set; }

    /// <summary>
    /// Turns off linking URLs as they are typed.
    /// </summary>
    [Parameter] public bool WithoutAutolink { get; set; }

    /// <summary>
    /// The JavaScript function that provides images (the element's <c>imageProvider</c>). When set, the image tool calls
    /// it instead of asking for a URL, and pasted or dropped image files are passed to it, e.g. to upload them. It is
    /// called as <c>(file, ctx)</c> (file is undefined for the tool) with <c>ctx.args</c> and <c>ctx.element</c>, and
    /// returns a Promise of a URL, of <c>{ src, alt }</c>, or of null to cancel. When null, the element embeds pasted
    /// and dropped image files as data URLs.
    /// </summary>
    [Parameter] public WaJsFunction? ImageProviderFunction { get; set; }

    /// <summary>
    /// The JavaScript function that resolves what users enter in the embed editor (the element's
    /// <c>embedProvider</c>). It is called as <c>(input, ctx)</c> with the entered URL or iframe code, with
    /// <c>ctx.args</c> and <c>ctx.element</c>, and returns a Promise of a URL, of
    /// <c>{ src, title, width, height }</c>, of null to reject the input, or of undefined to use the built-in resolver.
    /// When null, the built-in resolver applies.
    /// </summary>
    [Parameter] public WaJsFunction? EmbedProviderFunction { get; set; }

    #endregion

    #region ------ Slots ------

    /// <summary>
    /// Initial HTML (or Markdown, per <see cref="Format"/>) content, like the text inside a textarea. The element reads it
    /// once when it connects and it becomes the default value; it is ignored when a value is bound.
    /// </summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Custom buttons placed in the toolbar, after the built-in tools or where the <c>actions</c> token appears in
    /// <see cref="Toolbar"/>.
    /// </summary>
    [Parameter] public RenderFragment? ActionsContent { get; set; }

    #endregion

    #region ------ Events ------

    /// <summary>
    /// Emitted when the form control has been checked for validity and its constraints aren't satisfied.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnInvalid { get; set; }

    /// <summary>
    /// Emitted when the selection or the formatting at the selection changes. Use it to keep custom toolbar buttons in
    /// sync with <see cref="IsActiveAsync"/>.
    /// </summary>
    [Parameter] public EventCallback<EventArgs> OnSelectionChange { get; set; }

    /// <summary>
    /// Emitted when the data source of a suggestion source rejects. The suggestion sources are a JavaScript-only
    /// property of the element, so it fires only for sources an app sets on the element itself.
    /// </summary>
    [Parameter] public EventCallback<WaSuggestionsErrorEventArgs> OnSuggestionsError { get; set; }

    #endregion

    #region ------ Overrides ------

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        var attributes = builder.OpenWaElement(this, 0, "wa-rich-text-editor");

        // Add common attributes
        AddCommonAttributes(builder, 1);

        // Add the form control attributes the element declares
        builder.AddAttribute(7, "readonly", Readonly);
        builder.AddAttribute(8, "required", Required);
        builder.AddAttributeIfNotNull(9, "minlength", MinLength);
        builder.AddAttributeIfNotNull(10, "maxlength", MaxLength);
        AddLabelAndHintAttributes(builder, 12);
        AddWithHintAndLabelAttributes(builder, 14);

        // Add editor-specific attributes
        builder.AddAttributeIfNotNullOrEmpty(attributes, 20, "placeholder", Placeholder, DefaultPlaceholder);
        builder.AddAttributeIfNotNull(attributes, 21, "appearance", Appearance?.ToHtmlValue(), DefaultAppearance.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 22, "format", Format?.ToHtmlValue(), DefaultFormat.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 23, "toolbar", Toolbar, DefaultToolbar);
        builder.AddAttributeIfNotNull(attributes, 24, "toolbar-placement", ToolbarPlacement?.ToHtmlValue(), DefaultToolbarPlacement.ToHtmlValue());
        builder.AddAttributeIfNotNullOrEmpty(attributes, 25, "icon-library", IconLibrary, DefaultIconLibrary);
        builder.AddAttributeIfNotNull(attributes, 26, "disabled-features", DisabledFeatures?.ToHtmlValue(), DefaultDisabledFeatures.ToHtmlValue());
        builder.AddAttributeIfNotNull(attributes, 27, "heading-levels", FormatHeadingLevels(HeadingLevels), FormatHeadingLevels(DefaultHeadingLevels)!);
        builder.AddAttributeIfNotNull(28, "text-colors", FormatColors(TextColors));
        builder.AddAttributeIfNotNull(29, "highlight-colors", FormatColors(HighlightColors));
        builder.AddAttributeIfNotNullOrEmpty(attributes, 30, "link-target", LinkTarget, DefaultLinkTarget);
        builder.AddAttributeIfNotNullOrEmpty(attributes, 31, "link-rel", LinkRel, DefaultLinkRel);
        builder.AddTrueFalseAttribute(attributes, 32, "spellcheck", Spellcheck, DefaultSpellcheck);
        builder.AddAttributeIfNotNull(33, "autocapitalize", AutoCapitalize?.ToHtmlValue());
        builder.AddOnOffAttribute(34, "autocorrect", AutoCorrect);
        builder.AddAttributeIfNotNull(35, "enterkeyhint", EnterKeyHint?.ToHtmlValue());
        builder.AddAttributeIfNotNull(36, "inputmode", InputMode?.ToHtmlValue());
        builder.AddAttribute(37, "with-count", WithCount);
        builder.AddAttribute(38, "without-smart-punctuation", WithoutSmartPunctuation);
        builder.AddAttribute(39, "without-markdown-shortcuts", WithoutMarkdownShortcuts);
        builder.AddAttribute(40, "without-autolink", WithoutAutolink);

        // Add value binding
        builder.AddAttribute(45, "value", CurrentValueAsString);
        builder.AddAttribute(46, "onchange", EventCallback.Factory.CreateBinder<string?>(this, SetCurrentValueAsStringFromElement, CurrentValueAsString));
        builder.SetUpdatesAttributeName("value");

        // the editor relays its own focus and blur to the host (non-bubbling, so only the host's own handler gets them)
        builder.AddAttributeIfHasDelegate(50, "onfocus", OnFocus);
        builder.AddAttributeIfHasDelegate(51, "onblur", OnBlur);
        AddRelayedKeyDownHandler(builder, 52);
        builder.AddAttributeIfHasDelegate(54, "onkeyup", OnKeyUp);
        builder.AddAttributeIfHasDelegate(55, "onkeypress", OnKeyPress);
        builder.AddAttributeIfHasDelegate(56, "oninput", OnInput);

        // Add editor-specific event handlers
        builder.AddAttributeIfHasDelegate(57, "onwa-invalid", OnInvalid);
        builder.AddRelayedEventIfHasDelegate(58, Constants.RelayedSelectionChangeEventAttribute, OnSelectionChange);
        builder.AddAttributeIfHasDelegate(59, "onwa-suggestions-error", OnSuggestionsError);

        // Add element reference capture
        builder.AddElementReferenceCapture(60, __editorReference => Element = __editorReference);

        // Add label and hint slots
        AddLabelAndHintSlots(builder, 70);

        // Add the actions slot and the initial content
        if (ActionsContent is not null)
        {
            builder.OpenElement(76, "span");
            builder.AddAttribute(77, "slot", "actions");
            builder.AddContent(78, ActionsContent);
            builder.CloseElement();
        }

        builder.AddContent(80, ChildContent);

        builder.CloseElement();
    }

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out string? result, [NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = value;
        validationErrorMessage = null;
        return true;
    }

    /// <inheritdoc />
    protected override string? LiveValuePropertyName => "value";

    /// <inheritdoc />
    internal override bool RelaysKeyDown => true;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);

        await jsProperties.PushFunctionAsync(JSInterop, Element, ImageProviderMember, ImageProviderFunction);
        await jsProperties.PushFunctionAsync(JSInterop, Element, EmbedProviderMember, EmbedProviderFunction);
    }

    #endregion

    #region ------ Public Methods ------

    /// <summary>
    /// Removes focus from the editor.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task BlurAsync()
        => await JSInterop.InvokeMethodAsync(RenderedElement("blur"), "blur");

    /// <summary>
    /// Sets focus on the editor's content.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task FocusAsync()
        => await JSInterop.InvokeMethodAsync(RenderedElement("focus"), "focus");

    /// <summary>
    /// Deletes the selected content.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task DeleteSelectionAsync()
        => await JSInterop.InvokeMethodAsync(RenderedElement("delete the selection"), "deleteSelection");

    /// <summary>
    /// Selects all of the content. Does nothing while the editor is disabled.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task SelectAllAsync()
        => await JSInterop.InvokeMethodAsync(RenderedElement("select all"), "selectAll");

    /// <summary>
    /// Inserts HTML at the selection, replacing it. Markup the editor doesn't support is stripped.
    /// </summary>
    /// <param name="html">The HTML to insert</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task InsertHtmlAsync(string html)
        => await JSInterop.InvokeMethodAsync(RenderedElement("insert HTML"), "insertHtml", html);

    /// <summary>
    /// Inserts plain text at the selection, replacing it.
    /// </summary>
    /// <param name="text">The text to insert</param>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task InsertTextAsync(string text)
        => await JSInterop.InvokeMethodAsync(RenderedElement("insert text"), "insertText", text);

    /// <summary>
    /// Undoes the last edit.
    /// </summary>
    /// <returns>Whether there was an edit to undo</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<bool> UndoAsync()
        => await JSInterop.InvokeMethodAsync<bool>(RenderedElement("undo"), "undo");

    /// <summary>
    /// Redoes the last undone edit.
    /// </summary>
    /// <returns>Whether there was an edit to redo</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<bool> RedoAsync()
        => await JSInterop.InvokeMethodAsync<bool>(RenderedElement("redo"), "redo");

    /// <summary>
    /// Gets the selection as ProseMirror document positions.
    /// </summary>
    /// <returns>The selection range</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<WaRichTextEditorSelection> GetSelectionAsync()
        => await JSInterop.InvokeMethodAsync<WaRichTextEditorSelection>(RenderedElement("get the selection"), "getSelection");

    /// <summary>
    /// Whether a format or context is active at the selection.
    /// </summary>
    /// <param name="state">The format or context to test</param>
    /// <returns>True when it is active</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<bool> IsActiveAsync(WaRichTextEditorActiveState state)
        => await JSInterop.InvokeMethodAsync<bool>(RenderedElement("test the active state"), "isActive", state.ToHtmlValue());

    /// <summary>
    /// Runs a command, such as bold, a heading with its level, a link with its address or a table command.
    /// </summary>
    /// <param name="command">The command to run</param>
    /// <param name="options">The options the command reads; null for a command without any</param>
    /// <returns>Whether the command applied; unknown commands, commands that cannot apply at the selection and commands
    /// of disabled features return false</returns>
    /// <exception cref="InvalidOperationException">Thrown when the component has not been rendered yet</exception>
    public async Task<bool> RunCommandAsync(WaRichTextEditorCommand command, WaRichTextEditorCommandOptions? options = null)
        => await JSInterop.InvokeMethodAsync<bool>(RenderedElement("run a command"), "runCommand", command.ToHtmlValue(), options?.ToPayload() ?? new Dictionary<string, object?>());

    #endregion

    #region ------ Internals ------

    private const string ImageProviderMember = "imageProvider";
    private const string EmbedProviderMember = "embedProvider";

    private readonly WaJsPropertyPush jsProperties = new();

    // the element reference, or an exception naming what could not be done before the first render
    private ElementReference RenderedElement(string action)
    {
        if (Element is null)
            throw new InvalidOperationException($"Cannot {action}: component has not been rendered yet.");

        return Element.Value;
    }

    // the heading levels as the element splits them (whitespace); null or an empty list renders no attribute
    private static string? FormatHeadingLevels(IReadOnlyList<WaRichTextEditorHeadingLevel>? levels)
        => levels is null or { Count: 0 } ? null : WaWireFormat.FormatTokens(levels.Select(level => level.ToHtmlValue()), WaWireFormat.SpaceSeparator, WaWireFormat.WhitespaceSeparators);

    // the colors as the element splits them (semicolon); null or an empty list renders no attribute
    private static string? FormatColors(IReadOnlyList<string>? colors)
        => colors is null or { Count: 0 } ? null : WaWireFormat.FormatTokens(colors, WaWireFormat.SemicolonSeparator, WaWireFormat.SemicolonSeparators);

    #endregion
}

/// <summary>
/// The selection of a <see cref="WaRichTextEditor"/> as ProseMirror document positions, as returned by
/// <see cref="WaRichTextEditor.GetSelectionAsync"/>.
/// </summary>
/// <param name="From">The start position</param>
/// <param name="To">The end position</param>
/// <param name="Empty">Whether the selection is a caret</param>
public record WaRichTextEditorSelection(int From, int To, bool Empty);

/// <summary>
/// The options of <see cref="WaRichTextEditor.RunCommandAsync"/>. Each command reads only the options it needs; an unset
/// (null) option is left out, so the editor applies its own default.
/// </summary>
public class WaRichTextEditorCommandOptions
{
    /// <summary>
    /// The heading level for <see cref="WaRichTextEditorCommand.Heading"/>.
    /// </summary>
    public WaRichTextEditorHeadingLevel? Level { get; set; }

    /// <summary>
    /// The text alignment for <see cref="WaRichTextEditorCommand.Align"/>.
    /// </summary>
    public WaRichTextEditorAlignment? Align { get; set; }

    /// <summary>
    /// The image alignment for <see cref="WaRichTextEditorCommand.Image"/>.
    /// </summary>
    public WaRichTextEditorImageAlignment? ImageAlign { get; set; }

    /// <summary>
    /// Clears the alignment of the selected image for <see cref="WaRichTextEditorCommand.Image"/>; wins over
    /// <see cref="ImageAlign"/>.
    /// </summary>
    public bool ClearImageAlign { get; set; }

    /// <summary>
    /// The address for <see cref="WaRichTextEditorCommand.Link"/>.
    /// </summary>
    public string? Href { get; set; }

    /// <summary>
    /// The link target for <see cref="WaRichTextEditorCommand.Link"/>; the editor's link target applies when null.
    /// </summary>
    public string? Target { get; set; }

    /// <summary>
    /// The link rel for <see cref="WaRichTextEditorCommand.Link"/>; the editor's link rel applies when null.
    /// </summary>
    public string? Rel { get; set; }

    /// <summary>
    /// The address for <see cref="WaRichTextEditorCommand.Image"/> and <see cref="WaRichTextEditorCommand.Embed"/>.
    /// </summary>
    public string? Src { get; set; }

    /// <summary>
    /// The alternative text for <see cref="WaRichTextEditorCommand.Image"/>.
    /// </summary>
    public string? Alt { get; set; }

    /// <summary>
    /// The title for <see cref="WaRichTextEditorCommand.Image"/> and <see cref="WaRichTextEditorCommand.Embed"/>.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Whether the image of <see cref="WaRichTextEditorCommand.Image"/> has a caption (a figure); the image's current
    /// state is kept when null.
    /// </summary>
    public bool? WithCaption { get; set; }

    /// <summary>
    /// The width of an embed in pixels for <see cref="WaRichTextEditorCommand.Embed"/>.
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// The height of an embed in pixels for <see cref="WaRichTextEditorCommand.Embed"/>.
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// The number of rows for <see cref="WaRichTextEditorCommand.Table"/> (3 when null).
    /// </summary>
    public int? Rows { get; set; }

    /// <summary>
    /// The number of columns for <see cref="WaRichTextEditorCommand.Table"/> (3 when null).
    /// </summary>
    public int? Columns { get; set; }

    /// <summary>
    /// The text color (any CSS color) for <see cref="WaRichTextEditorCommand.Color"/>; the color is kept when null.
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Removes the text color for <see cref="WaRichTextEditorCommand.Color"/>; wins over <see cref="Color"/>.
    /// </summary>
    public bool RemoveColor { get; set; }

    /// <summary>
    /// The highlight color (any CSS color) for <see cref="WaRichTextEditorCommand.Color"/>; the highlight is kept when
    /// null.
    /// </summary>
    public string? Highlight { get; set; }

    /// <summary>
    /// Applies the default highlight (a bare mark) for <see cref="WaRichTextEditorCommand.Color"/>; wins over
    /// <see cref="Highlight"/>.
    /// </summary>
    public bool DefaultHighlight { get; set; }

    /// <summary>
    /// Removes the highlight for <see cref="WaRichTextEditorCommand.Color"/>; wins over <see cref="DefaultHighlight"/>.
    /// </summary>
    public bool RemoveHighlight { get; set; }

    #region ------ Internals ------

    // the options object the element reads; an option that is not set is left out
    internal Dictionary<string, object?> ToPayload()
    {
        var payload = new Dictionary<string, object?>();
        if (Level is { } level) payload["level"] = (int)level;
        if (Align is { } align) payload["align"] = align.ToHtmlValue();
        if (ClearImageAlign) payload["align"] = null;
        else if (ImageAlign is { } imageAlign) payload["align"] = imageAlign.ToHtmlValue();
        if (Href is not null) payload["href"] = Href;
        if (Target is not null) payload["target"] = Target;
        if (Rel is not null) payload["rel"] = Rel;
        if (Src is not null) payload["src"] = Src;
        if (Alt is not null) payload["alt"] = Alt;
        if (Title is not null) payload["title"] = Title;
        if (WithCaption is { } caption) payload["caption"] = caption;
        if (Width is { } width) payload["width"] = width;
        if (Height is { } height) payload["height"] = height;
        if (Rows is { } rows) payload["rows"] = rows;
        if (Columns is { } columns) payload["columns"] = columns;
        if (RemoveColor) payload["color"] = null;
        else if (Color is not null) payload["color"] = Color;
        if (RemoveHighlight) payload["highlight"] = null;
        else if (DefaultHighlight) payload["highlight"] = true;
        else if (Highlight is not null) payload["highlight"] = Highlight;
        return payload;
    }

    #endregion
}
