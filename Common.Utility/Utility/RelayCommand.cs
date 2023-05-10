using System;

namespace Common.Utility;

public class RelayCommand : Command
{

    readonly Action callback;

    public RelayCommand(Action callback) =>
        this.callback = callback;

    public override void Execute() =>
        callback.Invoke();

}

public class RelayCommand<T> : Command<T>
{

    readonly Action<T?> callback;

    public RelayCommand(Action<T?> callback) =>
        this.callback = callback;

    public override void Execute(T? param) =>
        callback.Invoke(param);

}
