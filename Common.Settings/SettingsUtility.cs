using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;

namespace Common
{

    /// <summary>Contains functions for managing <see cref="ISetting"/>.</summary>
    public static class SettingsUtility
    {

        static SettingsUtility()
        {
            InitializedSettings = new ReadOnlyObservableCollection<ISetting>(list);
            if (Application.Current != null)
            {
                Application.Current.Exit += (s, e) => EnsureWritten();
                Application.Current.SessionEnding += (s, e) => EnsureWritten();
            }
        }

        /// <summary>
        /// <para>The name of your app, this is used for the key in the registry.</para>
        /// <para>By default, the name of <see cref="Assembly.GetEntryAssembly"/> is used. If custom one is defined, it must be set before any settings are initialized, since values will not be updated otherwise ().</para>
        /// </summary>
        public static string AppName { get; set; } = Assembly.GetEntryAssembly().GetName().Name;

        /// <summary>Gets registry key for this app.</summary>
        public static RegistryKey RegKey(bool writable = false) =>
            Registry.CurrentUser.CreateSubKey(@$"Software\{AppName}", writable: writable);

        internal static void Add(ISetting setting) =>
            list.Add(setting);

        static readonly ObservableCollection<ISetting> list = new ObservableCollection<ISetting>();

        /// <summary>
        /// <para>All initialized settings defined for this app.</para>
        /// <para>Note: Unless <see cref="Initialize"/> is called, settings are lazily loaded and will only be initialized once accessed for the first time, and will as a result not be added to this list until then.</para>
        /// </summary>
        public static ReadOnlyObservableCollection<ISetting> InitializedSettings { get; }

        /// <summary>
        /// <para>All settings defined for this app. Initializes all settings.</para>
        /// <para>Note: Convience property that can be easily be binded to, calls <see cref="Initialize"/> and returns <see cref="InitializedSettings"/>.</para>
        /// </summary>
        public static ReadOnlyObservableCollection<ISetting> AllSettings
        {
            get
            {
                Initialize();
                return InitializedSettings;
            }
        }

        /// <summary>Resets all settings defined for this app.</summary>
        public static void Reset(bool ignoreUninitializedSettings = false)
        {
            
            if (!ignoreUninitializedSettings)
                Initialize();

            foreach (var setting in InitializedSettings)
                setting.Reset();

        }

        /// <summary>
        /// <para>Initialize all settings defined for this app.</para>
        /// <para>Note: This is optional, settings are by default lazily loaded and only initialized once accessed for the first time. This method will initialize all uninitialized settings immediately.</para>
        /// </summary>
        public static void Initialize()
        {
            var settings = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes()).Where(t => typeof(ISetting).IsAssignableFrom(t));
            foreach (var setting in settings)
                setting.GetProperty("Current").GetValue(null);
        }

        /// <summary>
        /// <para>Ensures all settings have been writted to the registry.</para>
        /// <para>Values are not written to the registry immediately, so that we don't spam write when using bindings, the delay is 0.5 seconds. This method invokes write immediately on all pending writes.</para>
        /// </summary>
        public static void EnsureWritten()
        {
            foreach (var setting in InitializedSettings)
                if (setting.WriteTimer.IsEnabled)
                {
                    setting.WriteTimer.Stop();
                    setting.DoWrite();
                }    
        }

    }

}
