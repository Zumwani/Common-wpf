using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility;

public static partial class Common
{

    #region IsVisible

    public static bool? GetIsVisible(UIElement obj) => (bool?)obj.GetValue(IsVisibleProperty);
    public static void SetIsVisible(UIElement obj, bool? value) => obj.SetValue(IsVisibleProperty, value);

    public static readonly DependencyProperty IsVisibleProperty =
        DependencyProperty.RegisterAttached("IsVisible", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsVisibleChanged));

    static async void OnIsVisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Collapsed;
        }

    }

    #endregion
    #region IsCollapsed

    public static bool? GetIsCollapsed(UIElement obj) => (bool?)obj.GetValue(IsCollapsedProperty);
    public static void SetIsCollapsed(UIElement obj, bool? value) => obj.SetValue(IsCollapsedProperty, value);

    public static readonly DependencyProperty IsCollapsedProperty =
        DependencyProperty.RegisterAttached("IsCollapsed", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsCollapsedChanged));

    static async void OnIsCollapsedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Collapsed;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }

    }

    #endregion
    #region IsHidden

    public static bool? GetIsHidden(UIElement obj) => (bool?)obj.GetValue(IsHiddenProperty);
    public static void SetIsHidden(UIElement obj, bool? value) => obj.SetValue(IsHiddenProperty, value);

    public static readonly DependencyProperty IsHiddenProperty =
        DependencyProperty.RegisterAttached("IsHidden", typeof(bool?), typeof(Common), new PropertyMetadata(null, OnIsHiddenChanged));

    static async void OnIsHiddenChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
    {

        if (sender is not UIElement element)
            return;

        if (e.NewValue is true)
        {
            await DoTransition(GetHideTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Hidden;
        }
        else if (e.NewValue is false)
        {
            await DoTransition(GetShowTransition(element), element as FrameworkElement);
            element.Visibility = Visibility.Visible;
        }

    }

    #endregion
    #region Transitions

    static Task DoTransition(Storyboard? storyboard, FrameworkElement? element)
    {

        if (storyboard is null || element is null)
            return Task.CompletedTask;

        var tcs = new TaskCompletionSource<bool>();
        storyboard.Completed += Storyboard_Completed;
        storyboard.RemoveRequested += Storyboard_Completed;
        storyboard.Begin(element);

        return tcs.Task;

        void Storyboard_Completed(object? sender, EventArgs e)
        {
            storyboard.Completed -= Storyboard_Completed;
            storyboard.RemoveRequested -= Storyboard_Completed;
            tcs.SetResult(true);
        }

    }

    public static Storyboard? GetShowTransition(UIElement obj) => (Storyboard?)obj.GetValue(ShowTransitionProperty);
    public static void SetShowTransition(UIElement obj, Storyboard? value) => obj.SetValue(ShowTransitionProperty, value);

    public static readonly DependencyProperty ShowTransitionProperty =
        DependencyProperty.RegisterAttached("ShowTransition", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    public static Storyboard? GetHideTransition(UIElement obj) => (Storyboard?)obj.GetValue(HideTransitionProperty);
    public static void SetHideTransition(UIElement obj, Storyboard? value) => obj.SetValue(HideTransitionProperty, value);

    public static readonly DependencyProperty HideTransitionProperty =
        DependencyProperty.RegisterAttached("HideTransition", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion

}

