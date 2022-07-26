using System.Windows;

namespace Common.Settings.Utility.AttachedProperties;

/// <summary>Attached properties accessible from 'Common' xaml namespace.</summary>
public static partial class Common
{

    #region Save Position

    public static bool GetSavePosition(Window window) => (bool)window.GetValue(SavePositionProperty);
    public static void SetSavePosition(Window window, bool value) => window.SetValue(SavePositionProperty, value);

    public static readonly DependencyProperty SavePositionProperty =
        DependencyProperty.RegisterAttached("SavePosition", typeof(bool), typeof(Common), new PropertyMetadata(false, OnSavePositionChanged));

    static readonly List<Type> list = new();

    static void OnSavePositionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not Window window)
            return;

        var name = window.GetType().FullName ?? throw new NullReferenceException("Window.GetType().Fullname is null.");

        window.LocationChanged -= Window_LocationChanged;
        window.SizeChanged -= Window_SizeChanged;
        window.Closed -= Window_Closed;

        if (list.Contains(window.GetType()) && (bool)e.NewValue)
            throw new InvalidOperationException("Cannot register multiple instances of same window type.");

        if ((bool)e.NewValue)
        {
            list.Add(window.GetType());
            Load(window, name);
            window.LocationChanged += Window_LocationChanged;
            window.SizeChanged += Window_SizeChanged;
            window.Closed += Window_Closed;
        }
        else
            _ = list.Remove(window.GetType());

        void Window_SizeChanged(object sender, SizeChangedEventArgs e) =>
            Save(window, name);

        void Window_LocationChanged(object? sender, EventArgs e) =>
            Save(window, name);

        void Window_Closed(object? sender, EventArgs e)
        {
            _ = list.Remove(window.GetType());
            window.LocationChanged -= Window_LocationChanged;
            window.SizeChanged -= Window_SizeChanged;
            window.Closed -= Window_Closed;
            Save(window, name, delay: false);
        }

    }

    static async void Load(Window window, string name)
    {

        if (SettingsUtility.Read<Rect>(name, out var rect))
        {

            window.Left = rect.Left;
            window.Top = rect.Top;
            window.Width = rect.Width;
            window.Height = rect.Height;
            window.Loaded += Window_Loaded;

            void Window_Loaded(object sender, RoutedEventArgs e)
            {
                window.Loaded -= Window_Loaded;
                window.Width = rect.Width;
                window.Height = rect.Height;
            }

        }
        else
        {

            while ((double.IsNaN(window.Width) && window.ActualWidth == 0) || (double.IsNaN(window.Height) && window.ActualHeight == 0))
                await Task.Delay(100);

            var w = window.ActualWidth != 0 ? window.ActualWidth : window.Width;
            var h = window.ActualHeight != 0 ? window.ActualHeight : window.Height;

            window.Left = (SystemParameters.PrimaryScreenWidth / 2) - (w / 2);
            window.Top = (SystemParameters.PrimaryScreenHeight / 2) - (h / 2);

        }

    }

    static void Save(Window window, string name, bool delay = true)
    {
        if (window.IsLoaded)
            SettingsUtility.Save(name, new Rect(window.Left, window.Top, window.ActualWidth, window.ActualHeight), delay);
    }

    #endregion

}
