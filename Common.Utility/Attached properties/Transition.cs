using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility.AttachedProperties;

public static class Transition
{

    public static async Task DoTransition(Storyboard? storyboard, FrameworkElement? element)
    {
        if (storyboard is not null || element is not null)
            await storyboard.BeginAsync(element);
    }

    public static Task BeginAsync(this Storyboard? storyboard, FrameworkElement? element = null)
    {

        if (storyboard is null || element is not null)
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

    static readonly CubicEase ease = new();

    public static Storyboard FadeAnimation(double to, double durationInSeconds = 0.25)
    {
        var storyboard = new Storyboard();
        var animation = new DoubleAnimation { Duration = new(TimeSpan.FromSeconds(durationInSeconds)), To = to, EasingFunction = ease };
        Storyboard.SetTargetProperty(animation, new(nameof(UIElement.Opacity)));
        storyboard.Children.Add(animation);
        return storyboard;
    }

    #region Show

    public static Storyboard GetShowDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(1, durationInSeconds);

    public static bool GetUseDefaultShow(UIElement obj) => (bool)obj.GetValue(UseDefaultShowProperty);
    public static void SetUseDefaultShow(UIElement obj, bool value) => obj.SetValue(UseDefaultShowProperty, value);

    public static readonly DependencyProperty UseDefaultShowProperty = DependencyProperty.RegisterAttached("UseDefaultShow", typeof(bool), typeof(Common), new PropertyMetadata(true));

    public static Storyboard? GetShow(UIElement obj) => (Storyboard?)obj.GetValue(ShowProperty) ?? (GetUseDefaultShow(obj) ? GetShowDefault() : null);
    public static void SetShow(UIElement obj, Storyboard? value) => obj.SetValue(ShowProperty, value);

    public static readonly DependencyProperty ShowProperty = DependencyProperty.RegisterAttached("Show", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion
    #region Hide

    public static Storyboard GetHideDefault(double durationInSeconds = 0.25) =>
        FadeAnimation(0, durationInSeconds);

    public static bool GetUseDefaultHide(UIElement obj) => (bool)obj.GetValue(UseDefaultHideProperty);
    public static void SetUseDefaultHide(UIElement obj, bool value) => obj.SetValue(UseDefaultHideProperty, value);

    public static readonly DependencyProperty UseDefaultHideProperty = DependencyProperty.RegisterAttached("UseDefaultHide", typeof(bool), typeof(Common), new PropertyMetadata(true));

    public static Storyboard? GetHide(UIElement obj) => (Storyboard?)obj.GetValue(HideProperty) ?? (GetUseDefaultShow(obj) ? GetHideDefault() : null);
    public static void SetHide(UIElement obj, Storyboard? value) => obj.SetValue(HideProperty, value);

    public static readonly DependencyProperty HideProperty =
        DependencyProperty.RegisterAttached("Hide", typeof(Storyboard), typeof(Common), new PropertyMetadata(null));

    #endregion

}