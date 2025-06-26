namespace SharpArch.SourceGenerators.DomainSignature.Impl;

/// <summary>
///     String extension methods
/// </summary>
public static class StringExtensions
{
    /// <summary>
    ///     Extension method to convert first character of a string to lowercase
    /// </summary>
    public static string ToLowerFirstChar(this string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        return char.ToLowerInvariant(input[0]) + input.Substring(1);
    }
}
