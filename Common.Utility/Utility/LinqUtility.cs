using System.Collections.Generic;
using System.Linq;

namespace Common.Utility;

/// <summary>Provides utility functions relating to linq.</summary>
public static class LinqUtility
{

    /// <inheritdoc cref="Enumerable.Except{TSource}(IEnumerable{TSource}, IEnumerable{TSource})"/>
    public static IEnumerable<T> Except<T>(this IEnumerable<T> list, T item) =>
        list.Except(new[] { item });

}
