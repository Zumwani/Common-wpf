﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace Common
{

    public interface ISetting
    {
        public object Value { get; set; }
        public object DefaultValue { get; }
        public string Key { get; }
        public string DisplayName { get; }
        public void Clamp(ref object value);
        public bool Validate(object value);
        public void Reset();
        public bool IsDefault { get; }
        void DoWrite();
        DispatcherTimer WriteTimer { get; }
    }

    /// <summary>Represents a setting in an app. Notifies when value changes (in current process only). Values are saved to registry.</summary>
    /// <typeparam name="T">The type of value to store in this setting.</typeparam>
    /// <typeparam name="TSelf">Used for singleton, pass the type of the subclass. See <see cref="Current"/>.</typeparam>
    public abstract class Setting<T, TSelf> : Binding, ISetting, INotifyPropertyChanged where TSelf : Setting<T, TSelf>, new()
    {

        #region Setup

        public Setting()
        {
            Mode = BindingMode.TwoWay;
            Source = Current;
            Path = new PropertyPath(nameof(Value));
        }

        /// <summary>The current instance of this setting.</summary>
        public static TSelf Current { get; } = new TSelf().Setup();

        //Called when created by Current, we inherit from MarkupExtension, so we don't want to do this for those instances, and we want to be singleton anyways
        TSelf Setup()
        {
            value = Read();
            return (TSelf)this;
        }

        /// <summary>Occurs when Value changes.</summary>
        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(bool write = true, [CallerMemberName] string name = "")
        {
            if (write)
                Write();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        #endregion
        #region ISetting

        void ISetting.Clamp(ref object value)
        {

            if (Convert.ChangeType(value, typeof(T)) is not T t)
                return;

            Clamp(ref t);
            value = t;

        }

        bool ISetting.Validate(object value) =>
            Convert.ChangeType(value, typeof(T)) is T t && Validate(t);

        object ISetting.Value 
        { 
            get => value; 
            set => Value = (T)value;
        }

        object ISetting.DefaultValue => DefaultValue;

        DispatcherTimer ISetting.WriteTimer => WriteTimer;

        void ISetting.DoWrite() =>
            DoWrite();

        #endregion
        #region Registry

        DispatcherTimer WriteTimer { get; set; }

        void DoWrite()
        {
            Debug.WriteLine("Write: " + Key);
            using var key = Settings.RegKey(writable: true);
            key.SetValue(Key, Value);
            WriteTimer.Stop();
        }

        /// <summary>Writes the value to registry.</summary>
        void Write()
        {
            
            //We're in design mode, while we don't want to save values here, AssemblyName is wrong anyway (might only be when using vs hosting process?)
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;

            WriteTimer ??= new DispatcherTimer(TimeSpan.FromSeconds(0.5), DispatcherPriority.Background, (s, e) => DoWrite(), Dispatcher.CurrentDispatcher);
            WriteTimer.Stop();
            WriteTimer.Start();

        }

        /// <summary>Reads the value from registry.</summary>
        T Read()
        {

            using var key = Settings.RegKey();
            var value = key?.GetValue(Key, DefaultValue);

            if (Convert.ChangeType(value, typeof(T)) is T t)
                return t;
            else
                return DefaultValue;

        }

        /// <summary>Deletes the value from registry.</summary>
        void DeleteValue()
        {
            using var key = Settings.RegKey(writable: true);
            key?.DeleteValue(Key);
        }

        /// <summary>Refreshes the value from the registry.</summary>
        public void Refresh() =>
            Value = Read();

        #endregion
        #region Validation

        /// <summary>Clamps the value if outside range of valid values.</summary>
        public virtual void Clamp(ref T value)
        { }

        /// <summary>Validates the value.</summary>
        public virtual bool Validate(T value) =>
            true;

        #endregion

        /// <summary>The key of this setting. This is <see cref="Type.FullName"/> of this setting.</summary>
        public string Key => GetType().FullName;

        /// <summary>An optional convience property for those times when we want to display this property in some kind of list, and we want a display name, rather than <see cref="Key"/>.</summary>
        public virtual string DisplayName => GetType().Name;

        /// <summary>The default value of this setting, which is returned when value does not exist in registry, or value is of wrong type.</summary>
        public virtual T DefaultValue { get; }

        private T value;
        /// <summary>The value of this setting.</summary>
        public T Value 
        { 
            get => value;
            set 
            {
                Clamp(ref value);
                if (Validate(Value) && !EqualityComparer<T>.Default.Equals(value, this.value))
                {
                    this.value = value;
                    OnPropertyChanged(); 
                }
            }
        }

        /// <summary>Gets if Value is default.</summary>
        public bool IsDefault => EqualityComparer<T>.Default.Equals(Value, DefaultValue);

        /// <summary>Resets value to default, this deletes value from registry until new value is set.</summary>
        public void Reset()
        {
            value = DefaultValue;
            OnPropertyChanged(write: false);
            DeleteValue();
        }

        public static implicit operator T (Setting<T, TSelf> setting) =>
            setting.value;

    }

}