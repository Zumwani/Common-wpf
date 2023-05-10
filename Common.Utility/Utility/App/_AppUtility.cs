using System.Windows.Threading;

namespace Common.Utility;

public static partial class AppUtility
{

    static readonly Dispatcher dispatcher;
    static AppUtility() =>
        dispatcher = Dispatcher.CurrentDispatcher;

}
