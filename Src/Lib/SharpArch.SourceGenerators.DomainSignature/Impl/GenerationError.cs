namespace SharpArch.SourceGenerators.DomainSignature.Impl;

/// <summary>
///     Flags describing the reasons why an entity class is not in a valid state for signature generation.
/// </summary>
[Flags]
public enum GenerationError
{
    /// <summary>
    ///     The class is valid for signature generation.
    /// </summary>
    None = 0,

    /// <summary>
    ///     The class is not declared as partial.
    /// </summary>
    NotPartial = 1,

    /// <summary>
    ///     The class does not derive from <c>BaseObject</c>.
    /// </summary>
    WrongBaseType = 2,

    /// <summary>
    ///     The class is not declared in a (non-global) namespace.
    /// </summary>
    NoNamespace = 4,

    /// <summary>
    ///     The class is nested inside another type.
    /// </summary>
    Nested = 8,
}