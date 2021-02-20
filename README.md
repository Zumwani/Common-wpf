# Common-wpf
A collection of useful libraries for wpf.

All utilities are available as NuGet packages:
> [Install-Package Common-wpf.Settings](https://www.nuget.org/packages/Common-wpf.Settings/)\
> [Install-Package Common-wpf.Utility](https://www.nuget.org/packages/Common-wpf.Utility/)

## Common.Settings

A settings system for wpf that allows easy access for both code behind and xaml, with auto-complete and two way bindings.

```csharp
namespace Example.Settings
{

    public class WindowLeft : Setting<double, WindowLeft>
    { }

    public class WindowTop : Setting<double, WindowTop>
    { }

    public class WindowWidth : Setting<double, WindowWidth>
    {
        public override double DefaultValue => 1000;
    }

    public class WindowHeight : Setting<double, WindowHeight>
    {
        public override double DefaultValue => 570;
    }

    public class WindowTitle : Setting<string, WindowTitle>
    {

        public override string DefaultValue => "Example";

        //Also available:
        //public override void Clamp(ref double value) { }
        //public override string DisplayName => base.DisplayName;
        //public override bool Validate(double value) => true;

    }

}
```

```xaml
<Window
  ..
  xmlns:settings="clr-namespace:Example.Settings"
  WindowStartupLocation="Manual"
  Left="{settings:WindowLeft}" Top="{settings:WindowTop}"
  Width="{settings:WindowWidth}" Height="{settings:WindowHeight}"
  Title="{settings:WindowTitle Mode=OneWay}">
  ..
</Window>
```

```csharp
public Window : System.Windows.Window
{

    private void Window_Loaded(..)
    {

      //Center window if first time launching...
      if (Settings.WindowLeft.Current.IsDefault)
          this.CenterHorizontally(SetLeft, screen);

      if (Settings.WindowTop.Current.IsDefault)
          this.CenterVertically(SetTop, screen);

      //Access single setting
      Settings.WindowTitle.Current.Value = "This is an example";

      //Access list of all settings
      foreach (var setting in Common.SettingsUtility.AllSettings)
          Debug.WriteLine(setting.DisplayName ?? setting.Key + ": " + setting.Value?.ToString());

    }

}

```

## Common.Utility
Contains utility functions for wpf.

> static class WindowUtility\
Contains extension methods for centering windows and also a function to clamp window position to bounds of screen(s).
