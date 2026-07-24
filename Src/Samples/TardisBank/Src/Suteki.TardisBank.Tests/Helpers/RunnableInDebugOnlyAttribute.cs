namespace Suteki.TardisBank.Tests.Helpers;

using System.Diagnostics;
using System.Runtime.CompilerServices;
using Xunit;


/// <summary>
///     Run test only if debugger attached.
///     Taken from https://lostechies.com/jimmybogard/2013/06/20/run-tests-explicitly-in-xunit-net/
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RunnableInDebugOnlyAttribute : FactAttribute
{
#if NET8_0_OR_GREATER
    public RunnableInDebugOnlyAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
#else
    public RunnableInDebugOnlyAttribute()
#endif
    {
        if (!Debugger.IsAttached)
            Skip = "Only running in interactive mode.";
    }
}
