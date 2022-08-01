using System;
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media.Imaging;

namespace Common.Utility;

[ContentProperty("Icon")]
public class ImageSourceFromIconExtension : MarkupExtension
{

    public Icon? Icon { get; set; }

    public ImageSourceFromIconExtension()
    { }

    public ImageSourceFromIconExtension(Icon icon) =>
        Icon = icon;

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).
    public override object? ProvideValue(IServiceProvider serviceProvider) =>
        Icon?.Handle is not null && Icon.Handle != IntPtr.Zero
        ? Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions())
        : null;
#pragma warning restore CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).

}