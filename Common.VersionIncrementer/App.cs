using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.VersionIncrementer
{

    static class App
    {

        static void Main(string[] args)
        {

            (Mode mode, string projectFile, string versionFile) = ReadArgs(args);
            var versionText = File.ReadAllText(versionFile);
            var name = GetName(versionText);

            if (mode == Mode.Increment)
            {
                Increment(ref versionText, out var version);
                File.WriteAllText(version, versionText);
                Console.WriteLine(name + " version incremented to: " + version + ".");
            }
            else if (mode == Mode.Copy)
            {
                var projectText = File.ReadAllText(projectFile);
                CopyVersion(ref versionFile, ref projectText);
                File.WriteAllText(projectFile, projectText);
                Console.WriteLine(name + " project version updated.");
            }

        }

        enum Mode
        {
            Increment, Copy
        }

        static (Mode mode, string projectFile, string versionFile) ReadArgs(string[] args)
        {

            var modeStr = args.ElementAtOrDefault(0);
            var projectFile = args.ElementAtOrDefault(1);
            var versionFile = args.ElementAtOrDefault(2);

            Mode mode;

            if (modeStr == "-increment")
                mode = Mode.Increment;
            else if (modeStr == "-copy")
                mode = Mode.Copy;
            else
                throw new ArgumentException("Mode is not valid.");

            //Make relative paths absolute
            if (!projectFile.Contains(":"))
                projectFile = Path.Combine(Directory.GetCurrentDirectory(), projectFile);

            if (!versionFile.Contains(":"))
                versionFile = Path.Combine(Directory.GetCurrentDirectory(), versionFile);

            //Check if files exist
            if (!File.Exists(projectFile))
                throw new FileNotFoundException(message: null, fileName: projectFile);
            
            if (!File.Exists(versionFile))
                throw new FileNotFoundException(message: null, fileName: versionFile);

            return (mode, projectFile, versionFile);

        }

        const string VersionTag = "Version";
        const string ProductTag = "Product";

        static void CopyVersion(ref string versionText, ref string projectText)
        {
            GetTag(versionText, VersionTag, out var value, out var _);
            ChangeTagValue(ref projectText, out var _, VersionTag, (_1) => value);
        }

        static void Increment(ref string text, out string newVersion) =>
            ChangeTagValue(ref text, out newVersion, VersionTag, current => Version.Parse(current).BumpBuild().ToString());

        static void ChangeTagValue(ref string text, out string newValue, string tag, Func<string, string> setValue)
        {

            newValue = default;
            if (GetTag(text, tag, out var value, out var fullValue))
            {
                newValue = setValue.Invoke(value);
                text = text.Replace(fullValue, $"<{tag}>{newValue}</{tag}>");
            }

        }

        static string GetTagValue(string text, string tag)
        {
            GetTag(text, tag, out var value, out var _);
            return value;
        }

        static bool GetTag(string text, string tag, out string value, out string fullValue)
        {

            value = null;
            fullValue = null;

            if (!(text.Contains($"<{tag}>") && text.Contains($"</{tag}>")))
                return false;

            var match = Regex.Match(text, $"<{tag}>(.*)</{tag}>");
            if (match.Groups.Count > 1)
            {
                value = match.Groups[1].Value;
                fullValue = match.Value;
                return true;
            }
            else
                return false;

        }

        public static string GetName(string text) =>
            GetTagValue(text, ProductTag) ?? "Unknown";

        public static Version BumpBuild(this Version version) =>
            new Version(version.Major, version.Minor, version.Build + 1);

    }

}
