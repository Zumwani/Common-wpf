using System.Collections.Generic;

namespace Common.Utility;

/// <summary>Provides utility functions relating to <see cref="Dictionary{TKey, TValue}"/>.</summary>
public static class DictionaryUtility
{

    /// <summary>Sets <paramref name="value"/> to the <paramref name="key"/>. Adds <paramref name="key"/> if it does not exist.</summary>
    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
    {
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
    }

}
