using Common.Settings.Internal;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Data;

namespace Common.Settings.Types;

/// <summary>Represents a setting of <see cref="ObservableCollection{T}"/>.</summary>
public abstract class CollectionSetting<T, TSelf> : SingletonSetting<TSelf>,
    INotifyCollectionChanged, IList<T>
    where TSelf : CollectionSetting<T, TSelf>, new()
{

    static readonly ObservableCollection<T> list = new();

    /// <summary>Gets whatever collection is empty.</summary>
    public override bool IsDefault => list.Count == 0;

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

    #region Constructor / Setup

    public CollectionSetting() : base() =>
        Mode = BindingMode.OneWay;

    protected override void OnSetupSingleton()
    {

        if (SettingsUtility.Read<T[]>(Name, out var items))
            AddRange(items);
        else if (DefaultItems is not null)
            AddRange(DefaultItems);
        SetValue(list);

    }

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

    /// <inheritdoc cref="AddRange(IEnumerable{T})"/>
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

    public void Add(T item) =>
        Add(item, notify: true);

    /// <inheritdoc cref="Add(T)"/>
    public void Add(T item, bool notify = true)
    {

        list.Add(item);
        if (notify)
        {
            OnCollectionChanged(addedItem: item);
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
        }

    }

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

    public bool Remove(T item) =>
        Remove(item, notify: true);

    /// <inheritdoc cref="Remove(T)"/>
    public bool Remove(T item, bool notify = true)
    {

        if (list.Remove(item))
        {

            if (notify)
            {
                OnCollectionChanged(removed: new[] { item });
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
            }

            return true;
        }

        return false;

    }

    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        OnCollectionChanged(removedItem: item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
    }

    /// <inheritdoc cref="Clear"/>
    public override void Reset() => Clear();

    public void Clear()
    {
        var items = list.ToArray();
        list.Clear();
        OnCollectionChanged(removed: items);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset));
    }

    #endregion

    public void Replace(int index, T item)
    {
        var oldItem = list[index];
        list[index] = item;
        OnCollectionChanged(removedItem: oldItem, addedItem: item);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Replace, item, oldItem, index));
    }

    #endregion
    #region IList

    public T this[int index]
    {
        get => ((IList<T>)list)[index];
        set => Replace(index, value);
    }

    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    public int Count => ((ICollection<T>)list).Count;
    public bool IsReadOnly => ((ICollection<T>)list).IsReadOnly;

    public bool Contains(T item) => ((ICollection<T>)list).Contains(item);
    public void CopyTo(T[] array, int arrayIndex) => ((ICollection<T>)list).CopyTo(array, arrayIndex);
    public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)list).GetEnumerator();
    public int IndexOf(T item) => ((IList<T>)list).IndexOf(item);

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)list).GetEnumerator();

    #endregion

}
