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

    public Icon Icon { get; set; }

    public ImageSourceFromIconExtension()
    { }

    public ImageSourceFromIconExtension(Icon icon) =>
        Icon = icon;

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        Imaging.CreateBitmapSourceFromHIcon(Icon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

}