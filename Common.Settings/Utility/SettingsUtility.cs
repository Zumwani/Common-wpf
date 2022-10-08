using Common.Settings.Internal;
using Common.Settings.JsonConverters;
using Microsoft.Win32;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using path = System.IO.Path;

namespace Common.Settings.Utility;

//TODO: Add SettingsUtility.Reload()

//TODO: Update example

public enum Backend
{
    Registry, FileSystem
}

/// <summary>Provides utility methods relating to settings.</summary>
public static partial class SettingsUtility
{

    static SettingsUtility() =>
        EnableDefaultConverters();

    #region Settings and initialization

    /// <summary>Initializes Common.Settings.</summary>
    /// <param name="backend">Specifies the backend to use when storing settings.</param>
    /// <param name="path">The path to the registry key or folder on disk store the settings. Overrides <paramref name="packageName"/>.</param>
    /// <param name="packageName">Specifies the package name. Used to generate <paramref name="path"/>, if not explicitly specified.</param>
    /// <param name="writeDelayInSeconds">Specifies the delay before writing, to prevent spam-writes.</param>
    /// <param name="throwOnDeserializationErrors">Specifies whatever exceptions should be rethrown when a serialization error occurs. See <see cref="ThrowOnDeserializationErrors"/> for more info.</param>
    /// <param name="performPendingWritesOnAppShutdown">Specifies whatever <see cref="SettingsUtility.SavePending"/> should be automatically called on <see cref="Application.Exit"/></param>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    /// <remarks>
    /// Default options will be used if not explicitly initialized.<br/><br/>
    /// <see cref="InvalidOperationException"/> will be thrown if called after any settings has been loaded.
    /// </remarks>
    public static void Initialize(Backend backend, string? packageName = null, string? path = null, double writeDelayInSeconds = 0.5, bool throwOnDeserializationErrors = false, bool performPendingWritesOnAppShutdown = true)
    {

        if (settings.Count > 0)
            throw new InvalidOperationException("Initialization can only occur before any settings has been loaded.");

        Backend = backend;
        PackageName = packageName ?? PackageName;
        Path = path;
        WriteDelay = TimeSpan.FromSeconds(writeDelayInSeconds);
        ThrowOnDeserializationErrors = throwOnDeserializationErrors;
        PerformPendingWritesOnAppShutdown = performPendingWritesOnAppShutdown;

        Initialize();

    }

