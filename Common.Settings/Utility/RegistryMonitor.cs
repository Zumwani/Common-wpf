using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Common.Settings.Utility;

/// <summary>
/// Filter for notifications reported by <see cref="RegistryMonitor"/>.
/// </summary>
[Flags]
internal enum RegChangeNotifyFilter
{
    /// <summary>Notify the caller if a subkey is added or deleted.</summary>
    Key = 1,
    /// <summary>Notify the caller of changes to the attributes of the key, such as the security descriptor information.</summary>
    Attribute = 2,
    /// <summary>Notify the caller of changes to a value of the key. This can include adding or deleting a value, or changing an existing value.</summary>
    Value = 4,
    /// <summary>Notify the caller of changes to the security descriptor of the key.</summary>
    Security = 8,
}

/// <summary><see cref="RegistryMonitor"/> allows you to monitor specific registry key.</summary>
/// <remarks>
/// If a monitored registry key changes, an event is fired. You can subscribe to these
/// events by adding a delegate to <see cref="RegChanged"/>.
/// <para>The Windows API provides a function
/// <a href="http://msdn.microsoft.com/library/en-us/sysinfo/base/regnotifychangekeyvalue.asp">
/// RegNotifyChangeKeyValue</a>, which is not covered by the
/// <see cref="Microsoft.Win32.RegistryKey"/> class. <see cref="RegistryMonitor"/> imports
/// that function and encapsulates it in a convenient manner.
/// </para>
/// </remarks>
/// <example>
/// This sample shows how to monitor <c>HKEY_CURRENT_USER\Environment</c> for changes:
/// <code>
/// public class MonitorSample
/// {
///     static void Main() 
///     {
///         RegistryMonitor monitor = new RegistryMonitor(RegistryHive.CurrentUser, "Environment");
///         monitor.RegChanged += new EventHandler(OnRegChanged);
///         monitor.Start();
///
///         while(true);
/// 
///			monitor.Stop();
///     }
///
///     private void OnRegChanged(object sender, EventArgs e)
///     {
///         Console.WriteLine("registry key has changed");
///     }
/// }
/// </code>
/// </example>
internal class RegistryMonitor : IDisposable
{

    #region Pinvoke

    [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    static extern int RegOpenKeyEx(IntPtr hKey, string subKey, uint options, int samDesired, out IntPtr phkResult);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern int RegNotifyChangeKeyValue(IntPtr hKey, bool bWatchSubtree, RegChangeNotifyFilter dwNotifyFilter, SafeWaitHandle hEvent, bool fAsynchronous);

    [DllImport("advapi32.dll", SetLastError = true)]
    static extern int RegCloseKey(IntPtr hKey);

    const int KEY_QUERY_VALUE = 0x0001;
    const int KEY_NOTIFY = 0x0010;
    const int STANDARD_RIGHTS_READ = 0x00020000;

    static readonly IntPtr HKEY_CLASSES_ROOT = new(unchecked((int)0x80000000));
    static readonly IntPtr HKEY_CURRENT_USER = new(unchecked((int)0x80000001));
    static readonly IntPtr HKEY_LOCAL_MACHINE = new(unchecked((int)0x80000002));
    static readonly IntPtr HKEY_USERS = new(unchecked((int)0x80000003));
    static readonly IntPtr HKEY_PERFORMANCE_DATA = new(unchecked((int)0x80000004));
    static readonly IntPtr HKEY_CURRENT_CONFIG = new(unchecked((int)0x80000005));

    #endregion
    #region IDisposable

    public void Dispose()
    {
        Stop();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    #endregion
    #region Constructors

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    /// <summary>Initializes a new instance of the <see cref="RegistryMonitor"/> class.</summary>
    /// <param name="key">The registry key to monitor.</param>
    public RegistryMonitor(RegistryKey key) : this(key.Name)
    { }

    /// <summary>Initializes a new instance of the <see cref="RegistryMonitor"/> class.</summary>
    /// <param name="name">The name.</param>
    public RegistryMonitor(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));
        InitRegistryKey(name);
    }

    /// <summary>Initializes a new instance of the <see cref="RegistryMonitor"/> class.</summary>
    /// <param name="registryHive">The registry hive.</param>
    /// <param name="subKey">The sub key.</param>
    public RegistryMonitor(RegistryHive registryHive, string subKey) =>
        InitRegistryKey(registryHive, subKey);

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

    void InitRegistryKey(RegistryHive hive, string name)
    {

        registrySubName = name;
        registryHive = hive switch
        {
            RegistryHive.ClassesRoot => HKEY_CLASSES_ROOT,
            RegistryHive.CurrentConfig => HKEY_CURRENT_CONFIG,
            RegistryHive.CurrentUser => HKEY_CURRENT_USER,
            RegistryHive.LocalMachine => HKEY_LOCAL_MACHINE,
            RegistryHive.PerformanceData => HKEY_PERFORMANCE_DATA,
            RegistryHive.Users => HKEY_USERS,
            _ => throw new InvalidEnumArgumentException(nameof(hive), (int)hive, typeof(RegistryHive)),
        };

    }

