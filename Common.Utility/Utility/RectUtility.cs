using System.Windows;

namespace Common.Utility;

public static class RectUtility
{

    public static Rect ToRect(this System.Drawing.Rectangle rect) =>
        new(rect.Left, rect.Top, rect.Width, rect.Height);

}