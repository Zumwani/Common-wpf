using System.Collections.Generic;

namespace Common
{
    public static class SettingExtensions
    {

        #region IList

        /// <summary>
        /// <para>Adds an item to the <typeparamref name="TList"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Add<T, TList, TSetting>(this TSetting setting, T item, bool allowDuplicate = true)
            where TSetting : Setting<TList, TSetting>, new()
            where TList : IList<T>
        {
            if (allowDuplicate || !setting.Value.Contains(item))
            {
                setting.Value.Add(item);
                setting.Save();
            }
        }

        /// <summary>
        /// <para>Removes an item from the <typeparamref name="TList"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Remove<T, TList, TSetting>(this TSetting setting, T item)
            where TSetting : Setting<TList, TSetting>, new()
            where TList : IList<T>
        {
            if (setting.Value.Contains(item))
            {
                setting.Value.Remove(item);
                setting.Save();
            }
        }

        /// <summary>
        /// <para>Inserts an item to the <typeparamref name="TList"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Insert<T, TList, TSetting>(this TSetting setting, int index, T item, bool allowDuplicate = true)
            where TSetting : Setting<TList, TSetting>, new()
            where TList : IList<T>
        {
            if (allowDuplicate || !setting.Value.Contains(item))
            {
                setting.Value.Remove(item);
                setting.Value.Insert(index, item);
                setting.Save();
            }
        }

        /// <summary>
        /// <para>Replaces an item in the <typeparamref name="TList"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Replace<T, TList, TSetting>(this TSetting setting, int index, T item, bool removeExisting = true)
            where TSetting : Setting<TList, TSetting>, new()
            where TList : IList<T>
        {

            if (removeExisting && setting.Value.Contains(item))
                setting.Value.Remove(item);

            setting.Value[index] = item;
            setting.Save();

        }

        #endregion
        #region IDictionary

        /// <summary>
        /// <para>Adds an item to the <typeparamref name="TDictionary"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Add<TKey, TValue, TDictionary, TSetting>(this TSetting setting, TKey key, TValue value)
            where TSetting : Setting<TDictionary, TSetting>, new()
            where TDictionary : IDictionary<TKey, TValue>
            where TKey : notnull
        {
            setting.Value.Add(key, value);
            setting.Save();
        }

        /// <summary>
        /// <para>Removes an item from the <typeparamref name="TDictionary"/>.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Remove<TKey, TValue, TDictionary, TSetting>(this TSetting setting, TKey key)
            where TSetting : Setting<TDictionary, TSetting>, new()
            where TDictionary : IDictionary<TKey, TValue>
            where TKey : notnull
        {
            setting.Value.Remove(key);
            setting.Save();
        }

        /// <summary>
        /// <para>Sets an item to the <typeparamref name="TDictionary"/>, this adds key if it does not exist, sets value if it does.</para>
        /// <para>Saves <typeparamref name="TSetting"/> to registry.</para>
        /// </summary>
        public static void Set<TKey, TValue, TDictionary, TSetting>(this TSetting setting, TKey key, TValue value)
            where TSetting : Setting<TDictionary, TSetting>, new()
            where TDictionary : IDictionary<TKey, TValue>
            where TKey : notnull
        {

            if (!setting.Value.ContainsKey(key))
                setting.Value.Add(key, value);
            else
                setting.Value[key] = value;

            setting.Save();

        }

        #endregion

    }

}
