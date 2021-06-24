using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;

namespace Common
{

    /// <summary>Represents a setting in an app that is a list of <typeparamref name="T"/>. Notifies and saves when an item is added, removed (in current process only). Values are saved to registry. Also, if <typeparamref name="T"/> implements <see cref="INotifyPropertyChanged"/>, then setting is saved when it is called for any item.</summary>
    /// <typeparam name="T">The type of value to store in this setting.</typeparam>
    /// <typeparam name="TSelf">Used for singleton, pass the type of the subclass. See <see cref="Current"/>.</typeparam>
    public abstract class CollectionSetting<T, TSelf> : Setting<ReadOnlyObservableCollection<T>, TSelf>, IList<T>
        where TSelf : CollectionSetting<T, TSelf>, new()
    {

        #region Hide Validate since it is not supported

        [Obsolete("Not supported by CollectionSetting.")]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override bool Validate(ReadOnlyObservableCollection<T> value) =>
            true;
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

        #endregion
        #region Add

        /// <summary>Specifies whatever to allow duplicates, only relevant by default in <see cref="VerifyAdd(T)"/> Default is false.</summary>
        public virtual bool AllowDuplicates { get; }

        public int Count => ((ICollection<T>)collection).Count;

        public bool IsReadOnly => ((ICollection<T>)collection).IsReadOnly;

        public T this[int index] { get => ((IList<T>)collection)[index]; set => ((IList<T>)collection)[index] = value; }

        /// <summary>Verify whatever this item should be added to the setting.</summary>
        public virtual bool VerifyAdd(T item) =>
            AllowDuplicates || !collection.Contains(item);

        /// <summary>Adds an item to this setting.</summary>
        public void Add(T item)
        {

            if (!VerifyAdd(item))
                return;

            collection.Add(item);
            Save();
            if (item is INotifyPropertyChanged notify)
                notify.PropertyChanged += Windows_PropertyChanged;

        }

        /// <inheritdoc cref="AddRange(T[])"/>
        public void AddRange(IEnumerable<T> items) =>
            AddRange(items.ToArray());

        /// <summary>Adds the items to this setting. Note that this is just a proxy for calling Add(<typeparamref name="T"/>) repeatedly.</summary>
        public void AddRange(params T[] items)
        {
            foreach (var item in items)
                Add(item);
        }

        #endregion
        #region Remove

        /// <summary>Verify whatever the item should be removed.</summary>
        public virtual bool VerifyRemove(T item) =>
            collection.Contains(item);

        /// <summary>Removes an item from this setting.</summary>
        public void Remove(T item)
        {

            if (!VerifyRemove(item))
                return;

            _ = collection.Remove(item);
            Save();
            if (item is INotifyPropertyChanged notify)
                notify.PropertyChanged -= Windows_PropertyChanged;

        }

        /// <inheritdoc cref="AddRange(T[])"/>
        public void RemoveRange(IEnumerable<T> items) =>
            RemoveRange(items.ToArray());

        /// <summary>Removes the items from this setting. Note that this is just a proxy for calling Remove(<typeparamref name="T"/>) repeatedly.</summary>
        public void RemoveRange(params T[] items)
        {
            foreach (var item in items)
                Remove(item);
        }

        #endregion
        #region IList

        public int IndexOf(T item) =>
            ((IList<T>)collection).IndexOf(item);

        public void Insert(int index, T item) =>
            ((IList<T>)collection).Insert(index, item);

        public void RemoveAt(int index) =>
            ((IList<T>)collection).RemoveAt(index);

        public void Clear() =>
            ((ICollection<T>)collection).Clear();

        public bool Contains(T item) =>
            ((ICollection<T>)collection).Contains(item);

        public void CopyTo(T[] array, int arrayIndex) =>
            ((ICollection<T>)collection).CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item) =>
            ((ICollection<T>)collection).Remove(item);

        public IEnumerator<T> GetEnumerator() =>
            ((IEnumerable<T>)collection).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            ((IEnumerable)collection).GetEnumerator();

        #endregion

        /// <summary>The underlying collection.</summary>
        protected ObservableCollection<T> collection = new();

        /// <summary>
        /// <inheritdoc/>
        /// <para>CollectionSetting: <see cref="INotifyPropertyChanged"/> will not be properly registered for items unless <see langword="base"/>.OnSetup() is called.</para>
        /// </summary>
        protected override void OnSetup()
        {
            foreach (var item in Value.OfType<INotifyPropertyChanged>().ToArray())
                item.PropertyChanged += Windows_PropertyChanged;
        }

        void Windows_PropertyChanged(object sender, PropertyChangedEventArgs e) =>
            Save();

        protected override string Serialize(ReadOnlyObservableCollection<T> value) =>
            JsonSerializer.Serialize(collection);

        protected override ReadOnlyObservableCollection<T> Deserialize(string json)
        {

            collection = json is not null
            ? JsonSerializer.Deserialize<ObservableCollection<T>>(json)
            : new();

            return new(collection);

        }

        public override void Reset()
        {
            base.Reset();
            Value = new(collection = new());
        }

    }

}
