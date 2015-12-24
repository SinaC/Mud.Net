using System;
using System.Configuration;
using Mud.Logger;

namespace Mud.Server.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            //TODO: int port = ConfigurationManager.AppSettings["port"]
            //CommandProcessor commandProcessor = new CommandProcessor();
            //SocketServer server = new SocketServer(() => new Client(commandProcessor));
            //server.Initialize(11000);
            //server.Start();
            //Console.WriteLine("Press ENTER to continue...");
            //Console.ReadLine();
            //server.Stop();

            CommandProcessor commandProcessor = new CommandProcessor();
            Client client = new Client(commandProcessor);
            //client.ProcessCommand("test");
            //client.ProcessCommand("test arg1");
            //client.ProcessCommand("test 'arg1' 'arg2' 'arg3' 'arg4'");
            //client.ProcessCommand("test 'arg1 arg2' 'arg3 arg4'");
            //client.ProcessCommand("test 'arg1 arg2\" arg3 arg4");
            //client.ProcessCommand("test 3.arg1");
            //client.ProcessCommand("test 2.'arg1'");
            //client.ProcessCommand("test 2'.arg1'");
            //client.ProcessCommand("test 2.'arg1 arg2' 3.arg3 5.arg4");
            //client.ProcessCommand("test 2."); // INVALID
            //client.ProcessCommand("test ."); // INVALID
            //client.ProcessCommand("test '2.arg1'");
            //client.ProcessCommand("unknown"); // INVALID
            //client.ProcessCommand("/test");
            //client.Impersonating = false; client.ProcessCommand("/shutdown");
            //client.Impersonating = true; client.ProcessCommand("/shutdown");
            //client.Impersonating = false; client.ProcessCommand("shutdown");
            //client.Impersonating = true; client.ProcessCommand("shutdown"); // INVALID
            //client.Impersonating = false; client.ProcessCommand("look");

            client.Impersonating = true; Log.Default.WriteLine(LogLevels.Debug, "IG:" + String.Join("|", client.CommandList()));
            client.Impersonating = false; Log.Default.WriteLine(LogLevels.Debug, "OOG:" + String.Join("|", client.CommandList()));

            //MysteryImporter importer = new MysteryImporter();
            //importer.Load(@"D:\GitHub\OldMud\area\midgaard.are");
            //importer.Parse();

            //string path = @"D:\GitHub\OldMud\area";
            //string fileList = Path.Combine(path, "area.lst");
            //string[] areaFilenames = File.ReadAllLines(fileList);
            //foreach (string areaFilename in areaFilenames)
            //{
            //    string areaFullName = Path.Combine(path, areaFilename);
            //    MysteryImporter importer = new MysteryImporter();
            //    importer.Load(areaFullName);
            //    importer.Parse();
            //}
        }
    }
}
