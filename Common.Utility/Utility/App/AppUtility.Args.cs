using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Common.Utility;

/// <summary>The arguments that an instance of this app was started with.</summary>
public class AppArguments
{

    public string[] Parameters { get; set; }
    public string AsString { get; set; }

    [JsonConstructor]
    public AppArguments(string[] parameters, string asString)
    {
        Parameters = parameters;
        AsString = asString;
    }

    public AppArguments(params string[] parameters)
    {
        Parameters = parameters;
        AsString = string.Join(" ", parameters);
    }

}

public static partial class AppUtility
{

    /// <summary>Occurs when a secondary instance is started.</summary>
    public static event ParseArguments? SecondaryInstanceStarted;
    public delegate void ParseArguments(AppArguments arguments);

    /// <summary>Gets the arguments that was used to open this instance.</summary>
    public static AppArguments GetArguments() =>
        new() { Parameters = Environment.GetCommandLineArgs().Skip(1).ToArray(), AsString = GetRawCommandLineArgs() };

    static string GetRawCommandLineArgs()
    {

        //https://stackoverflow.com/a/66242266

        // Separate the args from the exe path.. incl handling of dquote-delimited full/relative paths.
        var fullCommandLinePattern = new Regex(@"
            ^ #anchor match to start of string
                (?<exe> #capture the executable name; can be dquote-delimited or not
                    (\x22[^\x22]+\x22) #case: dquote-delimited
                    | #or
                    ([^\s]+) #case: no dquotes
                )
                \s* #chomp zero or more whitespace chars, after <exe>
                (?<args>.*) #capture the remainder of the command line
            $ #match all the way to end of string
            ",
            RegexOptions.IgnorePatternWhitespace |
            RegexOptions.ExplicitCapture |
            RegexOptions.CultureInvariant
        );

        var m = fullCommandLinePattern.Match(Environment.CommandLine);
        if (!m.Success) throw new ApplicationException("Failed to extract command line.");

        // Note: will return empty-string if no args after exe name.
        var commandLineArgs = m.Groups["args"].Value;
        return commandLineArgs;

    }

    static void ListenForArgs()
    {

        if (string.IsNullOrWhiteSpace(Info.PackageName))
            throw new ArgumentNullException("Package name cannot be null.");

        _ = Task.Run(() =>
        {

            using (var server = new NamedPipeServerStream(Info.PackageName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.None))
            {
                server.WaitForConnection();
                using var reader = new StreamReader(server);
                var args = JsonSerializer.Deserialize<AppArguments>(reader.ReadToEnd());
                dispatcher.Invoke(() => SecondaryInstanceStarted?.Invoke(args ?? new()));
            }

            ListenForArgs();

        });

    }

    static void SendArgs() =>
        SendArgsToPrimary(GetArguments());

    public static void SendArgsToPrimary(AppArguments args)
    {

        if (string.IsNullOrWhiteSpace(Info.PackageName))
            throw new InvalidOperationException("Package name cannot be null.");

        try
        {
            var client = new NamedPipeClientStream(".", Info.PackageName, PipeDirection.Out, PipeOptions.None);
            client.Connect(1000);
            using var writer = new StreamWriter(client);
            writer.Write(JsonSerializer.Serialize(args));
        }
        catch
        { }

    }

}
