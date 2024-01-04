using Common.Settings.Utility;

namespace Common.Settings.Internal;

/// <summary>Base class for singleton <see cref="Setting"/>.</summary>
public abstract class SingletonSetting<TSelf> : Setting where TSelf : SingletonSetting<TSelf>, new()
{

    /// <summary>Gets the current singleton for <see cref="TSelf"/>.</summary>
    public static TSelf Current { get; } = new TSelf().Setup(isSingleton: true);

    /// <summary>Creates a new <see cref="SingletonSetting{TSelf}"/>.</summary>
    public SingletonSetting() : base(Current)
    { }

    TSelf Setup(bool isSingleton)
    {

        IsSettingUp = true;

        if (isSingleton)
        {

            SettingsUtility.settings.Add(this);
            OnSetupSingleton();
            OnLoaded();
            Task.Delay(10).ContinueWith(t => Reload());

        }
        else
            OnSetupProxy();

        IsSettingUp = false;

        return (TSelf)this;

    }

    /// <summary>Set up your setting in here. This is were you're expected to read value from registry.</summary>
    /// <remarks>This makes sure we're only setting up the actual static singleton, and not the proxy bindings.</remarks>
    protected abstract void OnSetupSingleton();

    /// <summary>Set up your proxy to the singleton in here.</summary>
    /// <remarks>This makes sure we're only setting up the actual static singleton, and not the proxy bindings.</remarks>
    protected virtual void OnSetupProxy()
    { }

    /// <summary>Called when loaded.</summary>
    /// <remarks>Only called on the singleton instance.</remarks>
    protected virtual void OnLoaded()
    { }

}
