using Microsoft.Extensions.DependencyInjection;
using Mud.Network.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Player;
using System.Diagnostics;

namespace Mud.Server.TestApplication;

public class ConsoleNetworkServer : INetworkServer
{
    public event NewClientConnectedEventHandler? NewClientConnected;
    public event ClientDisconnectedEventHandler? ClientDisconnected;

    private ConsoleClient? _client;
    private bool _stopped;
    private IServiceProvider _serviceProvider;

    public ConsoleNetworkServer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IClient AddClient(string name, bool displayPlayerName, bool colorAccepted)
    {
        Debug.Assert(_client == null, "Client already added");
        ConsoleClient client = new (name)
        {
            DisplayPlayerName = displayPlayerName,
            ColorAccepted = colorAccepted
        };
        _client = client;
        NewClientConnected?.Invoke(client);
        return client;
    }

    public void Initialize()
    {
        _stopped = false;

    }

    public void Start()
    {
        while (!_stopped)
        {
            if (Console.KeyAvailable)
            {
                string? line;
                bool isEchoOff = _client != null && _client.IsEchoOff;
                if (isEchoOff)
                {
                    line = string.Empty;
                    while (true) // http://stackoverflow.com/questions/3404421/password-masking-console-application
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Enter)
                        {
                            Console.WriteLine();
                            break;
                        }
                        if (key.Key != ConsoleKey.Backspace)
                        {
                            Console.Write("*");
                            line += key.KeyChar;
                        }
                        else if (line.Length > 0)
                        {
                            line = line[..^1];
                            Console.Write("\b \b");
                        }
                    }
                }
                else
                    line = Console.ReadLine();
                if (line != null)
                {
                    // server commands
                    if (line.StartsWith('#'))
                    {
                        line = line.Replace("#", string.Empty).ToLower();
                        if (line == "quit")
                        {
                            _stopped = true;
                            ClientDisconnected?.Invoke(_client!);
                            break;
                        }
                        else if (line == "alist")
                        {
                            Console.WriteLine("Admins:");
                            foreach (IAdmin a in _serviceProvider.GetRequiredService<IAdminManager>().Admins)
                                Console.WriteLine(a.DisplayName + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
                        }
                        else if (line == "plist")
                        {
                            Console.WriteLine("players:");
                            foreach (IPlayer p in _serviceProvider.GetRequiredService<IPlayerManager>().Players)
                                Console.WriteLine(p.DisplayName + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
                        }
                        // TODO: characters/rooms/items
                    }
                    // client commands
                    else
                        _client?.OnDataReceived(line);
                }
            }
            else
                Thread.Sleep(100);
        }
    }

    public void Stop()
    {
        _stopped = true;
    }
}
