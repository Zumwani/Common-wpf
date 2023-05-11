using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Common.Utility;

/// <summary>The arguments that an instance of this app was started with.</summary>
public class AppArguments
{

    /// <summary>Gets the arguments as an array.</summary>
    public string[] Parameters { get; }

    /// <summary>Gets the arguments as a string.</summary>
    [JsonIgnore] public string AsString { get; }

    /// <summary>Creates a new <see cref="AppArguments"/>.</summary>
    [JsonConstructor]
    public AppArguments(params string[] parameters)
    {
        AsString = string.Join(" ", parameters);
        Parameters = parameters;
    }

}

public static partial class AppUtility
{

    /// <summary>Occurs when a secondary instance is started.</summary>
    public static event ParseArguments? SecondaryInstanceStarted;

    /// <summary>Occurs when a secondary instance is started.</summary>
    public delegate void ParseArguments(AppArguments arguments);

    /// <summary>Gets the arguments that was used to open this instance.</summary>
    public static AppArguments GetArguments() =>
        new(Environment.GetCommandLineArgs().Skip(1).ToArray());

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

    /// <summary>Sends args from a secondary instance to first.</summary>
    /// <exception cref="InvalidOperationException"></exception>
    public static void SendArgsToPrimary(AppArguments args)
    {

        if (IsPrimaryInstance())
            throw new InvalidOperationException("Cannot send args to primary from the primary instance.");

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