    void InitRegistryKey(string name)
    {

        var nameParts = name.Split('\\');

        switch (nameParts[0])
        {
            case "HKEY_CLASSES_ROOT":
            case "HKCR":
                registryHive = HKEY_CLASSES_ROOT;
                break;

            case "HKEY_CURRENT_USER":
            case "HKCU":
                registryHive = HKEY_CURRENT_USER;
                break;

            case "HKEY_LOCAL_MACHINE":
            case "HKLM":
                registryHive = HKEY_LOCAL_MACHINE;
                break;

            case "HKEY_USERS":
                registryHive = HKEY_USERS;
                break;

            case "HKEY_CURRENT_CONFIG":
                registryHive = HKEY_CURRENT_CONFIG;
                break;

            default:
                registryHive = IntPtr.Zero;
                throw new ArgumentException("The registry hive '" + nameParts[0] + "' is not supported", nameof(name));
        }

        registrySubName = String.Join("\\", nameParts, 1, nameParts.Length - 1);

    }

    #endregion

    IntPtr registryHive;
    string registrySubName;
    readonly object threadLock = new();
    Thread? thread;
    bool _disposed = false;
    readonly ManualResetEvent _eventTerminate = new(false);

    /// <summary>Occurs when the specified registry key has changed.</summary>
    public event Action? RegChanged;

    /// <summary>Occurs when the access to the registry fails.</summary>
    public event ErrorEventHandler? Error;

    /// <summary>Raises the <see cref="RegChanged"/> event.</summary>
    /// <remarks>
    /// <p>
    /// <b>OnRegChanged</b> is called when the specified registry key has changed.
    /// </p>
    /// <note type="inheritinfo">
    /// When overriding <see cref="OnRegChanged"/> in a derived class, be sure to call
    /// the base class's <see cref="OnRegChanged"/> method.
    /// </note>
    /// </remarks>
    protected virtual void OnRegChanged() =>
        RegChanged?.Invoke();

    /// <summary>Raises the <see cref="Error"/> event.</summary>
    /// <param name="e">The <see cref="Exception"/> which occured while watching the registry.</param>
    /// <remarks>
    /// <p><b>OnError</b> is called when an exception occurs while watching the registry.</p>
    /// <note type="inheritinfo">
    /// When overriding <see cref="OnError"/> in a derived class, be sure to call
    /// the base class's <see cref="OnError"/> method.
    /// </note>
    /// </remarks>
    protected virtual void OnError(Exception e) =>
        Error?.Invoke(this, new ErrorEventArgs(e));

    /// <summary><see langword="true"/> if this <see cref="RegistryMonitor"/> object is currently monitoring, otherwise, <see langword="false"/>.</summary>
    public bool IsEnabled
    {
        get => thread is not null;
        set { if (value) Start(); else Stop(); }
    }

    /// <summary>Start monitoring.</summary>
    void Start()
    {

        if (_disposed)
            throw new ObjectDisposedException(null, "This instance is already disposed");

        lock (threadLock)
        {
            if (!IsEnabled)
            {
                _ = _eventTerminate.Reset();
                thread = new Thread(new ThreadStart(MonitorThread)) { IsBackground = true };
                thread.Start();
            }
        }

    }

    /// <summary>Stops the monitoring thread.</summary>
    void Stop()
    {

        if (_disposed)
            throw new ObjectDisposedException(null, "This instance is already disposed");

        lock (threadLock)
        {
            if (thread is not null)
            {
                _ = _eventTerminate.Set();
                thread?.Join();
            }
        }

    }

    void MonitorThread()
    {

        try
        {
            ThreadLoop();
        }
        catch (Exception e)
        {
            OnError(e);
        }

        thread = null;

    }

    void ThreadLoop()
    {

        if (RegOpenKeyEx(registryHive, registrySubName, 0, STANDARD_RIGHTS_READ | KEY_QUERY_VALUE | KEY_NOTIFY, out var registryKey) != 0)
            throw new Win32Exception();

        try
        {

            var _eventNotify = new AutoResetEvent(false);
            var waitHandles = new WaitHandle[] { _eventNotify, _eventTerminate };

            while (!_eventTerminate.WaitOne(0, true))
            {

                if (RegNotifyChangeKeyValue(registryKey, true, RegChangeNotifyFilter.Value, _eventNotify.SafeWaitHandle, true) != 0)
                    throw new Win32Exception();

                if (WaitHandle.WaitAny(waitHandles) == 0)
                    OnRegChanged();

            }

        }
        finally
        {
            if (registryKey != IntPtr.Zero)
                _ = RegCloseKey(registryKey);
        }

    }
}
