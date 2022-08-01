using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Common.Utility;

[Serializable]
/// <summary>The arguments that an instance of this app was started with.</summary>
public class AppArguments
{

    public string[] Parameters { get; set; }
    public string AsString { get; set; }

    public AppArguments(string[] parameters, string asString)
    {
        Parameters = parameters;
        AsString = asString;
    }

    public AppArguments(params string[] parameters)
    {
        Parameters = parameters;
        AsString = string.Join(" ", parameters);
    }

}

/// <summary>Contains utility functions relating to <see cref="Application"/>.</summary>
public static class AppUtility
{

    static readonly Dispatcher dispatcher;
    static AppUtility() =>
        dispatcher = Dispatcher.CurrentDispatcher;

    #region Info

    /// <summary>Contains info about this app.</summary>
    public static class Info
    {

        static readonly FileVersionInfo FileVersionInfo = FileVersionInfo.GetVersionInfo(ExecutablePath);

        /// <summary>The publisher of this app.</summary>
        public static string Publisher => FileVersionInfo?.CompanyName ?? throw new InvalidOperationException();

        /// <summary>The name of this app.</summary>
        public static string Name => FileVersionInfo?.ProductName ?? throw new InvalidOperationException();

        /// <summary>
        /// <para>The package name of this app.</para>
        /// <para>This would be 'app.{Publisher}.{Name}'</para>
        /// </summary>
        public static string PackageName => $"app.{Publisher}.{Name}";

        /// <summary>The version of this app.</summary>
        public static string Version => FileVersionInfo?.ProductVersion ?? throw new InvalidOperationException();

        /// <summary>The executable path of this app.</summary>
        public static string ExecutablePath => Assembly.GetEntryAssembly()?.Location?.Replace(".dll", ".exe") ?? throw new InvalidOperationException();

    }

    #endregion
    #region Autostart

    /// <summary>An helper class for enabling or disabling auto start.</summary>
    public static AutoStartHelper AutoStart { get; } = new();

    public class AutoStartHelper : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>Gets if auto start is enabled.</summary>
        public bool IsEnabled
        {
            get => (string?)Key?.GetValue(Info.PackageName) == Info.ExecutablePath.Quotify();
            set
            {
                if (value)
                    Key?.SetValue(Info.PackageName, Info.ExecutablePath.Quotify());
                else
                    Key?.DeleteValue(Info.PackageName, throwOnMissingValue: false);
                PropertyChanged?.Invoke(this, new(nameof(IsEnabled)));
            }
        }

        static RegistryKey? Key =>
            Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

        /// <summary>Enable auto start.</summary>
        public void Enable() => IsEnabled = true;

