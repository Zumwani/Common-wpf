using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility;

public static class StoryboardUtility
{

    public static Task BeginAsync(this Storyboard? storyboard, FrameworkElement? element = null)
    {

        if (storyboard is null || element is null)
            return Task.FromResult(false);

        var tcs = new TaskCompletionSource<bool>();
        storyboard.Completed += Storyboard_Completed;
        storyboard.Begin(element);

        return tcs.Task;

        void Storyboard_Completed(object? sender, EventArgs e)
        {
            storyboard.Completed -= Storyboard_Completed;
            tcs.SetResult(true);
        }

    }

}