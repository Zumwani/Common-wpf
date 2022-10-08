using System.IO;

namespace Common.Settings.Utility;

internal enum FileStatus
{
    Created, Cleared, Written
}

public static partial class SettingsUtility
{

    //TODO: Make sure to add check so that we're not raising OnSettingsChangedExternally when we ourselves are writing

    //TODO: Make sure callbacks can be removed when using method names
    //TODO: Prevent duplicate calls

    /// <summary>Provides events for when changes are made to the registry key / folder, that are not ours (or at least not this instance of the application).</summary>
    public static class ChangeWatcher
    {

        internal static FileSystemWatcher? fileWatcher;
        internal static object? registryWatcher;

        static bool isEnabled = true;
        internal static bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                if (fileWatcher is not null) fileWatcher.EnableRaisingEvents = value;
            }
        }

        internal static void Reset()
        {

            DisableFilesystem();
            DisableRegistry();

            if (!HasCallbacks)
                return;

            if (Backend is Backend.Registry)
                EnableRegistry();
            else if (Backend is Backend.FileSystem)
                EnableFilesystem();

        }

        #region Callbacks

        /// <summary>Specifies whatever the watcher is currently watching for changes.</summary>
        /// <remarks><see cref="ChangeWatcher"/> automatically enables and disables itself depending on handlers added or removed.</remarks>
        public static bool IsTracking => (fileWatcher ?? registryWatcher) is not null;

        static bool HasCallbacks => callbacksExternal.Concat(callbacksSelf).OfType<Action>().Any();

        static readonly List<Action> callbacksExternal = new();
        static readonly List<Action> callbacksSelf = new();

        /// <summary>Adds an handler to be called when an external change is detected.</summary>
        /// <remarks>
        /// Starts watcher if needed.
        /// <br/><br/>
        /// Note that <see cref="SettingsUtility.Initialize(Backend, string?, string?, double, bool, bool)"/> must be called before this, if you need to do so explicitly.
        /// </remarks>
        public static void AddHandler(Action action, bool externalChangesOnly = true)
        {

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            callbacksExternal.Add(action);
            if (!externalChangesOnly) callbacksSelf.Add(action);

            if (!IsTracking)
                Reset();

        }

        /// <summary>Removes an handler that was to be called when an external change is detected.</summary>
        /// <remarks>Stops watcher if all handlers are removed.</remarks>
        public static void RemoveHandler(Action action)
        {

            callbacksExternal.Remove(action);
            callbacksSelf.Remove(action);

            if (!HasCallbacks)
                Reset();

        }

        #endregion
        #region Filesystem

        static void DisableFilesystem()
        {
            fileWatcher?.Dispose();
            fileWatcher = null;
        }

        static void EnableFilesystem()
        {

            Initialize();
            if (Path is null) throw new InvalidOperationException("Path must be initialized.");

            fileWatcher = new() { Path = Path, EnableRaisingEvents = IsEnabled, NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName };
            fileWatcher.Error += (s, e) => Reset();
            fileWatcher.Changed += (s, e) => OnExternalChangeDetected();
            fileWatcher.Deleted += (s, e) => OnExternalChangeDetected();

        }

        #endregion
        #region Registry

        static void DisableRegistry()
        {

        }

        static void EnableRegistry()
        {
            throw new NotImplementedException("Watching changes for registry key currently not supported.");
        }

        #endregion

        static void OnExternalChangeDetected()
        {
            foreach (var callback in callbacksExternal)
                callback?.Invoke();
        }

        internal static void NotifyInternalChange()
        {
            foreach (var callback in callbacksSelf)
                callback?.Invoke();
        }

    }

}
