using Common.Settings.Internal;
using Microsoft.Win32;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using RectConverter = Common.Settings.Utility.RectJsonConverter;

namespace Common.Settings.Utility;

/// <summary>Provides utility methods relating to settings.</summary>
public static class SettingsUtility
{

    static string key = $"Software\\{Assembly.GetEntryAssembly()?.GetName()?.Name}";

    /// <summary>The registry key to store settings in. This needs to be set before any settings has been instantiated.</summary>
    public static string Key
    {
        get => key;
        set => key = settings.Count == 0 ? value : throw new InvalidOperationException("Cannot set key after any settings have been instantiated.");
    }

    /// <summary>The delay to use when using delaying a save. This is to prevent spam writes.</summary>
    public static TimeSpan DelayDuration { get; set; } = TimeSpan.FromSeconds(0.5);

    /// <summary>Determines whatever <see cref="JsonException"/> will be propagated or not when deserializing. If <see langword="false"/>, then <see langword="default"/> will be returned.</summary>
    public static bool ThrowOnDeserializationErrors { get; set; }

    /// <summary>Determines whatever <see cref="SavePending"/> should be automatically called on <see cref="Application.Exit"/>.</summary>
    public static bool PerformPendingWritesOnShutdown { get; set; } = true;

    static SettingsUtility()
    {
        SerializerOptions.Converters.Add(new RectConverter());
        if (Application.Current is Application app)
            app.Exit += (s, e) => { if (PerformPendingWritesOnShutdown) SavePending(); };
    }

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

            await Task.Delay((int)DelayDuration.TotalMilliseconds, CancellationToken.None);
            if (!token.IsCancellationRequested)
                Write(name, value);

        }

    }

    #endregion
    #region Write / Read

    static void Write<T>(string name, T? value)
    {
        _ = pending.Remove(name);
        using var key = Registry.CurrentUser.CreateSubKey(Key, writable: true);
        key.SetValue(name, JsonSerializer.Serialize(value, SerializerOptions));
    }

    internal static bool Read<T>(string name, [NotNullWhen(true)] out T? value)
    {

        value = default;

        using var key = Registry.CurrentUser.CreateSubKey(Key);
        var result = key?.GetValue(name);
        if (result is string json && !string.IsNullOrWhiteSpace(json))
        {
            try
            {
                value = JsonSerializer.Deserialize<T>(json, SerializerOptions);
                return value is not null;
            }
            catch (JsonException)
            {
                if (ThrowOnDeserializationErrors)
                    throw;
                return false;
            }
        }
        else
            return false;

    }

    #endregion
    #region Enumerate / Get

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

    /// <summary>Gets the raw json from the registry.</summary>
    /// <returns><see langword="true"/> when value existed</returns>
    public static bool GetJson(Setting setting, [NotNullWhen(true)] out string? json)
    {

        using var key = Registry.CurrentUser.CreateSubKey(Key);
        json = (string?)key?.GetValue(setting.Name, "");

        return !string.IsNullOrWhiteSpace(json);

    }

    /// <summary>Sets the raw json to the registry.</summary>
    public static void SetJson(Setting setting, string json)
    {
        _ = pending.Remove(setting.Name);
        using var key = Registry.CurrentUser.CreateSubKey(Key, writable: true);
        key.SetValue(setting.Name, json);
    }

    #endregion
    #region JsonConverters

    public static JsonSerializerOptions SerializerOptions { get; } = new();

    #endregion

}
