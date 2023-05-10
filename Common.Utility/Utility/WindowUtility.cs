using System;
using System.Reflection;
using System.Windows;
using System.Windows.Interop;
using ShellUtility.Screens;

namespace Common.Utility;

/// <summary>Provides utility functions for working with <see cref="Window"/>.</summary>
public static class WindowUtility
{

    /// <summary>Gets if this window is modal.</summary>
    public static bool IsModal(this Window window) =>
        (bool?)typeof(Window).GetField("_showingAsDialog", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(window) ?? false;

    /// <summary>Gets the win32 handle of this <see cref="Window"/>.</summary>
    public static IntPtr Handle(this Window window, bool ensureCreated = false) =>
        ensureCreated
        ? new WindowInteropHelper(window).EnsureHandle()
        : new WindowInteropHelper(window).Handle;

    #region Location

    /// <summary>Center the window on the users desktop.</summary>
    public static void Center(this Window window, Action<double>? setLeft = null, Action<double>? setTop = null, Screen? screen = null)
    {
        screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
        window?.CenterVertically(setTop, screen);
        window?.CenterHorizontally(setLeft, screen);
    }

    /// <summary>Center window vertically on the users desktop.</summary>
    public static void CenterVertically(this Window window, Action<double>? setTop = null, Screen? screen = null)
    {

        if (window is null)
            return;

        setTop ??= (value) => window.Top = value;
        screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
        setTop?.Invoke(screen.WorkArea.Y + screen.WorkArea.Height / 2 - window.ActualHeight / 2);

    }

    /// <summary>Center window horizontally on the users desktop.</summary>
    public static void CenterHorizontally(this Window window, Action<double>? setLeft = null, Screen? screen = null)
    {

        if (window is null)
            return;

        setLeft ??= (value) => window.Left = value;
        screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
        setLeft?.Invoke(screen.WorkArea.X + screen.WorkArea.Width / 2 - window.ActualWidth / 2);

    }

    #endregion

    /// <summary>Gets the position and size of this window.</summary>
    public static Rect GetRect(this Window window) =>
        xaml.NoNamespace.Common.GetRect(window);

    /// <summary>Sets the position and size of this window.</summary>
    public static void SetRect(this Window window, Rect rect) =>
        xaml.NoNamespace.Common.SetRect(window, rect);

}
