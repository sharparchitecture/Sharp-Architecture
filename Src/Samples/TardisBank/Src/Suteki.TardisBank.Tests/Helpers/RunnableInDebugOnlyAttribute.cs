namespace Suteki.TardisBank.Tests.Helpers;

using System.Diagnostics;
using Xunit;


/// <summary>
///     Run test only if debugger attached.
///     Taken from https://lostechies.com/jimmybogard/2013/06/20/run-tests-explicitly-in-xunit-net/
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public sealed class RunnableInDebugOnlyAttribute : FactAttribute
{
    public RunnableInDebugOnlyAttribute()
    {
        if (!Debugger.IsAttached)
        {
            Skip = "Only running in interactive mode.";
        }
    }
}
