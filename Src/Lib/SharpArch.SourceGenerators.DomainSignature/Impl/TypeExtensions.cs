namespace SharpArch.SourceGenerators.DomainSignature.Impl;

using System.Reflection;
using System.Runtime.CompilerServices;


/// <summary>
///     Extension methods for Type and TypeInfo.
/// </summary>
public static class TypeExtensions
{
    /// <summary>
    ///     Determines whether the specified type is a nullable value type.
    /// </summary>
    /// <param name="typeInfo">The type information to check.</param>
    /// <returns>True if the type is Nullable{T}; otherwise, false.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsNullable(TypeInfo typeInfo)
        => typeInfo.IsGenericType
            && typeInfo.GetGenericTypeDefinition() == typeof(Nullable<>);
}
