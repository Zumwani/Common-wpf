using Common.Settings.Internal;
using System.Windows;

namespace Common.Settings.Types;

/// <summary>A setting of type <typeparamref name="T"/>.</summary>
public abstract class Setting<T, TSelf> : SingletonSetting<TSelf> where TSelf : Setting<T, TSelf>, new()
{

    #region Constructors / Setup

    public Setting() : this(new(nameof(Value)))
    { }

    public Setting(PropertyPath path) : base() =>
        Path = path;

    protected override void OnSetupSingleton() =>
        SetValue(SettingsUtility.Read<T>(Name, out var value) ? value : DefaultValue);

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
        (T?)base.DefaultValue;

    #endregion

    protected override bool SetRaw(object? value)
    {
        if (typeof(T).IsAssignableFrom(value?.GetType()))
        {
            Value = (T)value;
            return true;
        }
        return false;
    }

    public static implicit operator T?(Setting<T, TSelf> setting) =>
        setting.Value ?? setting.DefaultValue;

}
