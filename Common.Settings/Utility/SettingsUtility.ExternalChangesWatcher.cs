using Microsoft.Win32;
using System.IO;

namespace Common.Settings.Utility;

internal enum FileStatus
{
    Created, Cleared, Written
}

public static partial class SettingsUtility
{

    /// <summary>Provides events for when changes are made to the registry key / folder, that are not ours (or at least not this instance of the application).</summary>
    public static class ChangeWatcher
    {

        internal static FileSystemWatcher? fileWatcher;
        internal static RegistryMonitor? registryWatcher;

        static bool isEnabled = true;
        internal static bool IsEnabled
        {
            get => isEnabled;
            set
            {
                isEnabled = value;
                if (fileWatcher is not null) fileWatcher.EnableRaisingEvents = value;
                if (registryWatcher is not null) registryWatcher.IsEnabled = value;
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

        /// <summary>Specifies whatever the watcher is currently watching for changes.</summary>
        /// <remarks><see cref="ChangeWatcher"/> automatically enables and disables itself depending on handlers added or removed.</remarks>
        public static bool IsTracking => fileWatcher is not null || registryWatcher is not null;

        #region Callbacks

        /// <param name="name">The name of the setting that changed.<br/>Note that this is not supported when using registry backend.</param>
        /// <param name="isExternalChange">Specifies whatever this change was external, or whatever this instance of this app wrote it.</param>
        public delegate void ChangeDetected(string? name, bool isExternalChange);

        static bool HasCallbacks => callbacks.OfType<ChangeDetected>().Any();

        static readonly List<ChangeDetected> callbacks = new();

        /// <summary>
        /// Adds an handler to be called when a change is detected.<br/><br/>
        /// Example:<br/>
        /// <code>SettingsUtility.ChangeWatcher.AddHandler((name, isExternalChange) => { })</code>
        /// </summary>
        /// <remarks>
        /// Starts watcher if needed.<br/>
        /// Note that <see cref="Initialize(Backend, string?, string?, double, bool, bool)"/> must be called before this, if you need to do so explicitly.<br/>
        /// <br/>
        /// Note that when using registry as backend, name in <see cref="ChangeDetected"/> callback will be <see langword="null"/>, as it is not supported.
        /// </remarks>
        public static void AddHandler(ChangeDetected action)
        {

            if (action is null)
                throw new ArgumentNullException(nameof(action));

            callbacks.Add(action);

            if (!IsTracking)
                Reset();

        }

        /// <summary>Removes an handler that was to be called when a change would be detected.</summary>
        /// <remarks>Stops watcher if all handlers are removed.</remarks>
        public static void RemoveHandler(ChangeDetected action)
        {
            if (callbacks.Remove(action) && !HasCallbacks)
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
            fileWatcher.Changed += (s, e) => OnChangeDetected(GetName(e), isExternal: true);
            fileWatcher.Deleted += (s, e) => OnChangeDetected(GetName(e), isExternal: true);

        }

        static string GetName(FileSystemEventArgs e) =>
            System.IO.Path.GetFileNameWithoutExtension(e.FullPath);

        #endregion
        #region Registry

        static void DisableRegistry()
        {
            registryWatcher?.Dispose();
            registryWatcher = null;
        }

        static void EnableRegistry()
        {

            Initialize();
            if (Path is null) throw new InvalidOperationException("Path must be initialized.");

            registryWatcher = new(RegistryHive.CurrentUser, Path);
            registryWatcher.RegChanged += () => OnChangeDetected(null, isExternal: true);
            registryWatcher.IsEnabled = IsEnabled;

        }

        #endregion

        static void OnChangeDetected(string? name, bool isExternal)
        {
            foreach (var callback in callbacks)
                callback?.Invoke(name, isExternal);
        }

        internal static void NotifyInternalChange(string name) =>
            OnChangeDetected(name, isExternal: false);

    }

}
