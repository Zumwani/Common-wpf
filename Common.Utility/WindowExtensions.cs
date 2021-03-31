using ShellUtility.Screens;
using System;
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

        /// <summary>Makes sure that the window is visible on the users desktop.</summary>
        public static void MakeSureVisible(this Window window, Action<double> setLeft = null, Action<double> setTop = null)
        {

            if (window != null)
                return;

            setLeft ??= (value) => window.Left = value;
            setTop ??= (value) => window.Top = value;

            var pos = new System.Drawing.Rectangle((int)window.Left, (int)window.Top, (int)(window.Left + window.ActualWidth), (int)(window.Top + window.ActualHeight));

            if (!Screen.All().Any(s => s.Bounds.Contains(pos)))
            {

                var min = new Point(Screen.All().Min(s => s.WorkArea.Left), Screen.All().Min(s => s.WorkArea.Top));
                var max = new Point(Screen.All().Max(s => s.WorkArea.Right), Screen.All().Max(s => s.WorkArea.Bottom));

                if (window.Left < min.X)
                    setLeft?.Invoke(min.X);

                if (window.Left + window.ActualWidth > max.X)
                    setLeft?.Invoke(max.X - window.ActualWidth);

                if (window.Top < min.Y)
                    setTop?.Invoke(min.Y);

                if (window.Top + window.ActualHeight > max.Y)
                    setTop?.Invoke(max.Y - window.ActualHeight);

            }

        }

        #endregion
        #region Hide from alt-tab

        public static bool GetIsVisibleInAltTab(Window window) => (bool)window.GetValue(IsVisibleInAltTabProperty);
        public static void SetIsVisibleInAltTab(Window window, bool value) => window.SetValue(IsVisibleInAltTabProperty, value);

        public static readonly DependencyProperty IsVisibleInAltTabProperty =
            DependencyProperty.RegisterAttached("IsVisibleInAltTab", typeof(bool), typeof(WindowExtensions), new PropertyMetadata(false, IsVisibleInAltTabChanged));

        static void IsVisibleInAltTabChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {

            if (sender is Window window)
            {

                var handle = new WindowInteropHelper(window).EnsureHandle();
                var exStyle = (ExtendedWindowStyles)GetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE);

                if ((bool)e.NewValue)
                    exStyle &= ~ExtendedWindowStyles.WS_EX_TOOLWINDOW;
                else
                    exStyle |= ExtendedWindowStyles.WS_EX_TOOLWINDOW;

                SetWindowLong(handle, GetWindowLongFields.GWL_EXSTYLE, (IntPtr)exStyle);

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