        /// <summary>Disable auto start.</summary>
        public void Disable() => IsEnabled = false;

    }

    #endregion
    #region Single instance

    /// <summary>Gets if this instance is primary.</summary>
    /// <param name="mutexName">The name of the mutex, only has an effect if mutex is not already setup.</param>
    public static bool IsPrimaryInstance(ParseArguments? secondaryInstanceArgumentsHandler = null, bool handleIfPrimaryInstanceToo = true, string? mutexName = null)
    {

        if (mutex is null)
            SetupMutex(mutexName);

        if (!mutexIsOwnedByUs)
            return false;

        if (secondaryInstanceArgumentsHandler is not null)
        {
            SecondaryInstanceStarted += secondaryInstanceArgumentsHandler;
            if (handleIfPrimaryInstanceToo)
                secondaryInstanceArgumentsHandler?.Invoke(GetArguments());
        }

        return true;

    }

    /// <summary>Gets if this instance is secondary.</summary>
    /// <param name="mutexName">The name of the mutex, only has an effect if mutex is not already setup.</param>
    public static bool IsSecondaryInstance(ParseArguments? secondaryInstanceArgumentsHandler = null, bool handleIfPrimaryInstanceToo = true, string? mutexName = null)
    {

        if (mutex is null)
            SetupMutex(mutexName);

        if (!mutexIsOwnedByUs)
            return true;

        if (secondaryInstanceArgumentsHandler is not null)
        {
            SecondaryInstanceStarted += secondaryInstanceArgumentsHandler;
            if (handleIfPrimaryInstanceToo)
                secondaryInstanceArgumentsHandler?.Invoke(GetArguments());
        }

        return false;

    }

    /// <summary>Releases mutex (single instance token) and restarts the app.</summary>
    /// <param name="asAdmin">Adds 'runas' verb when restarting.</param>
    /// <param name="shutdownAction">The action to shutdown the app, defaults to <see cref="Application.Shutdown"/>, but can be overriden if necessary.</param>
    public static void Restart(bool asAdmin = false, Action? shutdownAction = null)
    {

        Release();
        _ = Process.Start(new ProcessStartInfo(Info.ExecutablePath) { Verb = asAdmin ? "runas" : "", UseShellExecute = true });

        (shutdownAction ?? Shutdown).Invoke();
        static void Shutdown() => Application.Current?.Shutdown();

    }

    /// <summary>Releases the mutex, allowing other instances to become primary.</summary>
    public static void Release()
    {
        if (mutexIsOwnedByUs)
            mutex?.ReleaseMutex();
        mutexIsOwnedByUs = false;
    }

    static Mutex? mutex;
    static bool mutexIsOwnedByUs;
    static void SetupMutex(string? mutexName = null)
    {

        mutexName ??= Info.PackageName;

        if (AppUtility.mutex is not null)
            return;

        //Modified version of the following stackoverflow answers:
        //Sam Saffron: https://stackoverflow.com/a/229567
        //Wouter: https://stackoverflow.com/a/59079638

        var mutexId = $"Global\\{mutexName}";

        // initiallyOwned: true == false + mutex.WaitOne()
        var mutex = new Mutex(initiallyOwned: true, mutexId, out var mutexCreated);

        if (mutexCreated)
        {

            ListenForArgs();

            //Multi-user support
            var allowEveryoneRule = new MutexAccessRule(
                new SecurityIdentifier(WellKnownSidType.WorldSid, null),
                MutexRights.FullControl,
                AccessControlType.Allow);
            var securitySettings = new MutexSecurity();
            securitySettings.AddAccessRule(allowEveryoneRule);

            mutex.SetAccessControl(securitySettings);

        }
        else
            SendArgs();

        AppUtility.mutex = mutex;
        mutexIsOwnedByUs = mutexCreated;

    }

    #endregion
    #region Args

    /// <summary>Occurs when a secondary instance is started.</summary>
    public static event ParseArguments? SecondaryInstanceStarted;
    public delegate void ParseArguments(AppArguments arguments);

    /// <summary>Gets the arguments that was used to open this instance.</summary>
    public static AppArguments GetArguments() =>
        new() { Parameters = Environment.GetCommandLineArgs().Skip(1).ToArray(), AsString = GetRawCommandLineArgs() };

    static string GetRawCommandLineArgs()
    {

        //https://stackoverflow.com/a/66242266

        // Separate the args from the exe path.. incl handling of dquote-delimited full/relative paths.
        var fullCommandLinePattern = new Regex(@"
            ^ #anchor match to start of string
                (?<exe> #capture the executable name; can be dquote-delimited or not
                    (\x22[^\x22]+\x22) #case: dquote-delimited
                    | #or
                    ([^\s]+) #case: no dquotes
                )
                \s* #chomp zero or more whitespace chars, after <exe>
                (?<args>.*) #capture the remainder of the command line
            $ #match all the way to end of string
            ",
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.ExplicitCapture |
            RegexOptions.CultureInvariant
        );

        var m = fullCommandLinePattern.Match(Environment.CommandLine);
        if (!m.Success) throw new ApplicationException("Failed to extract command line.");

        // Note: will return empty-string if no args after exe name.
        var commandLineArgs = m.Groups["args"].Value;
        return commandLineArgs;

    }

    static void ListenForArgs()
    {

        if (string.IsNullOrWhiteSpace(Info.PackageName))
            throw new ArgumentNullException("Package name cannot be null.");

        _ = Task.Run(() =>
          {

              using (var server = new NamedPipeServerStream(Info.PackageName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None))
              {
                  server.WaitForConnection();
                  using var reader = new StreamReader(server);
                  var args = JsonSerializer.Deserialize<AppArguments>(reader.ReadToEnd());
                  dispatcher.Invoke(() => SecondaryInstanceStarted?.Invoke(args ?? new()));
              }

              ListenForArgs();

          });

    }

    static void SendArgs()
    {

        if (string.IsNullOrWhiteSpace(Info.PackageName))
            throw new ArgumentNullException("Package name cannot be null.");

        try
        {
            var client = new NamedPipeClientStream(".", Info.PackageName, PipeDirection.Out, PipeOptions.None);
            client.Connect(1000);
            using var writer = new StreamWriter(client);
            var sd = GetArguments();
            writer.Write(JsonSerializer.Serialize(GetArguments()));
        }
        catch
        { }

    }

    #endregion
    #region Pack uri scheme

    const string PackScheme = "pack";
    public static void RegisterPackUriScheme()
    {
        if (!UriParser.IsKnownScheme(PackScheme))
            UriParser.Register(new GenericUriParser(GenericUriParserOptions.GenericAuthority), PackScheme, -1);
    }

    #endregion
    #region UAC

    /// <summary>Gets if this instance is elevated.</summary>
    public static bool IsElevated { get; } = GetIsElevated();

    static bool GetIsElevated()
    {
        using var identity = WindowsIdentity.GetCurrent();
        var principal = new WindowsPrincipal(identity);
        return principal.IsInRole(WindowsBuiltInRole.Administrator);
    }

    #endregion

}

