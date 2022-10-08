namespace Common.Settings.Utility;

static class DictionaryUtility
{

    public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dict, TKey key, TValue value) where TKey : notnull
    {
        if (dict.ContainsKey(key))
            dict[key] = value;
        else
            dict.Add(key, value);
    }

}