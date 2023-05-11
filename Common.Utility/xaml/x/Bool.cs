using System;

namespace Common.Utility.xaml.x;

/// <summary>An extension to easily specify <see langword="true"/> in xaml. Usage: {x:True}.</summary>
public class TrueExtension : MarkupExtension
{
    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => true;
}

/// <summary>An extension to easily specify <see langword="false"/> in xaml. Usage: {x:False}.</summary>
public class FalseExtension : MarkupExtension
{
    /// <inheritdoc/>
    public override object ProvideValue(IServiceProvider serviceProvider) => false;
}
