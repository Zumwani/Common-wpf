using System;

namespace Common.Utility.xaml.x;

public class TrueExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => true;
}

public class FalseExtension : MarkupExtension
{
    public override object ProvideValue(IServiceProvider serviceProvider) => false;
}
