using ShellUtility.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://common", "Common.Utility")]
[assembly: XmlnsPrefix("http://common", "common")]
namespace Common.Utility
{

    public static class WindowExtensions
    {

        #region Is resizing

        public static bool? GetIsResizing(DependencyObject obj) => (bool?)obj.GetValue(IsResizingProperty);
        public static void SetIsResizing(DependencyObject obj, bool? value) => obj.SetValue(IsResizingProperty, value);

        public static readonly DependencyProperty IsResizingProperty =
            DependencyProperty.RegisterAttached("IsResizing", typeof(bool?), typeof(WindowExtensions), new PropertyMetadata(null, OnIsResizingChanged));

        static readonly Dictionary<IntPtr, Window> resizeWindows = new();

        static void OnIsResizingChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is not Window window)
                return;

            var handle = new WindowInteropHelper(window).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);

            window.Unloaded -= Window_Unloaded;
            source.RemoveHook(WndProc_IsResizing);
            resizeWindows.Remove(handle);

            if (((bool?)e.NewValue).HasValue)
            {
                window.Unloaded += Window_Unloaded;
                source.AddHook(WndProc_IsResizing);
                resizeWindows.Add(handle, window);
            }

            static void Window_Unloaded(object sender, RoutedEventArgs e)
            {
                if (sender is Window window)
                    SetIsResizing(window, null);
            }

        }

        static IntPtr WndProc_IsResizing(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == (int)WindowsMessage.WM_ENTERSIZEMOVE)
                SetIsResizing(resizeWindows[hwnd], true);
            else if (msg == (int)WindowsMessage.WM_EXITSIZEMOVE)
                SetIsResizing(resizeWindows[hwnd], false);

            return IntPtr.Zero;

        }

        #endregion
        #region Clamp to screens

        public static bool GetUseWorkAreaForMonitorClamping(DependencyObject obj) => (bool)obj.GetValue(UseWorkAreaForMonitorClampingProperty);
        public static void SetUseWorkAreaForMonitorClamping(DependencyObject obj, bool value) => obj.SetValue(UseWorkAreaForMonitorClampingProperty, value);

        public static bool GetClampToMonitors(DependencyObject obj) => (bool)obj.GetValue(ClampToMonitorsProperty);
        public static void SetClampToMonitors(DependencyObject obj, bool value) => obj.SetValue(ClampToMonitorsProperty, value);

        public static readonly DependencyProperty UseWorkAreaForMonitorClampingProperty =
            DependencyProperty.RegisterAttached("UseWorkAreaForMonitorClamping", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(false));

        public static readonly DependencyProperty ClampToMonitorsProperty =
            DependencyProperty.RegisterAttached("ClampToMonitors", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(default(bool), OnClampToMonitorsChanged));

        static readonly Dictionary<IntPtr, Window> clampedWindows = new();
        static void OnClampToMonitorsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is not Window window)
                return;

            window.Unloaded -= Window_Unloaded;
            window.Loaded -= UpdateClamp;
            window.LocationChanged -= UpdateClamp;
            window.SizeChanged -= UpdateClamp;

            var handle = new WindowInteropHelper(window).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);
            source.RemoveHook(WndProc_ClampToMonitors);
            _ = clampedWindows.Remove(handle);

            if ((bool)e.NewValue)
            {
                window.Unloaded += Window_Unloaded;
                window.LocationChanged += UpdateClamp;
                window.SizeChanged += UpdateClamp;
                window.Loaded += UpdateClamp;
                clampedWindows.Add(handle, window);
                source.AddHook(WndProc_ClampToMonitors);
                EnsureClamped(window);
            }

            void Window_Unloaded(object sender, RoutedEventArgs e) =>
                SetClampToMonitors(window, false);

            void UpdateClamp(object sender, EventArgs e)
            {
                if (window.IsLoaded)
                    EnsureClamped(window);
            }

        }

        /// <summary>Clamps the window to screen if needed.</summary>
        public static void EnsureClamped(this Window window)
        {

            var screen = Screen.FromWindowHandle(new WindowInteropHelper(window).Handle);
            var rect = GetUseWorkAreaForMonitorClamping(window) ? screen.WorkArea : screen.Bounds;

            //TODO: Does this override bindings?
            if (window.Left < rect.Left) window.Left = rect.Left;
            if (window.Top < rect.Top) window.Top = rect.Top;
            if (window.Left + window.ActualWidth > rect.Right) window.Width = rect.Right - window.Left;
            if (window.Top + window.ActualHeight > rect.Bottom) window.Height = rect.Bottom - window.Top;

        }

        static POINT? relativeMousePos;
        static IntPtr WndProc_ClampToMonitors(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            if (msg == (int)WindowsMessage.WM_EXITSIZEMOVE)
                relativeMousePos = null;
            else if (msg == (int)WindowsMessage.WM_MOVING)
            {

                _ = GetCursorPos(out var absoluteMousePos);
                var rect = Marshal.PtrToStructure<WIN32Rectangle>(lParam);
                if (!relativeMousePos.HasValue)
                    relativeMousePos = new POINT(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

                var screen = Screen.FromWindowHandle(hwnd);
                ClampToScreen(screen, ref handled);

                if (handled)
                    Marshal.StructureToPtr(rect, lParam, true);

                void ClampToScreen(Screen screen, ref bool handled)
                {

                    var bounds = GetUseWorkAreaForMonitorClamping(clampedWindows[hwnd]) ? screen.WorkArea : screen.Bounds;

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;

                    var preferredRect = new WIN32Rectangle() { Left = absoluteMousePos.X - relativeMousePos.Value.X, Top = absoluteMousePos.Y - relativeMousePos.Value.Y };
                    preferredRect.Right = preferredRect.Left + width;
                    preferredRect.Bottom = preferredRect.Top + height;

                    var isLeft = rect.Left < bounds.Left + relativeMousePos.Value.X;
                    var isRight = rect.Right > bounds.Right - relativeMousePos.Value.X;
                    var isTop = rect.Top < bounds.Top + relativeMousePos.Value.Y;
                    var isBottom = rect.Bottom > bounds.Bottom - relativeMousePos.Value.X;

                    if (isLeft || isRight || isTop || isBottom)
                    {

                        rect.Left = Math.Clamp(preferredRect.Left, bounds.Left, bounds.Right - width);
                        rect.Top = Math.Clamp(preferredRect.Top, bounds.Top, bounds.Bottom - height);

                        rect.Right = rect.Left + width;
                        rect.Bottom = rect.Top + height;

                        if (Screen.All().FirstOrDefault(s => s.Bounds.Contains(absoluteMousePos)) is Screen s && s.Handle != screen.Handle)
                            ClampToScreen(s, ref handled);

                        handled = true;

                    }

                }

            }
            else if (msg == (int)WindowsMessage.WM_SIZING)
            {

                _ = GetCursorPos(out var absoluteMousePos);
                var rect = Marshal.PtrToStructure<WIN32Rectangle>(lParam);
                if (!relativeMousePos.HasValue)
                    relativeMousePos = new POINT(absoluteMousePos.X - rect.Left, absoluteMousePos.Y - rect.Top);

                var screen = Screen.FromWindowHandle(hwnd);
                ClampToScreen(screen, ref handled);

                if (handled)
                    Marshal.StructureToPtr(rect, lParam, true);

                void ClampToScreen(Screen screen, ref bool handled)
                {

                    var bounds = GetUseWorkAreaForMonitorClamping(clampedWindows[hwnd]) ? screen.WorkArea : screen.Bounds;

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;

                    var preferredRect = new WIN32Rectangle() { Left = absoluteMousePos.X - relativeMousePos.Value.X, Top = absoluteMousePos.Y - relativeMousePos.Value.Y };
                    preferredRect.Right = preferredRect.Left + width;
                    preferredRect.Bottom = preferredRect.Top + height;

                    var isLeft = ((WindowEdge)wParam).IsLeft() && rect.Left < bounds.Left + relativeMousePos.Value.X;
                    var isRight = ((WindowEdge)wParam).IsRight() && rect.Right > bounds.Right - relativeMousePos.Value.X;
                    var isTop = ((WindowEdge)wParam).IsTop() && rect.Top < bounds.Top + relativeMousePos.Value.Y;
                    var isBottom = ((WindowEdge)wParam).IsBottom() && rect.Bottom > bounds.Bottom - relativeMousePos.Value.X;

                    if (isLeft || isRight || isTop || isBottom)
                    {

                        if (isLeft && rect.Left < bounds.Left)
                            rect.Left = bounds.Left;

                        if (isTop && rect.Top < bounds.Top)
                            rect.Top = bounds.Top;

                        if (isRight && rect.Right > bounds.Right)
                            rect.Right = bounds.Right;

                        if (isBottom && rect.Bottom > bounds.Bottom)
                            rect.Bottom = bounds.Bottom;

                        handled = true;

                    }

                }

            }

            return IntPtr.Zero;

        }

        static bool IsLeft(this WindowEdge edge) =>
            edge is WindowEdge.WMSZ_LEFT or WindowEdge.WMSZ_TOPLEFT or WindowEdge.WMSZ_BOTTOMLEFT;

        static bool IsTop(this WindowEdge edge) =>
            edge is WindowEdge.WMSZ_TOP or WindowEdge.WMSZ_TOPLEFT or WindowEdge.WMSZ_TOPRIGHT;

        static bool IsRight(this WindowEdge edge) =>
            edge is WindowEdge.WMSZ_RIGHT or WindowEdge.WMSZ_TOPRIGHT or WindowEdge.WMSZ_BOTTOMRIGHT;

        static bool IsBottom(this WindowEdge edge) =>
            edge is WindowEdge.WMSZ_BOTTOM or WindowEdge.WMSZ_BOTTOMLEFT or WindowEdge.WMSZ_BOTTOMRIGHT;

        [StructLayout(LayoutKind.Sequential)]
        public struct WIN32Rectangle
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {

            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p) => new System.Drawing.Point(p.X, p.Y);
            public static implicit operator POINT(System.Drawing.Point p) => new(p.X, p.Y);

        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetCursorPos(out POINT lpPoint);

        enum WindowsMessage
        {
            /// <summary>Sent after a window has been moved.</summary>
            WM_MOVE = 0x0003,
            /// <summary>
            /// Sent to a window when the size or position of the window is about to change.
            /// An application can use this message to override the window's default maximized size and position,
            /// or its default minimum or maximum tracking size.
            /// </summary>
            WM_GETMINMAXINFO = 0x0024,
            /// <summary>
            /// Sent to a window whose size, position, or place in the Z order is about to change as a result
            /// of a call to the SetWindowPos function or another window-management function.
            /// </summary>
            WM_WINDOWPOSCHANGING = 0x0046,
            /// <summary>
            /// Sent to a window whose size, position, or place in the Z order has changed as a result of a
            /// call to the SetWindowPos function or another window-management function.
            /// </summary>
            WM_WINDOWPOSCHANGED = 0x0047,
            /// <summary>
            /// Sent to a window that the user is moving. By processing this message, an application can monitor
            /// the position of the drag rectangle and, if needed, change its position.
            /// </summary>
            WM_MOVING = 0x0216,
            /// <summary>
            /// Sent once to a window after it enters the moving or sizing modal loop. The window enters the
            /// moving or sizing modal loop when the user clicks the window's title bar or sizing border, or
            /// when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the wParam
            /// parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is complete
            /// when DefWindowProc returns.
            /// <para />
            /// The system sends the WM_ENTERSIZEMOVE message regardless of whether the dragging of full windows
            /// is enabled.
            /// </summary>
            WM_ENTERSIZEMOVE = 0x0231,
            /// <summary>
            /// Sent once to a window once it has exited moving or sizing modal loop. The window enters the
            /// moving or sizing modal loop when the user clicks the window's title bar or sizing border, or
            /// when the window passes the WM_SYSCOMMAND message to the DefWindowProc function and the
            /// wParam parameter of the message specifies the SC_MOVE or SC_SIZE value. The operation is
            /// complete when DefWindowProc returns.
            /// </summary>
            WM_EXITSIZEMOVE = 0x0232,
            WM_SIZING = 0x0214
        }

        public enum WindowEdge
        {
            WMSZ_BOTTOM = 6,
            WMSZ_BOTTOMLEFT = 7,
            WMSZ_BOTTOMRIGHT = 8,
            WMSZ_LEFT = 1,
            WMSZ_RIGHT = 2,
            WMSZ_TOP = 3,
            WMSZ_TOPLEFT = 4,
            WMSZ_TOPRIGHT = 5,
        }

        #endregion
        #region Location

        /// <summary>Center the window on the users desktop.</summary>
        public static void Center(this Window window, Action<double> setLeft = null, Action<double> setTop = null, Screen screen = null)
        {
            screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
            window?.CenterVertically(setTop, screen);
            window?.CenterHorizontally(setLeft, screen);
        }

        /// <summary>Center window vertically on the users desktop.</summary>
        public static void CenterVertically(this Window window, Action<double> setTop = null, Screen screen = null)
        {

            if (window != null)
                return;

            setTop ??= (value) => window.Top = value;
            screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
            setTop?.Invoke(screen.WorkArea.Y + (screen.WorkArea.Height / 2) - (window.ActualHeight / 2));

        }

        /// <summary>Center window horizontally on the users desktop.</summary>
        public static void CenterHorizontally(this Window window, Action<double> setLeft = null, Screen screen = null)
        {

            if (window != null)
                return;

            setLeft ??= (value) => window.Left = value;
            screen ??= Screen.FromWindowHandle(new WindowInteropHelper(window).EnsureHandle());
            setLeft?.Invoke(screen.WorkArea.X + (screen.WorkArea.Width / 2) - (window.ActualWidth / 2));

        }

        #endregion
        #region Hide from alt-tab

        public static bool GetIsVisibleInAltTab(Window window) => (bool)window.GetValue(IsVisibleInAltTabProperty);
        public static void SetIsVisibleInAltTab(Window window, bool value) => window.SetValue(IsVisibleInAltTabProperty, value);

        public static readonly DependencyProperty IsVisibleInAltTabProperty =
            DependencyProperty.RegisterAttached("IsVisibleInAltTab", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(true, IsVisibleInAltTabChanged));

        static void IsVisibleInAltTabChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is Window window)
            {

                window.Loaded += Window_Loaded;

                void Window_Loaded(object _, RoutedEventArgs _1)
                {

                    window.Loaded -= Window_Loaded;

                    var handle = new WindowInteropHelper(window).Handle;
                    var exStyle = (ExtendedWindowStyles)GetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE);

                    if ((bool)e.NewValue)
                        exStyle &= ~ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                    else
                        exStyle |= ExtendedWindowStyles.WS_EX_TOOLWINDOW;

                    SetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

                }

            }

        }

        [Flags]
        enum ExtendedWindowStyles
        {
            WS_EX_TOOLWINDOW = 0x00000080,
        }

        enum GetWindowLongFields
        {
            GWL_EXSTYLE = (-20),
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetWindowLong(IntPtr hWnd, GetWindowLongFields nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowLong(IntPtr hWnd, GetWindowLongFields nIndex, IntPtr dwNewLong);

        [DllImport("kernel32.dll", EntryPoint = "SetLastError")]
        static extern void SetLastError(int dwErrorCode);

        #endregion

    }

}
