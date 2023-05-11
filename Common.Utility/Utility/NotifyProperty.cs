using System;
using System.ComponentModel;

namespace Common.Utility;

/// <summary>Represents a property that provides <see cref="INotifyPropertyChanged"/> notifications.</summary>
public class NotifyProperty<T> : INotifyPropertyChanged
{

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    T? value;
    /// <summary>Gets or sets the value of this <see cref="NotifyProperty{T}"/>.</summary>
    public virtual T? Value
    {
        get => GetValue();
        set => OnSetValue(value, true);
    }

    /// <summary>Sets the value of this <see cref="NotifyProperty{T}"/> without raising <see cref="PropertyChanged"/>.</summary>
    public void SetValueWithoutNotify(T? value) => OnSetValue(value, false);

    /// <summary>Gets the value of this <see cref="NotifyProperty{T}"/>.</summary>
    protected virtual T? GetValue() => value;

    /// <summary>Called when setting the value of this <see cref="NotifyProperty{T}"/>.</summary>
    protected virtual void OnSetValue(T? value, bool notify)
    {
        this.value = value;
        if (notify)
            PropertyChanged?.Invoke(this, new(nameof(Value)));
    }

    /// <summary>Converts a <see cref="NotifyProperty{T}"/> to <see cref="T"/>.</summary>
    public static implicit operator T?(NotifyProperty<T> property) =>
        property.Value;

    /// <summary>Occurs when <see cref="RequestUpdate"/> is called.</summary>
    public Action? OnUpdateRequest { get; init; }

    /// <summary>Requests that whatever is automatically updating this property, if any, do an update immediately.</summary>
    public void RequestUpdate() =>
        OnUpdateRequest?.Invoke();


}

