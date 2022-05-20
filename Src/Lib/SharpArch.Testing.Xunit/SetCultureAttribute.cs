namespace SharpArch.Testing.Xunit;

using System.Globalization;
using System.Reflection;
using global::Xunit.Sdk;


/// <summary>
///     Overrides <see cref="Thread.CurrentThread" /> <see cref="CultureInfo.CurrentCulture" /> and
///     <see cref="CultureInfo.CurrentUICulture" /> with given value.
///     Culture will be restored after method execution.
/// </summary>
[PublicAPI]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public sealed class SetCultureAttribute : BeforeAfterTestAttribute
{
    readonly Lazy<CultureInfo> _culture;
    readonly Lazy<CultureInfo> _uiCulture;
    CultureInfo? _originalCulture;
    CultureInfo? _originalUiCulture;

    /// <summary>
    ///     Overrides the culture and UI culture of the current thread given culture.
    /// </summary>
    /// <param name="culture">The name of the culture to set.</param>
    public SetCultureAttribute(string culture)
        : this(culture, culture)
    {
    }

    /// <summary>
    ///     Overrides the culture and UI culture of the current thread with
    ///     <paramref name="culture" /> and <paramref name="uiCulture" />
    /// </summary>
    /// <param name="culture">The name of the culture.</param>
    /// <param name="uiCulture">The name of the UI culture.</param>
    public SetCultureAttribute(string culture, string uiCulture)
    {
        if (string.IsNullOrWhiteSpace(culture)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(culture));
        if (string.IsNullOrWhiteSpace(uiCulture)) throw new ArgumentException("Value cannot be null or whitespace.", nameof(uiCulture));
        _culture = new Lazy<CultureInfo>(() => new CultureInfo(culture, false));
        _uiCulture = new Lazy<CultureInfo>(() => new CultureInfo(uiCulture, false));
    }

    /// <summary>
    ///     Stores the current <see cref="Thread.CurrentPrincipal" />
    ///     <see cref="CultureInfo.CurrentCulture" /> and <see cref="CultureInfo.CurrentUICulture" />
    ///     and replaces them with the new cultures defined in the constructor.
    /// </summary>
    public override void Before(MethodInfo methodUnderTest)
    {
        _originalCulture = CultureInfo.CurrentCulture;
        _originalUiCulture = CultureInfo.CurrentUICulture;
        CultureInfo.CurrentCulture = _culture.Value;
        CultureInfo.CurrentUICulture = _uiCulture.Value;
        ClearCachedData();
    }

    static void ClearCachedData()
    {
        CultureInfo.CurrentCulture.ClearCachedData();
        CultureInfo.CurrentUICulture.ClearCachedData();
    }

    /// <summary>
    ///     Restores the original <see cref="CultureInfo.CurrentCulture" /> and
    ///     <see cref="CultureInfo.CurrentUICulture" />
    /// </summary>
    public override void After(MethodInfo methodUnderTest)
    {
        if (_originalCulture != null) CultureInfo.CurrentCulture = _originalCulture;
        if (_originalUiCulture != null) CultureInfo.CurrentUICulture = _originalUiCulture;
        ClearCachedData();
    }
}
