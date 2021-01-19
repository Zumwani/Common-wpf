﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.VersionIncrementer
{

    static class App
    {

        static void Main(string[] args)
        {

            try
            {

                var path = GetProjectFile(args);
                var text = File.ReadAllText(path);
                (string name, Version newVersion) = Increment(ref text);

                File.WriteAllText(path, text);
                Console.WriteLine(name + " incremented to: " + newVersion);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.GetType().Name + ":");
                Console.WriteLine(e.Message);
                //Console.Read();
            }

        }

        static string GetProjectFile(string[] args)
        {

            var projectFile = args.FirstOrDefault();

            if (!projectFile.Contains(":"))
                projectFile = Path.Combine(Directory.GetCurrentDirectory(), projectFile);

            if (!File.Exists(projectFile))
                throw new FileNotFoundException(message: null, fileName: projectFile);

            if (!projectFile.EndsWith(".csproj"))
                throw new ArgumentException("Invalid project file");

            return projectFile;

        }

        static (string name, Version newVersion) Increment(ref string text)
        {

            if (!(text.Contains("<Version>") && text.Contains("</Version>")))
                throw new ArgumentException("No version tag exists.");

            var match = Regex.Match(text, "<Version>(.*)</Version>");
            var currentVersion = new Version(match.Groups[1].ToString());
            var newVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build + 1);

            text = text.Replace(match.Value, $"<Version>{newVersion}</Version>");

            var nameMatch = Regex.Match(text, "<Product>(.*)</Product>");
            var name = nameMatch.Groups.Count == 2 ? nameMatch.Groups[1].ToString() : "Unknown";

            return (name, newVersion);

        }

    }

}
