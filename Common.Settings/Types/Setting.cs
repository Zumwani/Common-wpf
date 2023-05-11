using System.Windows;
using System.Windows.Data;
using Common.Settings.Internal;
using Common.Settings.Utility;

namespace Common.Settings.Types;

/// <summary>A setting of type <typeparamref name="T"/>.</summary>
public abstract class Setting<T, TSelf> : SingletonSetting<TSelf> where TSelf : Setting<T, TSelf>, new()
{

    #region Constructors / Setup

    /// <summary>Creates a new instance of <see cref="Setting{T, TSelf}"/>.</summary>
    public Setting() : this(new(nameof(Value)))
    { }

    /// <summary>Creates a new instance of <see cref="Setting{T, TSelf}"/>.</summary>
    public Setting(PropertyPath path) : base()
    {
        Path = path;
        Mode = BindingMode.TwoWay;
    }

    /// <inheritdoc/>
    protected override void OnSetupSingleton() =>
        SetValue(SettingsUtility.Read<T>(Name, out var value) ? value : DefaultValue);

    /// <inheritdoc/>
    protected override void OnSetupProxy() =>
        Source = Current;

    #endregion
    #region Properties

    /// <inheritdoc cref="Binding.Path"/>
    public new PropertyPath Path
    {
        get => base.Path;
        set => base.Path = value.Path.StartsWith(nameof(Value)) ? (value) : (new(nameof(Value) + "." + value.Path, value.PathParameters));
    }

    /// <inheritdoc cref="Setting.Value"/>
    public T? Value
    {
        get => GetValue(out var value) ? (T?)value : DefaultValue;
        set => SetValue(value);
    }

    /// <inheritdoc cref="Setting.DefaultValue"/>
    public virtual new T? DefaultValue =>
        base.DefaultValue is not null
        ? (T?)base.DefaultValue
        : default;

    #endregion

    /// <inheritdoc/>
    protected override bool SetRaw(object? value)
    {
        if (typeof(T).IsAssignableFrom(value?.GetType()))
        {
            Value = (T)value;
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    public override void Reload()
    {

        if (Current != this)
        {
            Current.Reload();
            return;
        }

        SetValue(SettingsUtility.Read<T>(Name, out var value) ? value : default);

    }

    /// <summary>Converts a <see cref="Setting{T, TSelf}"/> to <see cref="T"/>.</summary>
    public static implicit operator T?(Setting<T, TSelf> setting) =>
        setting.Value ?? setting.DefaultValue;

}
