using ShellUtility.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;

[assembly: XmlnsDefinition("http://common", "Common.Utility")]
[assembly: XmlnsPrefix("http://common", "common")]
namespace Common.Utility
{

    public static class WindowExtensions
    {

        #region Clamp to screens

        public static bool GetClampToMonitors(DependencyObject obj) => (bool)obj.GetValue(ClampToMonitorsProperty);
        public static void SetClampToMonitors(DependencyObject obj, bool value) => obj.SetValue(ClampToMonitorsProperty, value);

        public static readonly DependencyProperty ClampToMonitorsProperty =
            DependencyProperty.RegisterAttached("ClampToMonitors", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(default(bool), OnClampToMonitorsChanged));

        static readonly Dictionary<IntPtr, Window> windows = new();
        static void OnClampToMonitorsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is not Window window)
                return;

            var handle = new WindowInteropHelper(window).EnsureHandle();
            var source = HwndSource.FromHwnd(handle);

            windows.Remove(handle);
            source.RemoveHook(WndProc);

            if ((bool)e.NewValue)
            {
                windows.Add(handle, window);
                source.AddHook(WndProc);
            }

        }

        static POINT? relativeMousePos;
        static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
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

                void ClampToScreen(Screen screen, ref bool handled)
                {

                    var width = rect.Right - rect.Left;
                    var height = rect.Bottom - rect.Top;

                    var preferredRect = new WIN32Rectangle() { Left = absoluteMousePos.X - relativeMousePos.Value.X, Top = absoluteMousePos.Y - relativeMousePos.Value.Y };
                    preferredRect.Right = preferredRect.Left + width;
                    preferredRect.Bottom = preferredRect.Top + height;

                    var isLeft = rect.Left < screen.Bounds.Left + relativeMousePos.Value.X;
                    var isRight = rect.Right > screen.Bounds.Right - relativeMousePos.Value.X;
                    var isTop = rect.Top < screen.Bounds.Top + relativeMousePos.Value.Y;
                    var isBottom = rect.Bottom > screen.Bounds.Bottom - relativeMousePos.Value.X;

                    if (isLeft || isRight || isTop || isBottom)
                    {

                        rect.Left = Math.Clamp(preferredRect.Left, screen.Bounds.Left, screen.Bounds.Right - width);
                        rect.Top = Math.Clamp(preferredRect.Top, screen.Bounds.Top, screen.Bounds.Bottom - height);

                        rect.Right = rect.Left + width;
                        rect.Bottom = rect.Top + height;

                        if (Screen.All().FirstOrDefault(s => s.Bounds.Contains(absoluteMousePos)) is Screen s && s.Handle != screen.Handle)
                            ClampToScreen(s, ref handled);

                        handled = true;

                    }

                }

                if (handled)
                    Marshal.StructureToPtr(rect, lParam, true);

            }

            return IntPtr.Zero;

        }

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
            WM_EXITSIZEMOVE = 0x0232
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

        static async void IsVisibleInAltTabChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is Window window)
            {

                var handle = await GetHandle(window);
                var exStyle = (ExtendedWindowStyles)GetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE);

                if ((bool)e.NewValue)
                    exStyle &= ~ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                else
                    exStyle |= ExtendedWindowStyles.WS_EX_TOOLWINDOW;

                SetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

            }

        }

        static async Task<IntPtr> GetHandle(Window window)
        {

            //During InitializeComponent() handle has not been created yet,
            //and we cannot call WindowInteropHelper.EnsureHandle() since this prevents setting AllowTransparency.
            //So we need to just wait until handle is created

            var interop = new WindowInteropHelper(window);
            while (interop.Handle == IntPtr.Zero)
                await Task.Delay(100);
            return interop.Handle;

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
