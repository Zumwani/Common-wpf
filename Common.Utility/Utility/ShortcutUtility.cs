using System;
using System.IO;
using IWshRuntimeLibrary;

namespace Common.Utility;

/// <summary>Provides utility functions for creating shortcuts.</summary>
public static class ShortcutUtility
{

    /// <summary>Gets the path to the startmenu folder.</summary>
    public static string StartMenuPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs");

    /// <summary>Creates a .lnk shortcut at the specified path.</summary>
    public static void CreateShortcut(string path)
    {

        if (Directory.Exists(path))
            path += AppUtility.Info.Name + ".lnk";

        Directory.GetParent(path)?.Create();

        var shell = new WshShell();
        var shortcut = (IWshShortcut)shell.CreateShortcut(path);

        shortcut.Description = AppUtility.Info.Description;
        shortcut.TargetPath = Environment.ProcessPath;
        shortcut.Save();

    }

}