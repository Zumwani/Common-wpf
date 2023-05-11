using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Animation;

namespace Common.Utility;

/// <summary>Provides utility functions relating to <see cref="Storyboard"/>.</summary>
public static class StoryboardUtility
{

    /// <summary>Begins and <see langword="await"/> this <see cref="Storyboard"/>.</summary>
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