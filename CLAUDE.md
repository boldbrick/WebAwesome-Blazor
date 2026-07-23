# CLAUDE.md

This file provides guidance to Claude Code when working with code in this repository.

## Technology Stack

### Core Framework
- **.NET 9** with **C# 13**
- **ASP.NET Core 9** for web framework
- **Blazor** for UI (no JavaScript/TypeScript)

### Solution Configuration
- **Solution Format**: XML-based `.slnx` format
- **Central Package Management** via `Directory.Packages.props`
- **Build Customizations** via `Directory.Build.props` and `Version.props`
- **Target Runtime**: Release builds for .NET 9.0.x

## Build Commands

- **Build Debug**: `dotnet build src/WebAwesome.slnx -p:Configuration=Debug` or run `src/dev_build_Debug.cmd`
- **Build Release**: `dotnet build src/WebAwesome.slnx -p:Configuration=Release` or run `src/dev_build_Release.cmd`
- **Publish Release**: Run `src/dev_publish_Release.cmd`
- **Run Tests**: `dotnet test src/WebAwesome.slnx`

## Code Style Rules

### Coding Practices

#### Nullability
- Include proper nullability constraints with `?` in type indications
- Disable nullability warnings for DTOs/DAOs filled by external engines:
```csharp
#nullable disable
public class ComponentDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
}
#nullable restore
```

#### Records and Properties
- Use records for simple data-holding classes
- Use automatic properties unless backing field access is necessary
- When using backing fields, declare them immediately **after** the property:
```csharp
public int Width
{
    get => width;
    set
    {
        if (value < 0) throw new ArgumentException("Width cannot be negative");
        width = value;
    }
}
private int width;
```

#### Control Flow
- Use braces for multi-line or complex conditions
- Single-line statements without braces are acceptable for simple cases:
```csharp
// Good - single line
if (value == null) throw new ArgumentNullException(nameof(value));

// Good - complex condition needs braces
if (component != null && component.IsVisible && component.State == ComponentState.Active)
{
    RenderComponent(component);
}
```

### Naming Conventions

- **NO underscore prefixes** for fields (no `_field`, no `m_field`)
- Use `this.` to resolve naming conflicts
- Clear, descriptive names for all identifiers

### Code Organization

#### File Structure
- Use `namespace WebAwesome.Blazor;`, i.e. file-scoped, no braces
- Explicit usings (no implicit usings)
- One class per file (unless small related types like enums/records)
- Folder structure follows namespace structure

#### Class Organization with Regions

Complex classes must be organized with regions in this order:

```csharp
public class ComponentManager : IComponentManager
{
    // Public members without region (properties first, then methods)
    public ILayoutManager LayoutManager { get; }
    public int MinimumWidth { get; set; } = 50;

    public ComponentResult CreateComponent(ComponentRequest request)
    {
        // implementation
    }

    #region ------ Constructors ------

    public ComponentManager(ILayoutManager layoutManager)
    {
        LayoutManager = layoutManager ?? throw new ArgumentNullException(nameof(layoutManager));
    }

    #endregion

    #region ------ Implementation of IComponentManager ------

    public bool CanRender(string userId, string componentId)
    {
        // implementation
    }

    #endregion

    #region ------ Internals ------

    private void ValidateComponent(ComponentRequest request)
    {
    }

    // Private members MUST be in Internals region
    private readonly object renderLock = new();

    #endregion

    #region ------ Interface for descendants ------

    protected virtual void OnComponentRendered(Component component)
    {
        // Protected members for inheritance
    }

    #endregion
}
```

### Comments and Documentation

#### Documentation Comments
- **Required** for all non-private members
- Include parameter descriptions and important exceptions
- Avoid redundant phrases like "Gets or sets..."
- **Never use `<see langword="..."/>`** — write keywords like `true`, `false`, `null` as plain text
```csharp
/// <summary>
/// Creates a new UI component with the specified configuration.
/// </summary>
/// <param name="request">Component request containing type, dimensions, and properties</param>
/// <returns>Result of the component creation including rendered HTML</returns>
/// <exception cref="InvalidComponentException">Thrown when component validation fails</exception>
public ComponentResult CreateComponent(ComponentRequest request)
```

#### Code Comments
- Start with lowercase, no ending period
- Place before the code block being commented
```csharp
// validate component dimensions are within limits
if (request.Width < MinimumWidth)
    throw new InvalidComponentException($"Component width must be at least {MinimumWidth}");

// check user has permission to create this component type
var permissions = GetUserPermissions(request.UserId);
```

## Collaboration Guidelines

When working on this codebase:
1. **Ask for clarification** when requirements are incomplete or ambiguous
2. **Work step-by-step**: analyze → plan → implement
3. **Use existing patterns** - check neighboring files for conventions
4. **Focus on the task** - don't introduce unrelated improvements
5. **Verify file states** - files may have been modified externally
6. **Follow the exact region structure** for complex classes
7. **Private members must be in Internals region**, not inline with public members (this is a hard rule)

## Hard Rules (Must Follow)

### Shell Rules
- **ALWAYS use PowerShell** for shell commands — **NEVER use Bash**. This is a Windows environment.
- This rule applies to **all agents, subagents, and skills** operating in this repository — pass it along when delegating work.

### Package Management Rules
- **NEVER add version numbers** to PackageReference items in .csproj files
- **ALWAYS add packages** to Directory.Packages.props first with version numbers
- **ALWAYS obey central package management** - the solution uses centralized package versions

### Using Statements Rules
- **NEVER turn on implicit usings** - always use explicit using statements
- **ALWAYS ensure correct using statements** - add all required System.* and other namespaces explicitly
- **Check existing patterns** - look at neighboring files for using statement conventions

### No Magic Constants Rule
- **NEVER use magic constants**, always declare them as constants in e.g. `Constants.cs`
- **ALWAYS pass user context** as a parameter or obtain from current context
- If a method needs a user ID, it must be an explicit parameter
- System operations should still have traceable user context

### Property Inheritance Rules
- **NEVER shadow base class properties** with `new` keyword unless absolutely necessary
- **Use inherited properties** from base classes instead of redeclaring them

### Security Rules
- **NEVER hard-code _specific_ API keys, tokens, passwords, or any sensitive credentials** in source code or configuration files
- **NEVER include license keys** (e.g., Font Awesome Pro tokens, third-party service keys) directly in code or configuration files
- **ALWAYS use configuration systems** - `appsettings.json`, environment variables, or secure key vaults
- **ALWAYS use placeholder values** in example code or configuration files with clear comments indicating configuration is needed
- **NEVER commit real secrets** to version control, even in comments or documentation
```csharp
// Good - use configuration
private readonly string apiKey = configuration["ExternalApi:ApiKey"] ?? throw new ConfigurationErrorsException("ExternalApi:ApiKey not configured");

// Bad - hard-coded secret
private readonly string apiKey = "sk_live_abc123def456...";

// Good - placeholder in examples
private const string EXAMPLE_API_KEY = "your_api_key_here"; // Configure in appsettings.json
```