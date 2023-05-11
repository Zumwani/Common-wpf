using System.Windows.Threading;

namespace Common.Utility;

/// <summary>Provides utility methods for working with the app.</summary>
public static partial class AppUtility
{

    static readonly Dispatcher dispatcher;
    static AppUtility() =>
        dispatcher = Dispatcher.CurrentDispatcher;

}
