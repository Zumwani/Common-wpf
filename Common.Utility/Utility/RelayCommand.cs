using System;

namespace Common.Utility;

/// <summary>Represents a command that executes a callback.</summary>
public class RelayCommand : Command
{

    readonly Action callback;

    /// <summary>Creates a new <see cref="RelayCommand"/>.</summary>
    public RelayCommand(Action callback) =>
        this.callback = callback;

    /// <inheritdoc/>
    public override void Execute() =>
        callback.Invoke();

}

/// <summary>Represents a command that executes a callback.</summary>
public class RelayCommand<T> : Command<T>
{

    readonly Action<T?> callback;

    /// <summary>Creates a new <see cref="RelayCommand{T}"/>.</summary>
    public RelayCommand(Action<T?> callback) =>
        this.callback = callback;

    /// <inheritdoc/>
    public override void Execute(T? param) =>
        callback.Invoke(param);

}
