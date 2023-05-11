using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using Common.Settings.Utility;

namespace Common.Settings.Internal;

/// <summary>Base class for settings.</summary>
public abstract class Setting : Binding, INotifyPropertyChanged
{

    /// <summary>Creates a new <see cref="Setting"/>.</summary>
    public Setting(object source)
    {
        Source = source;
        Mode = BindingMode.TwoWay;
    }

    /// <summary>Gets whatever this instance is being set up.</summary>
    internal protected bool IsSettingUp { get; protected set; }

    #region INotifyPropertyChanged

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>Raises <see cref="PropertyChanged"/>, by default.</summary>
    protected virtual void OnPropertyChanged([CallerMemberName] string name = "") =>
        PropertyChanged?.Invoke(this, new(name));

    #endregion
    #region Properties

    object? Value { get; set; }

    /// <summary>The default value of this setting.</summary>
    internal virtual object? DefaultValue { get; }

    /// <summary>The name of the registry key value.</summary>
    public virtual string Name => GetType().FullName ?? throw new InvalidOperationException("GetType().FullName is null.");

    /// <summary>The display name of this setting.</summary>
    public virtual string DisplayName => GetType().Name;

    /// <inheritdoc cref="Binding.Path"/>
    internal new PropertyPath Path
    {
        get => base.Path;
        set => base.Path = value;
    }

    #endregion
    #region Get / set value

    /// <summary>Sets the value of this setting.</summary>
    protected void SetValue(object? value)
    {

        if (Equals(value, Value))
            return;

        UnregisterListeners();
        Value = value;
        RegisterListeners();

        OnPropertyChanged(nameof(Value));
        this.Save();

    }

    void Value_PropertyChanged(object? sender, PropertyChangedEventArgs e) =>
        this.Save();

    void RegisterListeners()
    {
        if (Value is INotifyPropertyChanged prop)
            prop.PropertyChanged += Value_PropertyChanged;
    }

    void UnregisterListeners()
    {
        if (Value is INotifyPropertyChanged prop)
            prop.PropertyChanged -= Value_PropertyChanged;
    }

    /// <summary>Gets the value of this setting.</summary>
    protected bool GetValue([NotNullWhen(true)] out object? value) =>
        (value = Value ?? DefaultValue) is not null;

    /// <summary>Gets whatever value is <see cref="DefaultValue"/>.</summary>
    public virtual bool IsDefault =>
        Equals(Value, DefaultValue);

    /// <summary>Reset this setting to <see cref="DefaultValue"/>.</summary>
    public virtual void Reset() =>
        SetValue(DefaultValue);

    internal bool GetValueInternal([NotNullWhen(true)] out object? value) =>
        GetValue(out value);

    #endregion

    /// <summary>Reloads this setting.</summary>
    /// <remarks>
    /// Value becomes, if:<br/>
    /// Backend value has been deleted: <see langword="default"/>.<br/>
    /// Backend value cannot be deserialzed: <see langword="default"/>, or <see cref="System.Text.Json.JsonException"/> is thrown if <see cref="SettingsUtility.ThrowOnDeserializationErrors"/> is <see langword="true"/>.
    /// </remarks>
    public virtual void Reload() =>
        throw new NotSupportedException();

    /// <summary>Adds the value directly.</summary>
    /// <returns><see langword="true"/> if <paramref name="value"/> could be added.</returns>
    protected virtual bool SetRaw(object? value) =>
        false;

    /// <inheritdoc cref="SetRaw(object?)"/>
    internal bool SetRawInternal(object? value) =>
        SetRaw(value);

    /// <summary>Occurs before saving to persistent store.</summary>
    public virtual void OnBeforeSave()
    { }

}
