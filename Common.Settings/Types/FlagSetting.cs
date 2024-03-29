﻿using System.Collections.Specialized;
using Common.Settings.Internal;
using Common.Settings.Utility;

namespace Common.Settings.Types;

/// <summary><![CDATA[Represents a setting of type Dictionary<T, bool>.]]></summary>
public abstract class FlagSetting<T, TSelf> : SingletonSetting<TSelf>, INotifyCollectionChanged
    where TSelf : FlagSetting<T, TSelf>, new()
    where T : notnull
{

    static readonly Dictionary<T, bool> dict = new();

    /// <inheritdoc cref="INotifyCollectionChanged.CollectionChanged"/>
    /// <remarks>Note: <see cref="FlagSetting{T, TSelf}"/> uses only <see cref="NotifyCollectionChangedAction.Replace"/> to indicate changes, and <see cref="NotifyCollectionChangedAction.Reset"/> on <see cref="Clear()"/>.</remarks>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <summary>Gets the default items.</summary>
    public virtual Dictionary<T, bool>? DefaultItems { get; } = null;

    /// <summary><inheritdoc/></summary>
    /// <remarks>Note that if value cannot be read, then <see cref="DefaultItems"/> will be added again.</remarks>
    public override void Reload()
    {

        if (Current != this)
        {
            Current.Reload();
            return;
        }

        var dict = new Dictionary<T, bool>();
        if (SettingsUtility.Read<Dictionary<T, bool>>(Name, out var items))
            foreach (var kvp in items)
                Set(kvp.Key, kvp.Value);
        else if (DefaultItems is not null)
            foreach (var item in DefaultItems)
                Set(item.Key, item.Value);
        SetValue(dict);

    }

    #region Constructor / Setup

    /// <inheritdoc/>
    protected override void OnSetupSingleton() =>
        Reload();

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
