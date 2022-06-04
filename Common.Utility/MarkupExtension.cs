using System;

namespace Common.Utility;

public class MarkupExtension : System.Windows.Markup.MarkupExtension
{

    public override object ProvideValue(IServiceProvider serviceProvider) =>
        this;

}

