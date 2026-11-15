using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using WebAwesome.Blazor.Base;
using WebAwesome.Blazor.Infrastructure;

namespace WebAwesome.Blazor.Models;

/// <summary>
/// An immutable reference to a JavaScript function, for a Web Awesome member that takes a function (for example the
/// <see cref="WaDataGridColumn.Formatter"/> of a data grid column). A function cannot cross the JS interop boundary, so
/// the reference travels as a JSON descriptor, and the browser resolves it into the function: an app function
/// registered by name (<see cref="Registered"/>), an export of an ES module (<see cref="Module"/>), or a function on a
/// global path (<see cref="Global"/>).
/// </summary>
/// <remarks>
/// <para>
/// The element calls the resolved function with its own arguments in their order, plus one trailing context object
/// (<c>ctx</c>): <c>ctx.args</c> holds the <see cref="WithArgs"/> value, <c>ctx.element</c> the element the member
/// belongs to, and a member can add more (the data grid adds <c>ctx.column</c>, <c>ctx.grid</c> and
/// <c>ctx.rowKey</c>). Read the members of <c>ctx</c>; do not keep or spread it.
/// </para>
/// <para>
/// No JavaScript source text crosses the boundary, and the library never evaluates text as code. The function is code
/// of the app: when it writes HTML (for example with <c>innerHTML</c>), the app must escape the data.
/// </para>
/// <para>
/// When a reference does not resolve (an unknown name, a module that does not load, a missing export), the browser
/// writes one <c>console.error</c> that names the reference, and the member is left out, so the element applies its
/// own default.
/// </para>
/// </remarks>
[JsonConverter(typeof(WaJsFunctionJsonConverter))]
public sealed record WaJsFunction
{
    /// <summary>
    /// The export that <see cref="Module"/> uses when no export name is given: the default export of the module.
    /// </summary>
    public const string DefaultExportName = "default";

    /// <summary>
    /// How the browser finds the function.
    /// </summary>
    public WaJsFunctionKind Kind { get; }

    /// <summary>
    /// The registered name (<see cref="WaJsFunctionKind.Registered"/>), the module URL
    /// (<see cref="WaJsFunctionKind.Module"/>) or the dotted global path (<see cref="WaJsFunctionKind.Global"/>).
    /// </summary>
    public string Reference { get; }

    /// <summary>
    /// The name of the module export (<see cref="WaJsFunctionKind.Module"/> only); null for the other kinds.
    /// </summary>
    public string? ExportName { get; }

    /// <summary>
    /// Whether the module may come from another origin than the page (<see cref="WaJsFunctionKind.Module"/> only).
    /// </summary>
    public bool AllowCrossOrigin { get; }

    /// <summary>
    /// The JSON value the function receives as <c>ctx.args</c>, or null when <see cref="WithArgs"/> was not called.
    /// </summary>
    public JsonElement? Args
    {
        get
        {
            if (argsJson is null) return null;

            using var document = JsonDocument.Parse(argsJson);
            return document.RootElement.Clone();
        }
    }

    /// <summary>
    /// The Web Awesome tags the function renders (see <see cref="Uses"/>), in the order given, without duplicates.
    /// </summary>
    public IReadOnlyList<string> Tags { get; private init; }

    /// <summary>
    /// Refers to a function that the app registered by name in the browser, with <c>registerFunction(name, fn)</c> of
    /// the module <c>_content/WebAwesome.Blazor/webawesome-functions.js</c> (for example in the app's own JS
    /// initializer). When the name is not registered yet, the browser waits a short time for it.
    /// </summary>
    /// <param name="name">The registered name, for example <c>myapp.orderState</c></param>
    /// <returns>The function reference</returns>
    /// <exception cref="ArgumentException">Thrown when the name is empty, has leading or trailing whitespace, or
    /// contains a control character</exception>
    public static WaJsFunction Registered(string name)
    {
        ValidateName(name, nameof(name));
        return new WaJsFunction(WaJsFunctionKind.Registered, name, null, false);
    }

    /// <summary>
    /// Refers to an export of an ES module. A collocated <c>Page.razor.js</c> file next to <c>Page.razor</c> is the usual
    /// module: Blazor serves it at <c>./Pages/Page.razor.js</c>, or at <c>./_content/&lt;Library&gt;/Pages/Page.razor.js</c>
    /// in a Razor class library. The browser imports each module once.
    /// </summary>
    /// <param name="url">The module URL: relative to the document base, or absolute with the http or https scheme</param>
    /// <param name="exportName">The name of the export; the default export when omitted</param>
    /// <param name="allowCrossOrigin">Allows a module of another origin than the page. When false, the browser refuses
    /// such a module, so that a URL cannot load code from a foreign server by mistake.</param>
    /// <returns>The function reference</returns>
    /// <exception cref="ArgumentException">Thrown when the URL is empty, contains whitespace or a control character, or
    /// has a scheme other than http or https, or when the export name is empty</exception>
    public static WaJsFunction Module(string url, string exportName = DefaultExportName, bool allowCrossOrigin = false)
    {
        ValidateModuleUrl(url, nameof(url));
        ValidateName(exportName, nameof(exportName));
        return new WaJsFunction(WaJsFunctionKind.Module, url, exportName, allowCrossOrigin);
    }

