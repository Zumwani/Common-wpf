using Microsoft.Win32;

namespace Common.Utility;

public static partial class AppUtility
{

    /// <summary>An helper class for enabling or disabling auto start.</summary>
    public static AutoStartHelper AutoStart { get; } = new();

    public class AutoStartHelper : NotifyProperty<bool>
    {

        protected override bool GetValue() =>
            (string?)Key?.GetValue(Info.PackageName) == Info.ExecutablePath.Quotify() + " " + param;

        protected override void OnSetValue(bool value, bool notify)
        {
            if (value)
                Key?.SetValue(Info.PackageName, Info.ExecutablePath.Quotify() + " " + param);
            else
                Key?.DeleteValue(Info.PackageName, throwOnMissingValue: false);
        }

        static RegistryKey? Key =>
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        /// <summary>Enable auto start.</summary>
        /// <param name="parameter">Sets <see cref="Parameter"/> before enabling.</param>
        public void Enable(string? parameter = null)
        {
            param = parameter;
            Value = true;
        }

        /// <summary>Disable auto start.</summary>
        public void Disable() => Value = false;

        string? param;
        public string? Parameter
        {
            get => param;
            set { param = value; if (Value) Enable(); }
        }

    }

}

