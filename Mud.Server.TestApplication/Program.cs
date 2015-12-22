using System;
using System.Configuration;
using Mud.Logger;
using Mud.Network;

namespace Mud.Server.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            //TODO: int port = ConfigurationManager.AppSettings["port"]
            CommandProcessor commandProcessor = new CommandProcessor();
            SocketServer server = new SocketServer(() => new Client(commandProcessor));
            server.Initialize(11000);
            server.Start();

            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();

            server.Stop();
        }
    }
}