    /// <summary>
    /// Refers to a function on a global dotted path, for example <c>myApp.grid.stateCell</c> for
    /// <c>window.myApp.grid.stateCell</c>. The function is called with its owner object as <c>this</c>.
    /// </summary>
    /// <param name="path">The dotted path from the global object</param>
    /// <returns>The function reference</returns>
    /// <exception cref="ArgumentException">Thrown when the path is not a dot-separated list of JavaScript
    /// identifiers</exception>
    public static WaJsFunction Global(string path)
    {
        if (string.IsNullOrEmpty(path) || !path.Split(PathSeparator).All(segment => IdentifierPattern.IsMatch(segment)))
            throw new ArgumentException($"'{path}' is not a dotted path of JavaScript identifiers.", nameof(path));

        return new WaJsFunction(WaJsFunctionKind.Global, path, null, false);
    }

    /// <summary>
    /// Returns a copy that gives the function a JSON-serializable value as <c>ctx.args</c>, for example options or a
    /// lookup table. The value is serialized at once (camelCase property names, as the JS interop does), so a later
    /// change of the object does not change the reference.
    /// </summary>
    /// <param name="args">The value, for example an anonymous object</param>
    /// <returns>A copy with the arguments; they replace earlier ones</returns>
    /// <exception cref="ArgumentNullException">Thrown when args is null</exception>
    /// <exception cref="ArgumentException">Thrown when args cannot be serialized to JSON</exception>
    public WaJsFunction WithArgs(object args)
    {
        if (args == null) throw new ArgumentNullException(nameof(args));

        string json;
        try
        {
            json = JsonSerializer.Serialize(args, args.GetType(), JsonSerializerOptions.Web);
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException or InvalidOperationException)
        {
            throw new ArgumentException($"The arguments of a JavaScript function must serialize to JSON: {ex.Message}", nameof(args), ex);
        }

        return new WaJsFunction(this) { argsJson = json };
    }

    /// <summary>
    /// Returns a copy that declares the Web Awesome elements the function renders, for example <c>wa-badge</c>. The
    /// browser registers them before the element first renders the function's content: content in a shadow root is
    /// not visible to the Web Awesome autoloader, so an element that only the function renders can otherwise stay
    /// undefined.
    /// </summary>
    /// <param name="tags">The tag names; they are added to the tags declared before</param>
    /// <returns>A copy with the tags</returns>
    /// <exception cref="ArgumentNullException">Thrown when tags is null</exception>
    /// <exception cref="ArgumentException">Thrown when a tag is not a lowercase <c>wa-</c> tag name</exception>
    public WaJsFunction Uses(params string[] tags)
    {
        if (tags == null) throw new ArgumentNullException(nameof(tags));

        foreach (var tag in tags)
        {
            if (tag == null || !TagPattern.IsMatch(tag))
                throw new ArgumentException($"'{tag}' is not a Web Awesome tag name such as 'wa-badge'.", nameof(tags));
        }

        return new WaJsFunction(this) { Tags = Tags.Concat(tags).Distinct(StringComparer.Ordinal).ToArray() };
    }

