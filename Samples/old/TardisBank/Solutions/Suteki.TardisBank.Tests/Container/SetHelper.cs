using System.Collections.Generic;

namespace Suteki.TardisBank.Tests.Container
{
    public static class SetHelper
    {
        public static HashSet<T> ToSet<T>(this IEnumerable<T> x)
        {
            return new HashSet<T>(x);
        }
    }
}