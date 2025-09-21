We need to build new Blazor bindings for Web Assembly 3.0 components.

Bindings must be done in pure C#, no Razor. Hence, the markup is constructed manually as a sequence of `builder.OpenElement`, `builder.Add*`, …, `builder.CloseElement`.

Rules:
- Blazor bindings shall be named `Wa*` (e.g. `wa-button` → `WaButton`).
- All documented properties and events of `wa-*` components must be implemented.
- Properties must be strongly typed. Where an enumeration of values is specified in the documentation, `enum` type must be used.
- Input elements must inherit from Blazor's `InputBase<T>`. Common ancestor for `wa-*` input components must be introduced, and strongly-typed ancestor per `wa-input`-type created.
- Layout Utilities which have no `wa-*` component but a known markup structure + CSS class(s) must be implemented as Blazor components as well.
- Opportunities for other common ancestors shall be analyzed and recommended.
- Event Scope: Many components have multiple events. Implement all documented events.
- Validation: Beyond basic `InputBase<T>` integration, implement enhanced validation features specific to `wa-*` components as well.
- When outputting elements and attributes, never use the `sequence++` anti-pattern; always number with contants (e.g. `builder.AddAttributeIfNotNullOrEmpty(10, "href", Href);`). Gaps in sequences are OK. Hence, when calling a method to build a portion, pass constant (e.g. 15) and in the method do `sequence + 0`, `sequence + 1` etc.
-  When JS interop is needed, point that out. It's acceptable; however, the preference is using Blazor features over custom *.js files. Prefer implementing basic functionality, make stubs for features that required JS and TODO them properly.

# Example

The `wa-textarea` component:
- documentation: `inputs\WebAwesome\components\textarea.md`
- resulting C# source code: `src\WebAwesome.Blazor\Components\WaTextarea.cs`

Common ancestor for input components: `src\WebAwesome.Blazor\Base\WaInputBase.cs`

Other pre-existing components in: `src\WebAwesome.Blazor\Components\`

Pre-existing layouts in: `src\WebAwesome.Blazor\Layouts\`

# Inputs
- Coding rules: CLAUDE.md
- Web Awesome component documentation: `inputs\WebAwesome`
- Source code of some built-in Blazor components as an inspiration how to do it: `inputs\Blazor`
- Pre-existing enums for components: `src\WebAwesome.Blazor\Components\Enums.cs`
- Pre-existing enums for layuouts: `src\WebAwesome.Blazor\Layout\Enums.cs`

# Outpus
- Location of our WebAwesome.Blazor project: `src\WebAwesome.Blazor`
- Produce `*.cs` files into `Components\` and `Layouts\`.

# What **NOT** to do
- Do not produce any *.razor markup.
- No JavaScript files. When a JavaScript would be necessary, **stop work, provide your recommendation and ask how to proceed**.

# Work process

Work in phases:
1. Analyze inputs and make a plan. Ask questions. Wait for clarifications and approval.
2. Work step-by-step on components by logical groups. First, work on a single component and wait for feedback.
3. Learn from feedback. Work on another group and wait for feedback. Take maximu 10 components per group.

Now start with Phase 1.
