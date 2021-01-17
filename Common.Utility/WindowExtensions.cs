using System;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using ShellUtility.Screens;

namespace Common.Utility
{

    public static class WindowExtensions
    {

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
            setTop  ??= (value) => window.Top = value;

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

    }

}
