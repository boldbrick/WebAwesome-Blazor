using System;
using System.Globalization;

namespace WebAwesome.Blazor.Tests.Components;

/// <summary>
/// Sets the current culture and UI culture for the lifetime of the scope (they flow with the async context, so a
/// test's own renders and handlers see them) and restores the previous ones on dispose. Used to prove a conversion
/// is culture-free: pass RenderedAttributeParityTests.HostileCulture, whose date and number formats differ from
/// the invariant culture's.
/// </summary>
internal sealed class CultureScope : IDisposable
{
    /// <summary>
    /// Switches to the given culture.
    /// </summary>
    /// <param name="culture">The culture for the scope</param>
    public CultureScope(CultureInfo culture)
    {
        previousCulture = CultureInfo.CurrentCulture;
        previousUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }

    /// <inheritdoc />
    public void Dispose()
    {
        CultureInfo.CurrentCulture = previousCulture;
        CultureInfo.CurrentUICulture = previousUiCulture;
    }

    #region ------ Internals ------

    private readonly CultureInfo previousCulture;
    private readonly CultureInfo previousUiCulture;

    #endregion
}
