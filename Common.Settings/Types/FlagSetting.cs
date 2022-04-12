using Common.Settings.Internal;
using System.Collections.Specialized;

namespace Common.Settings.Types;

public abstract class FlagSetting<T, TSelf> : SingletonSetting<TSelf>, INotifyCollectionChanged
    where TSelf : FlagSetting<T, TSelf>, new()
    where T : notnull
{

    static readonly Dictionary<T, bool> dict = new();

    /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged"/>
    /// <remarks>Note: <see cref="FlagSetting{T, TSelf}"/> uses only <see cref="NotifyCollectionChangedAction.Replace"/> to indicate changes, and <see cref="NotifyCollectionChangedAction.Reset"/> on <see cref="Clear()"/>.</remarks>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    #region Constructor / Setup

    protected override void OnSetupSingleton()
    {

        if (SettingsUtility.Read<Dictionary<T, bool>>(Name, out var items))
            foreach (var kvp in items)
                Set(kvp.Key, kvp.Value);

        SetValue(dict);

    }

    #endregion
    #region Custom methods

    /// <summary>Sets the flag.</summary>
    public void Set(T key, bool isSet = true)
    {

        if (IsSet(key) == isSet)
            return;

        _ = dict.Remove(key, out var prevFlag);
        dict.Add(key, isSet);
        CollectionChanged?.Invoke(this, new(
            NotifyCollectionChangedAction.Replace,
            newItem: new KeyValuePair<T, bool>(key, isSet),
            oldItem: new KeyValuePair<T, bool>(key, prevFlag)));

        this.Save();

    }

    /// <summary>Unsets the flag.</summary>
    public void Unset(T key) =>
        Set(key, false);

    /// <summary>Gets whatever the flag is set.</summary>
    public bool IsSet(T key) =>
        dict.GetValueOrDefault(key);

    /// <summary>Gets whatever the list is empty.</summary>
    public override bool IsDefault =>
        dict.Count == 0;

    /// <inheritdoc cref="Clear"/>
    public override void Reset() =>
        Clear();

    /// <summary>Clears all flags.</summary>
    public void Clear()
    {

        var items = dict.ToList();

        dict.Clear();
        CollectionChanged?.Invoke(this, new(
            NotifyCollectionChangedAction.Reset,
            newItems: Array.Empty<KeyValuePair<T, bool>>(),
            oldItems: items));

        this.Save();

    }

    #endregion

}
