using Common.Settings.Internal;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

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

    protected override void OnSetupSingleton()
    {

        list.CollectionChanged += List_CollectionChanged;

        if (SettingsUtility.Read<T[]>(Name, out var items))
            AddRange(items);
        else if (DefaultItems is not null)
            AddRange(DefaultItems);
        SetValue(list);

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

    #region Add

    /// <inheritdoc cref="AddRange(IEnumerable{T})"/>
    public void AddRange(params T[] items) =>
        AddRange(items.AsEnumerable());

    /// <inheritdoc cref="List{T}.Add(T)"/>
    public void AddRange(IEnumerable<T> items)
    {
        foreach (var item in items)
            Add(item, notify: false);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset, newItems: items.ToList(), oldItems: Array.Empty<T>()));
    }

    public void Add(T item) =>
        Add(item, notify: true);

    /// <inheritdoc cref="Add(T)"/>
    public void Add(T item, bool notify = true)
    {
        list.Add(item);
        if (notify)
            CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Add, item));
    }

    public void Insert(int index, T item)
    {
        list.Insert(index, item);
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
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset, newItems: Array.Empty<T>(), oldItems: items.ToList()));
    }

    public bool Remove(T item) =>
        Remove(item, notify: true);

    /// <inheritdoc cref="Remove(T)"/>
    public bool Remove(T item, bool notify = true)
    {
        if (list.Remove(item))
        {
            if (notify)
                CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item));
            return true;
        }
        return false;
    }

    public void RemoveAt(int index)
    {
        var item = list[index];
        list.RemoveAt(index);
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Remove, item, index));
    }

    /// <inheritdoc cref="Clear"/>
    public override void Reset() => Clear();

    public void Clear()
    {
        var items = list.ToArray();
        list.Clear();
        CollectionChanged?.Invoke(this, new(NotifyCollectionChangedAction.Reset, newItems: Array.Empty<T>(), oldItems: items));
    }

    #endregion

    public void Replace(int index, T item)
    {
        var oldItem = list[index];
        list[index] = item;
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
