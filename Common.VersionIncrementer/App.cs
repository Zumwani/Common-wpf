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

            try
            {

                var (path, isSetHasChanged) = GetProjectFile(args);
                var text = File.ReadAllText(path);

                string name = "";
                Version newVersion = null;
                if (isSetHasChanged)
                    Increment(ref text, out name, out newVersion);
                else
                    SetHasChanged(ref text, out name, true);

                File.WriteAllText(path, text);

                if (isSetHasChanged)
                    Console.WriteLine(name + "HasChanged changed to: true");
                else
                    Console.WriteLine(name + " incremented to: " + newVersion);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name + ":");
                Console.WriteLine(e.Message);
                //Console.Read();
            }

        }

        static (string path, bool isSetChanged) GetProjectFile(string[] args)
        {

            var projectFile = args.FirstOrDefault();

            if (!projectFile.Contains(":"))
                projectFile = Path.Combine(Directory.GetCurrentDirectory(), projectFile);

            if (!File.Exists(projectFile))
                throw new FileNotFoundException(message: null, fileName: projectFile);

            if (!projectFile.EndsWith(".csproj"))
                throw new ArgumentException("Invalid project file");

            var isSetChanged = args.Length > 1 && args[1] == "-setHasChanged";

            return (projectFile, isSetChanged);

        }

        static string Hash(ref string text, string newValue = "")
        {
            if (!string.IsNullOrEmpty(newValue))
                ChangeTagValue(ref text, out newValue, "Hash", h => newValue);
            return newValue;
        }

        static void Increment(ref string text, out Version newVersion) =>
            ChangeTagValue(ref text, out newVersion, "Version", current => current.BumpBuild());

        static void ChangeTagValue<T>(ref string text, out T newValue, string tag, Func<T, T> setValue)
        {

            newValue = default;
            if (GetTag(text, tag, out var value, out var fullValue))
            {
                newValue = (T)Convert.ChangeType(setValue.Invoke((T)Convert.ChangeType(value, typeof(T))), typeof(T));
                text = text.Replace(fullValue, $"<{tag}>{newValue}</{tag}>");
            }

        }

        static string GetTagValue(string text, string tag)
        {
            GetTag(text, tag, out var value, out var _);
            return value;
        }

        static bool GetTag(string text, string tag, out string value) =>
            GetTag(text, tag, out value, out var _);

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

        public static string GetHash(string projectFile)
        {

        }

    }

}
