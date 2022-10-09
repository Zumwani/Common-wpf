using Common.Settings.Internal;
using Common.Settings.Utility;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Common.Settings.Types;

public abstract class DictionarySetting<TKey, TValue, TSelf> : SingletonSetting<TSelf>, IDictionary<TKey, TValue>, INotifyCollectionChanged
    where TSelf : DictionarySetting<TKey, TValue, TSelf>, new()
    where TKey : notnull
{

    static readonly Dictionary<TKey, TValue> dict = new();

    /// <summary>Gets whatever collection is empty.</summary>
    public override bool IsDefault => dict.Count == 0;

    protected override bool SetRaw(object? value)
    {
        if (value is KeyValuePair<TKey, TValue> kvp)
            return Add(kvp);
        else if (value is Tuple<TKey, TValue> tuple)
            return Add(tuple);
        return false;
    }

    /// <summary>Gets the default items.</summary>
    public virtual Dictionary<TKey, TValue>? DefaultItems { get; } = null;

    /// <summary><inheritdoc/></summary>
    /// <remarks>Note that if value cannot be read, then <see cref="DefaultItems"/> will be added again.</remarks>
    public override void Reload()
    {

        if (Current != this)
        {
            Current.Reload();
            return;
        }

        var dict = new Dictionary<TKey, TValue>();
        if (SettingsUtility.Read<Dictionary<TKey, TValue>>(Name, out var items))
            foreach (var kvp in items)
                _ = Add(kvp);
        else if (DefaultItems is not null)
            foreach (var item in DefaultItems)
                Set(item);
        SetValue(dict);

    }

    #region Constructor / Setup

    protected override void OnSetupSingleton()
    {
        CollectionChanged += List_CollectionChanged;
        Reload();
    }

    void List_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {

        if (e.Action is NotifyCollectionChangedAction.Remove)
            UnregisterListeners(e.OldItems?.OfType<INotifyPropertyChanged>());

        else if (e.Action == NotifyCollectionChangedAction.Add)
            RegisterListeners(e.NewItems?.OfType<INotifyPropertyChanged>());

        else if (e.Action == NotifyCollectionChangedAction.Reset)
        {
            UnregisterListeners(e.OldItems?.OfType<INotifyPropertyChanged>());
            RegisterListeners(e.NewItems?.OfType<INotifyPropertyChanged>());
        }

        this.Save();

    }

    void RegisterListeners(IEnumerable<INotifyPropertyChanged>? items)
    {
        if (items is not null)
            foreach (var item in items)
                item.PropertyChanged += Item_PropertyChanged;
    }

    void UnregisterListeners(IEnumerable<INotifyPropertyChanged>? items)
    {
        if (items is not null)
            foreach (var item in items)
                item.PropertyChanged -= Item_PropertyChanged;
    }

    void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) =>
      this.Save();

    #endregion
    #region Custom methods

    /// <summary>Adds the value.</summary>
    /// <returns><see langword="true"/> if value could be added. <see langword="false"/> is returned when key already exists.</returns>
    public bool Add(TKey key, TValue value)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, value);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="Add(TKey, TValue)"/>
    public bool Add(KeyValuePair<TKey, TValue> value) =>
        Add(value.Key, value.Value);

    /// <inheritdoc cref="Add(TKey, TValue)"/>
    public bool Add((TKey key, TValue value) value) =>
        Add(value.key, value.value);

    /// <inheritdoc cref="Add(TKey, TValue)"/>
    public bool Add(Tuple<TKey, TValue> value) =>
        Add(value.Item1, value.Item2);


    /// <summary>Sets the value. Adds key if it does not exist, sets value if it does.</summary>
    public void Set(TKey key, TValue value)
    {
        if (!dict.ContainsKey(key))
        {
            dict.Add(key, value);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, new KeyValuePair<TKey, TValue>(key, value)));
        }
        else
        {
            var v = dict[key];
            dict[key] = value;
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, new KeyValuePair<TKey, TValue>(key, value), new KeyValuePair<TKey, TValue>(key, v)));
        }
    }

    /// <inheritdoc cref="Set(TKey, TValue)"/>
    public void Set(KeyValuePair<TKey, TValue> value) =>
        Set(value.Key, value.Value);

    /// <inheritdoc cref="Set(TKey, TValue)"/>
    public void Set((TKey key, TValue value) value) =>
        Set(value.key, value.value);

    /// <inheritdoc cref="Set(TKey, TValue)"/>
    public void Set(Tuple<TKey, TValue> value) =>
        Set(value.Item1, value.Item2);

    /// <inheritdoc cref="Clear"/>
    public override void Reset() =>
        Clear();

    public void Clear()
    {
        dict.Clear();
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    /// <summary>Adds the value.</summary>
    /// <returns><see langword="true"/> if value could be added. <see langword="false"/> is returned when key already exists.</returns>
    public bool Remove(TKey key)
    {
        if (dict.ContainsKey(key) && dict.Remove(key, out var value))
        {
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, new KeyValuePair<TKey, TValue>(key, value)));
            return true;
        }
        return false;
    }

    /// <inheritdoc cref="Remove(TKey, TValue)"/>
    public bool Remove(KeyValuePair<TKey, TValue> value) =>
        Remove(value.Key);

    /// <inheritdoc cref="Remove(TKey, TValue)"/>
    public bool Remove((TKey key, TValue value) value) =>
        Remove(value.key);

    /// <inheritdoc cref="Remove(TKey, TValue)"/>
    public bool Remove(Tuple<TKey, TValue> value) =>
        Remove(value.Item1);

    #endregion
    #region IDictionary

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public TValue this[TKey key] { get => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict)[key]; set => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict)[key] = value; }

    public ICollection<TKey> Keys => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict).Keys;
    public ICollection<TValue> Values => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict).Values;
    public int Count => ((ICollection<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).Count;
    public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).IsReadOnly;

    void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict).Add(key, value);
    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).Add(item);

    public bool ContainsKey(TKey key) => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict).ContainsKey(key);
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => ((IDictionary<TKey, TValue>)DictionarySetting<TKey, TValue, TSelf>.dict).TryGetValue(key, out value);
    public bool Contains(KeyValuePair<TKey, TValue> item) => ((ICollection<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).Contains(item);
    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => ((ICollection<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).CopyTo(array, arrayIndex);
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)DictionarySetting<TKey, TValue, TSelf>.dict).GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)DictionarySetting<TKey, TValue, TSelf>.dict).GetEnumerator();

    #endregion

}
