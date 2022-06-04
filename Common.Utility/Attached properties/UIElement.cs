using System.Windows;

namespace Common.Utility;

public static partial class Common
{

    #region IsVisible

    public static bool? GetIsVisible(UIElement obj) => (bool?)obj.GetValue(IsVisibleProperty);
    public static void SetIsVisible(UIElement obj, bool? value) => obj.SetValue(IsVisibleProperty, value);

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached("IsVisible", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsVisibleChanged));

    static void OnIsVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
            element.Visibility = Visibility.Visible;
        else if (e.NewValue is false)
            element.Visibility = Visibility.Collapsed;

    }

    #endregion
    #region IsCollapsed

    public static bool? GetIsCollapsed(UIElement obj) => (bool?)obj.GetValue(IsCollapsedProperty);
    public static void SetIsCollapsed(UIElement obj, bool? value) => obj.SetValue(IsCollapsedProperty, value);

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.RegisterAttached("IsCollapsed", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsCollapsedChanged));

    static void OnIsCollapsedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
            element.Visibility = Visibility.Collapsed;
        else if (e.NewValue is false)
            element.Visibility = Visibility.Visible;

    }

    #endregion
    #region IsHidden

    public static bool? GetIsHidden(UIElement obj) => (bool?)obj.GetValue(IsHiddenProperty);
    public static void SetIsHidden(UIElement obj, bool? value) => obj.SetValue(IsHiddenProperty, value);

    public static readonly DependencyProperty IsHiddenProperty =
        DependencyProperty.RegisterAttached("IsHidden", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsHiddenChanged));

    static void OnIsHiddenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
            element.Visibility = Visibility.Hidden;
        else if (e.NewValue is false)
            element.Visibility = Visibility.Visible;

    }

    #endregion

}