    /// <summary>
    /// Whether the other reference has the same kind, target, arguments and tags.
    /// </summary>
    /// <param name="other">The other reference</param>
    /// <returns>True when both references serialize to the same descriptor</returns>
    public bool Equals(WaJsFunction? other)
    {
        return other is not null
            && Kind == other.Kind
            && string.Equals(Reference, other.Reference, StringComparison.Ordinal)
            && string.Equals(ExportName, other.ExportName, StringComparison.Ordinal)
            && AllowCrossOrigin == other.AllowCrossOrigin
            && string.Equals(argsJson, other.argsJson, StringComparison.Ordinal)
            && Tags.SequenceEqual(other.Tags, StringComparer.Ordinal);
    }

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Kind, Reference, ExportName, AllowCrossOrigin, argsJson, Tags.Count);

    /// <summary>
    /// Returns the JSON descriptor that the browser receives.
    /// </summary>
    /// <returns>The descriptor JSON</returns>
    public override string ToString() => JsonSerializer.Serialize(this, JsonSerializerOptions.Web);

    #region ------ Constructors ------

    private WaJsFunction(WaJsFunctionKind kind, string reference, string? exportName, bool allowCrossOrigin)
    {
        Kind = kind;
        Reference = reference;
        ExportName = exportName;
        AllowCrossOrigin = allowCrossOrigin;
        Tags = Array.Empty<string>();
    }

    private WaJsFunction(WaJsFunction original)
    {
        Kind = original.Kind;
        Reference = original.Reference;
        ExportName = original.ExportName;
        AllowCrossOrigin = original.AllowCrossOrigin;
        Tags = original.Tags;
        argsJson = original.argsJson;
    }

    #endregion

    #region ------ Internals ------

    /// <summary>
    /// Writes the descriptor of design 5.1 (WAB-49): the marker, the reference, and the arguments and tags when set.
    /// The resolver in webawesome-functions.js reads exactly these keys.
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    internal void WriteDescriptor(Utf8JsonWriter writer)
    {
        writer.WriteStartObject();
        writer.WriteString(MarkerKey, FunctionMarker);

        writer.WritePropertyName(ReferenceKey);
        writer.WriteStartObject();
        switch (Kind)
        {
            case WaJsFunctionKind.Registered:
                writer.WriteString(KindKey, RegisteredKind);
                writer.WriteString(NameKey, Reference);
                break;
            case WaJsFunctionKind.Module:
                writer.WriteString(KindKey, ModuleKind);
                writer.WriteString(UrlKey, Reference);
                writer.WriteString(ExportKey, ExportName);
                if (AllowCrossOrigin) writer.WriteBoolean(AllowCrossOriginKey, true);
                break;
            default:
                writer.WriteString(KindKey, GlobalKind);
                writer.WriteString(PathKey, Reference);
                break;
        }
        writer.WriteEndObject();

        if (argsJson is not null)
        {
            writer.WritePropertyName(ArgsKey);
            writer.WriteRawValue(argsJson, skipInputValidation: true);
        }

        if (Tags.Count > 0)
        {
            writer.WritePropertyName(UsesKey);
            writer.WriteStartArray();
            foreach (var tag in Tags)
                writer.WriteStringValue(tag);
            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }

    private static void ValidateName(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length != value.Length || value.Any(char.IsControl))
            throw new ArgumentException($"'{value}' is not a valid name: it must be non-empty, without leading or trailing whitespace and without control characters.", parameterName);
    }

    private static void ValidateModuleUrl(string url, string parameterName)
    {
        // the browser drops tabs and line breaks inside a URL and trims spaces, so "java\tscript:" would become a scheme
        if (string.IsNullOrEmpty(url) || url.Any(c => char.IsWhiteSpace(c) || char.IsControl(c)))
            throw new ArgumentException($"'{url}' is not a valid module URL: it must be non-empty, without whitespace and without control characters.", parameterName);

        // the origin is known only in the browser, which refuses another origin unless allowCrossOrigin is set
        var scheme = SchemePattern.Match(url);
        if (scheme.Success && !AllowedSchemes.Contains(scheme.Groups[1].Value, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException($"'{url}' is not a valid module URL: only a relative URL or the http and https schemes are allowed.", parameterName);
    }

    // the JSON keys and values of the descriptor (design 5.1), read by webawesome-functions.js
    private const string MarkerKey = "$wab";
    private const string FunctionMarker = "fn";
    private const string ReferenceKey = "ref";
    private const string KindKey = "kind";
    private const string RegisteredKind = "registered";
    private const string ModuleKind = "module";
    private const string GlobalKind = "global";
    private const string NameKey = "name";
    private const string UrlKey = "url";
    private const string ExportKey = "export";
    private const string AllowCrossOriginKey = "allowCrossOrigin";
    private const string PathKey = "path";
    private const string ArgsKey = "args";
    private const string UsesKey = "uses";

    private const char PathSeparator = '.';

    private static readonly string[] AllowedSchemes = ["http", "https"];

    // a URL scheme as the URL standard reads it: a letter, then letters, digits, "+", "-" or ".", then ":"
    private static readonly Regex SchemePattern = new("^([A-Za-z][A-Za-z0-9+.-]*):", RegexOptions.CultureInvariant);

    // a JavaScript identifier (ECMAScript IdentifierName without escape sequences)
    private static readonly Regex IdentifierPattern = new(
        @"^[\p{L}\p{Nl}$_][\p{L}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}$‌‍]*$", RegexOptions.CultureInvariant);

    // a Web Awesome tag name: "wa-" and lowercase words joined with "-"
    private static readonly Regex TagPattern = new("^wa-[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant);

    private string? argsJson;

    #endregion
}

/// <summary>
/// How the browser finds the function of a <see cref="WaJsFunction"/>.
/// </summary>
public enum WaJsFunctionKind
{
    /// <summary>
    /// A function the app registered by name (<see cref="WaJsFunction.Registered"/>).
    /// </summary>
    Registered,

    /// <summary>
    /// An export of an ES module (<see cref="WaJsFunction.Module"/>).
    /// </summary>
    Module,

    /// <summary>
    /// A function on a global dotted path (<see cref="WaJsFunction.Global"/>).
    /// </summary>
    Global
}
