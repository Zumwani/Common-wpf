using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Common.VersionIncrementer
{

    static class App
    {

        static void Main(string[] args)
        {

            var path = GetProjectFile(args);
            var text = File.ReadAllText(path);

            Increment(ref text, out var newVersion);

            File.WriteAllText(path, text);

            var name = GetName(text);
            Console.WriteLine(name + " version incremented to: " + newVersion);

        }

        static string GetProjectFile(string[] args)
        {

            var projectFile = args.FirstOrDefault();

            if (!projectFile.Contains(":"))
                projectFile = Path.Combine(Directory.GetCurrentDirectory(), projectFile);

            if (!File.Exists(projectFile))
                throw new FileNotFoundException(message: null, fileName: projectFile);

            if (!(projectFile.EndsWith(".csproj") || projectFile.EndsWith(".props")))
                throw new ArgumentException("Invalid project file");

            return projectFile;

        }

        static void Increment(ref string text, out string newVersion) =>
            ChangeTagValue(ref text, out newVersion, "Version", current => Version.Parse(current).BumpBuild().ToString());

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
            GetTagValue(text, "Product") ?? "Unknown";

        public static Version BumpBuild(this Version version) =>
            new Version(version.Major, version.Minor, version.Build + 1);

    }

}
