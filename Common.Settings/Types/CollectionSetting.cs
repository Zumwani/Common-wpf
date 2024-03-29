﻿using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;
using Common.Settings.Internal;
using Common.Settings.Utility;

namespace Common.Settings.Types;

/// <summary>Represents a setting of <see cref="ObservableCollection{T}"/>.</summary>
public abstract class CollectionSetting<T, TSelf> : SingletonSetting<TSelf>,
    INotifyCollectionChanged, IList<T>
    where TSelf : CollectionSetting<T, TSelf>, new()
{

    static readonly ObservableCollection<T> list = new();

    /// <summary>Gets whatever collection is empty.</summary>
    public override bool IsDefault => list.Count == 0;

    /// <inheritdoc/>
    protected override bool SetRaw(object? value)
    {
        if (typeof(T).IsAssignableFrom(value?.GetType()))
        {
            Add((T)value);
            return true;
        }
        return false;
    }

    /// <summary>Gets the default items.</summary>
    public virtual IEnumerable<T>? DefaultItems { get; } = null;

    /// <summary><inheritdoc/></summary>
    /// <remarks>Note that if value cannot be read, then <see cref="DefaultItems"/> will be added again.</remarks>
    public override void Reload()
    {

        if (Current != this)
        {
            Current?.Reload();
            return;
        }

        var list = new ObservableCollection<T>();
        if (SettingsUtility.Read<T[]>(Name, out var items))
            AddRange(items);
        else if (DefaultItems is not null)
            AddRange(DefaultItems);
        SetValue(list);

    }

    #region Constructor / Setup

    /// <summary>Creates a new <see cref="CollectionSetting{T, TSelf}"/>.</summary>
    public CollectionSetting() : base() =>
        Mode = BindingMode.OneWay;

    /// <inheritdoc/>
    protected override void OnSetupSingleton() =>
        Reload();

    #endregion
    #region Notify

    void OnCollectionChanged(IEnumerable<T>? removed = null, IEnumerable<T>? added = null, T? removedItem = default, T? addedItem = default)
    {

        UnregisterListeners(removed?.ToArray());
        RegisterListeners(added?.ToArray());

        UnregisterListeners(removedItem);
        UnregisterListeners(addedItem);

        this.Save();

    }

    void RegisterListeners(params T?[]? items)
    {
        if (items is not null)
            foreach (var item in items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged += Item_PropertyChanged;
    }

    void UnregisterListeners(params T?[]? items)
    {
        if (items is not null)
            foreach (var item in items.OfType<INotifyPropertyChanged>())
                item.PropertyChanged -= Item_PropertyChanged;
    }

    void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e) =>
      this.Save();

    #endregion
    #region Custom methods

    #region Add

    /// <inheritdoc cref="List{T}.Add(T)"/>
    public void AddRange(params T[] items) =>
        AddRange(items.AsEnumerable());

    /// <inheritdoc cref="List{T}.Add(T)"/>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item, notify: false);
        OnCollectionChanged(added: items);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    /// <inheritdoc cref="List{T}.Add(T)"/>
    public void Add(T item) =>
        Add(item, notify: true);

    /// <inheritdoc cref="List{T}.Add(T)"/>
    public void Add(T item, bool notify = true)
    {

        list.Add(item);
        if (notify)
        {
            OnCollectionChanged(addedItem: item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
        }

    }

    /// <inheritdoc cref="Collection{T}.Insert(int, T)"/>
    public void Insert(int index, T item)
    {
        list.Insert(index, item);
        OnCollectionChanged(addedItem: item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, index));
    }

    #endregion
    #region Remove

    /// <summary>Removes the items.</summary>
    public void RemoveRange(params T[] items) =>
        RemoveRange(items.AsEnumerable());

    /// <inheritdoc cref="RemoveRange(T[])"/>
    public void RemoveRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            _ = Remove(item, notify: false);
        OnCollectionChanged(removed: items);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    /// <inheritdoc cref="List{T}.Remove(T)"/>
    public bool Remove(T item) =>
        Remove(item, notify: true);

    /// <inheritdoc cref="List{T}.Remove(T)"/>
    public bool Remove(T item, bool notify = true)
    {

        var i = IndexOf(item);
        if (list.Remove(item))
        {

            if (notify)
            {
                OnCollectionChanged(removed: new[] { item });
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, i));
            }

            return true;

        }

        return false;

    }

    /// <inheritdoc cref="List{T}.RemoveAt(int)"/>
    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        OnCollectionChanged(removedItem: item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
    }

    /// <inheritdoc cref="Clear"/>
    public override void Reset() => Clear();

    /// <inheritdoc cref="List{T}.Clear"/>
    public void Clear()
    {
        var items = list.ToArray();
        list.Clear();
        OnCollectionChanged(removed: items);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    #endregion

    /// <summary>Replaces the item at the specified index.</summary>
    public void Replace(int index, T item)
    {
        var oldItem = list[index];
        list[index] = item;
        OnCollectionChanged(removedItem: oldItem, addedItem: item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
    }

    #endregion
    #region IList

    /// <inheritdoc/>
    public T this[int index]
    {
        get => ((IList<T>)list)[index];
        set => Replace(index, value);
    }

    /// <inheritdoc/>
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    /// <inheritdoc/>
    public int Count => ((ICollection<T>)list).Count;
    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

    /// <inheritdoc/>
    public bool Contains(T item) => ((ICollection<T>)list).Contains(item);
    /// <inheritdoc/>
    public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)list).CopyTo(array, arrayIndex);
    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)list).GetEnumerator();
    /// <inheritdoc/>
    public int IndexOf(T item) => ((IList<T>)list).IndexOf(item);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();

    #endregion

}
