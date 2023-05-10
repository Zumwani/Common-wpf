using System;
using System.ComponentModel;

namespace Common.Utility;

public class NotifyProperty<T> : INotifyPropertyChanged
{

    public event PropertyChangedEventHandler? PropertyChanged;

    T? value;
    public virtual T? Value
    {
        get => GetValue();
        set => OnSetValue(value, true);
    }

    public void SetValueWithoutNotify(T? value) => OnSetValue(value, false);

    protected virtual T? GetValue() => value;

    protected virtual void OnSetValue(T? value, bool notify)
    {
        this.value = value;
        if (notify)
            PropertyChanged?.Invoke(this, new(nameof(Value)));
    }

    public static implicit operator T?(NotifyProperty<T> property) =>
        property.Value;


    public Action? OnUpdateRequest { get; init; }

    /// <summary>Requests that whatever is automatically updating this property, if any, do an update immediately.</summary>
    public void RequestUpdate() =>
        OnUpdateRequest?.Invoke();


}