    static void Initialize()
    {

        if (Path is not null)
            return;

        if (string.IsNullOrWhiteSpace(PackageName))
            throw new InvalidOperationException("PackageName cannot be null when auto generating path.");

        Path =
            Backend is Backend.Registry
            ? "Software\\" + PackageName
            : path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), PackageName);

        if (Application.Current is Application app && PerformPendingWritesOnAppShutdown)
            app.Exit += (s, e) => SavePending();

    }

    /// <summary>Specifies the backend to use when storing settings.</summary>
    /// <remarks>Default is <see cref="Backend.Registry"/>.</remarks>
    public static Backend Backend { get; private set; }

    /// <summary>Specifies the package name.</summary>
    /// <remarks>
    /// Used to generate <see cref="Path"/> when it is <see langword="null"/> or empty.<br/><br/>
    /// Produces the following:<br/>
    /// <see cref="Backend.Registry"/>: HKEY_Current_User\Software\[PackageName]<br/>
    /// <see cref="Backend.FileSystem"/>: %AppData%\[PackageName]
    /// <br/><br/>
    /// Default is: <code>Assembly.GetEntryAssembly().GetName().Name.</code>
    /// </remarks>
    public static string PackageName { get; private set; } =
        Assembly.GetEntryAssembly()?.GetName()?.Name ??
        throw new InvalidOperationException("Assembly.GetEntryAssembly()?.GetName()?.Name returned null.");

    /// <summary>The path to the registry key or folder on disk store the settings.</summary>
    public static string? Path { get; private set; }

    /// <summary>Specifies the delay before writing, to prevent spam-writes.</summary>
    /// <remarks>Default is 0.5 seconds.</remarks>
    public static TimeSpan WriteDelay { get; private set; } = TimeSpan.FromSeconds(0.5);

    /// <summary>Specifies whatever exceptions should be rethrown when a serialization error occurs.</summary>
    /// <remarks>
    /// Settings will be automatically recreated on serialization if <see langword="false"/>, when error occurs.<br/>
    /// Default is <see langword="false"/>.
    /// </remarks>
    public static bool ThrowOnDeserializationErrors { get; private set; }

    /// <summary>Specifies whatever <see cref="SettingsUtility.SavePending"/> should be automatically called on <see cref="Application.Exit"/>.</summary>
    /// <remarks>Default is <see langword="true"/>.</remarks>
    public static bool PerformPendingWritesOnAppShutdown { get; private set; } = true;

    #endregion
    #region Save

    /// <summary>Saves the setting.</summary>
    /// <param name="delay">Determines whatever a delay should be used before writing, this prevent spam writes.</param>
    public static void Save(this Setting setting, bool delay = true)
    {

        if (setting.IsSettingUp)
            return;

        setting.OnBeforeSave();
        Save(setting.Name, setting.GetValueInternal(out var value) ? value : null, delay);

    }

    /// <summary>Sets the value to null.</summary>
    public static void Remove(this Setting setting, bool delay = true) =>
        Save(setting.Name, null, delay);

    /// <summary>Saves <paramref name="value"/> to the registry, using <paramref name="key"/> as value name.</summary>
    /// <param name="delay">Determines whatever a delay should be used before writing, this prevent spam writes.</param>
    public static void Save(string key, object? value, bool delay = true)
    {
        if (delay)
            Delay(key, value);
        else
            Write(key, value);
    }

    /// <summary>Cancels all pending save requests.</summary>
    public static void CancelPendingSaves()
    {
        foreach (var token in pending)
            token.Value.token.Cancel();
        pending.Clear();
    }

    /// <summary>Cancels delay for all pending writes and writes them immediately.</summary>
    public static void SavePending()
    {
        foreach (var setting in pending)
            Save(setting.Key, setting.Value.value, delay: false);
    }

    /// <summary>Saves all settings.</summary>
    public static void Save(bool delay = true)
    {
        foreach (var setting in Enumerate())
            setting.Save(delay);
    }

    #endregion
    #region Delay

    static readonly Dictionary<string, (CancellationTokenSource token, object? value)> pending = new();
    static void Delay(string name, object? value)
    {

        if (pending.Remove(name, out var token))
            token.token.Cancel();

        var t = new CancellationTokenSource();
        pending.Add(name, (t, value));
        _ = Task.Run(() => Delay(t.Token));

        async void Delay(CancellationToken token)
        {
            await Task.Delay(WriteDelay, CancellationToken.None);
            if (!token.IsCancellationRequested)
                Write(name, value);
        }

    }

    #endregion
    #region Write / Read

    static async void Write<T>(string name, T? value)
    {

        Initialize();
        if (Path is null) throw new InvalidOperationException("Path must be initialized.");

        _ = pending.Remove(name);

        try
        {

            ChangeWatcher.IsEnabled = false;

            var json = value as string ?? JsonSerializer.Serialize(value, SerializerOptions);

            if (Backend is Backend.Registry)
            {
                using var key = Registry.CurrentUser.CreateSubKey(Path, writable: true);
                key.SetValue(name, json);
            }
            else if (Backend is Backend.FileSystem)
            {

                _ = Directory.CreateDirectory(Path);
                var file = path.Combine(Path, name + ".json");

                File.WriteAllText(file, json);

            }

        }
        catch (JsonException)
        {
            if (ThrowOnDeserializationErrors)
                throw;
        }
        finally
        {

            //Lets wait so that fileystem watcher has actually detected the changes
            if (Backend is Backend.FileSystem)
                await Task.Delay(500);
            ChangeWatcher.IsEnabled = true;
            ChangeWatcher.NotifyInternalChange();

        }

    }

    /// <summary>Reads value directly from registry.</summary>
    public static bool Read<T>(string name, [NotNullWhen(true)] out T? value)
    {

        Initialize();
        if (Path is null) throw new InvalidOperationException("Path must be initialized.");

        value = default;

        try
        {

            string? json = null;
            if (Backend is Backend.Registry)
            {
                using var key = Registry.CurrentUser.CreateSubKey(Path);
                json = key?.GetValue(name) as string;
            }
            else if (Backend is Backend.FileSystem)
            {
                var file = path.Combine(Path, name + ".json");
                if (File.Exists(file))
                    json = File.ReadAllText(file);
            }

            if (typeof(T) == typeof(string))
                value = (T?)(object?)json;
            else if (!string.IsNullOrWhiteSpace(json))
                value = JsonSerializer.Deserialize<T>(json, SerializerOptions);

        }
        catch (JsonException)
        {
            if (ThrowOnDeserializationErrors)
                throw;
        }

        return value is not null;

    }

    #endregion
    #region Enumerate

    internal static readonly List<Setting> settings = new();

    /// <summary>Enumerates all instantiated settings.</summary>
    public static IEnumerable<Setting> Enumerate() =>
        settings;

    #endregion
    #region Get / Set raw

    /// <summary>Gets the raw value of a <see cref="Setting"/>.</summary>
    public static bool GetRawValue(Setting setting, [NotNullWhen(true)] out object? value) =>
        setting.GetValueInternal(out value);

    /// <summary>Sets the value of a <see cref="Setting"/> directly.</summary>
    /// <returns><see langword="true"/> if type was compatible and was set.</returns>
    public static bool SetRawValue(Setting setting, object? value) =>
        setting.SetRawInternal(value);

    /// <summary>Gets the raw json.</summary>
    /// <returns><see langword="true"/> when value existed</returns>
    public static bool GetJson(Setting setting, [NotNullWhen(true)] out string? json) =>
        GetJson(setting.Name, out json);

    /// <summary>Gets the raw json.</summary>
    /// <returns><see langword="true"/> when value existed</returns>
    public static bool GetJson(string name, [NotNullWhen(true)] out string? json)
    {

        Initialize();
        if (Path is null) throw new InvalidOperationException("Path must be initialized.");

        json = null;
        if (Backend is Backend.Registry)
        {
            using var key = Registry.CurrentUser.OpenSubKey(Path);
            json = key?.GetValue(name, "") as string;
        }
        else if (Backend is Backend.FileSystem)
        {
            var file = path.Combine(Path, name + ".json");
            if (File.Exists(file))
                json = File.ReadAllText(file);
        }

        return !string.IsNullOrWhiteSpace(json);

    }

    /// <summary>Serializes the setting and returns the json string.</summary>
    public static bool Serialize(Setting setting, [NotNullWhen(true)] out string? json)
    {

        json = setting.GetValueInternal(out var obj)
        ? JsonSerializer.Serialize(obj, SerializerOptions)
        : null;

        return json is not null;

    }

    /// <summary>Sets the raw json to the registry.</summary>
    public static void SetJson(Setting setting, string json) =>
        Write(setting.Name, json);

    #endregion
    #region JsonConverters

    /// <summary>The <see cref="JsonSerializerOptions"/> to use when serializing.</summary>
    public static JsonSerializerOptions SerializerOptions { get; } = new();

    /// <summary>Gets if a default converter is enabled.</summary>
    public static bool IsDefaultConverterEnabled<T>() where T : JsonConverter, new() =>
        SerializerOptions.Converters.OfType<T>().Any();

    /// <summary>Enables a default converter.</summary>
    /// <param name="isEnabled">Determines whatever the converter should be enabled. Setting to <see langword="false"/> has same effect as <see cref="DisableDefaultConverter{T}"/>.</param>
    public static void EnableDefaultConverter<T>(bool isEnabled = true) where T : JsonConverter, new()
    {
        _ = SerializerOptions.Converters.Remove(SerializerOptions.GetConverter(typeof(T)));
        if (isEnabled)
            SerializerOptions.Converters.Add(new T());
    }

    /// <summary>Disables a default converter.</summary>
    public static void DisableDefaultConverter<T>() where T : JsonConverter, new() =>
        EnableDefaultConverter<T>(isEnabled: false);

    static void EnableDefaultConverters()
    {

        EnableDefaultConverter<RectJsonConverter.NonNullable>();
        EnableDefaultConverter<BitmapSourceJsonConverter.NonNullable>();
        EnableDefaultConverter<IntPtrJsonConverter.NonNullable>();

        EnableDefaultConverter<RectJsonConverter>();
        EnableDefaultConverter<BitmapSourceJsonConverter>();
        EnableDefaultConverter<IntPtrJsonConverter>();

    }

    #endregion

    /// <summary>Opens <see cref="Path"/> in explorer or regedit, depending on <see cref="Backend"/>.</summary>
    public static void OpenBackendLocation()
    {

        Initialize();
        if (Path is null) throw new InvalidOperationException("Path must be initialized.");

        if (Backend is Backend.Registry)
        {
            using var key = Registry.CurrentUser.CreateSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Applets\\Regedit", writable: true);
            key.SetValue("LastKey", "computer\\HKEY_CURRENT_USER\\" + Path);
            _ = Process.Start(new ProcessStartInfo("regedit") { UseShellExecute = true });
        }
        else
        {
            _ = Directory.CreateDirectory(Path);
            _ = Process.Start(new ProcessStartInfo(Path) { UseShellExecute = true });
        }

    }

}
