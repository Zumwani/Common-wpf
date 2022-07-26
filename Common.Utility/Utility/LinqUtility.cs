using System.Collections.Generic;
using System.Linq;

namespace Common.Utility;

public static class LinqUtility
{

    /// <inheritdoc cref="Enumerable.Except"/>
    public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T item) =>
        list.Except(new[] { item });

}
