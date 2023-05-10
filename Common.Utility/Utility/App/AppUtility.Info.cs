using System;
using System.Diagnostics;

namespace Common.Utility;

public static partial class AppUtility
{

    /// <summary>Contains info about this app.</summary>
    public static class Info
    {

        static readonly FileVersionInfo FileVersionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);

        /// <summary>The publisher of this app.</summary>
        public static string Publisher => FileVersionInfo?.CompanyName ?? throw new InvalidOperationException();

        /// <summary>The name of this app.</summary>
        public static string Name => FileVersionInfo?.ProductName ?? throw new InvalidOperationException();

        /// <summary>The description of this app.</summary>
        public static string Description => FileVersionInfo?.FileDescription ?? throw new InvalidOperationException();

        /// <summary>
        /// <para>The package name of this app.</para>
        /// <para>This would be 'app.{Publisher}.{Name}'</para>
        /// </summary>
        public static string PackageName => $"app.{Publisher}.{Name}";

        /// <summary>The version of this app.</summary>
        public static string Version => FileVersionInfo?.ProductVersion ?? throw new InvalidOperationException();

        /// <summary>The executable path of this app.</summary>
        public static string ExecutablePath => Process.GetCurrentProcess().MainModule?.FileName ?? throw new InvalidOperationException();

    }

}

