using System;
using System.Diagnostics;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Windows;

namespace Common.Utility;

public static partial class AppUtility
{

    /// <summary>Gets if this instance is primary.</summary>
    /// <param name="secondaryInstanceArgumentsHandler">Specifies a callback to handle app arguments, when a second instance is started.</param>
    /// <param name="handleIfPrimaryInstanceToo">Calls <paramref name="secondaryInstanceArgumentsHandler"/> immediately.</param>
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
    /// <param name="secondaryInstanceArgumentsHandler">Specifies a callback to handle app arguments, when a second instance is started.</param>
    /// <param name="handleIfPrimaryInstanceToo">Calls <paramref name="secondaryInstanceArgumentsHandler"/> immediately.</param>
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
    /// <param name="shutdownAction">The action to shutdown the app, defaults to <see cref="Application.Shutdown()"/>, but can be overriden if necessary.</param>
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

}
