using System.IO;

namespace Common.Utility.App;

public class CreateShortcutInStartmenuCommand : Command
{

    public override void Execute() =>
        ShortcutUtility.CreateShortcut(Path.Combine(ShortcutUtility.StartMenuPath, AppUtility.Info.Publisher));

}