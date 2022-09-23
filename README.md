# Common-wpf
A collection of useful libraries for wpf.

All utilities are available as NuGet packages:
> [Install-Package Common-wpf.Settings](https://www.nuget.org/packages/Common-wpf.Settings/)\
> [Install-Package Common-wpf.Utility](https://www.nuget.org/packages/Common-wpf.Utility/)

## Common.Settings

A settings system for wpf that allows easy access for both code behind and xaml, with auto-complete and two way bindings.

```csharp

namespace Example.Settings; //namespace we need to import in xaml (xmlns:settings="clr-namespace:Example.Settings")

public class WindowTitle : Setting<string, WindowTitle>
{
    public override string? DefaultValue => "Example";
}

public class ExampleCollection : CollectionSetting<string, Collection>
{
    public override IEnumerable<string>? DefaultItems => new[] { "" };
}

public class ExampleDictionary : DictionarySetting<string, string, Dictionary>
{
    public override Dictionary<string, string>? DefaultItems => new() { { "testKey", "testValue" } };
}

public class ExampleFlags : FlagSetting<string, Flag>
{
    public override Dictionary<string, bool>? DefaultItems => null; //null is default, so no reason to override, but for example purposes
}

```

```xaml
<Window
  ..
  xmlns:settings="clr-namespace:Example.Settings"
  xmlns:settingsUtility="common://settings"
  Width="800" Height="450" WindowStartupLocation="Manual"
  Title="{settings:WindowTitle Mode=OneWay}"
  Settings.SavePosition="True"><!--Attached property for saving window pos-->
  ..
</Window>
```

```csharp
public Window : System.Windows.Window
{

    private void Window_Loaded(..)
    {

        //Set some new values through code
        Settings.WindowTitle.Current.Value = "This is an example";
        Settings.ExampleCollection.Current.Add("test");
        Settings.ExampleDictionary.Current.Set("test", "value");
        Settings.ExampleFlags.Current.Set("test");
        Settings.ExampleFlags.Current.Unset("test");

        //We delay the actual write for a bit to ensure we don't spam write value to registry.
        //While delay duration can be modified using Common.Settings.SettingsUtility.DelayDuration property,
        //the following call ensures all pending writes are done at once.
        //This is by automatically called during App.Current.Exit event, by default.
        Common.Settings.SettingsUtility.SavePending(); 

        //List all settings and values
        foreach (var setting in Common.Settings.SettingsUtility.Enumerate())
            if (Common.Settings.SettingsUtility.GetJson(setting, out var json))
                Debug.WriteLine($"{setting.Name} ({setting.Name}):\n{json}\n");

    }

}

```

## Common.Utility
Contains utility functions for wpf.

```xml
<Window ..
  xmlns:common="http://common"
  Common:IsVisibleInAltTab="False">

</Window>
```
```csharp
public class Window : System.Windows.Window
{

    private void Window_Loaded(..)
    {

        //Center window on the screen that the window is currently on
        this.Center();
        this.CenterVertically();
        this.CenterHorizontally();

        //Restricts window from being moved offscreen (supports multiple monitors)
        this.MakeSureVisible();

    }
}

```
```csharp
public class App : System.Windows.Application
{

    void Application_Startup(object sender, StartupEventArgs e)
    {
    
        //Makes sure app runs as single instance
        if (AppUtility.IsSecondaryInstance(HandleArguments))
        {
            Shutdown();
            return;
        }
        
        //Enable auto start, can be bound to using two-way binding
        //{Binding Source={x:Static common:AppUtility.AutoStart}, Path=IsEnabled, Mode=TwoWay}
        AppUtility.AutoStart.IsEnabled = true;

    }
    
    //Handle command line arguements here, which are passed from secondary instance
    void HandleArguments(AppArguments arguments)
    { }
    
}

```
