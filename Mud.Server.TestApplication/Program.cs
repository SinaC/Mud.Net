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
            //SocketServer server = new SocketServer(() => new Player(commandProcessor));
            //server.Initialize(11000);
            //server.Start();
            //Console.WriteLine("Press ENTER to continue...");
            //Console.ReadLine();
            //server.Stop();

            //CommandProcessor commandProcessor = new CommandProcessor();
            //DummyEntity entity = new DummyEntity(commandProcessor);
            //Player player = new Player(commandProcessor);
            ////player.ProcessCommand("testoog");
            ////player.ProcessCommand("testoog arg1");
            ////player.ProcessCommand("testoog 'arg1' 'arg2' 'arg3' 'arg4'");
            ////player.ProcessCommand("testoog 'arg1 arg2' 'arg3 arg4'");
            ////player.ProcessCommand("testoog 'arg1 arg2\" arg3 arg4");
            ////player.ProcessCommand("testoog 3.arg1");
            ////player.ProcessCommand("testoog 2.'arg1'");
            ////player.ProcessCommand("testoog 2'.arg1'");
            ////player.ProcessCommand("testoog 2.'arg1 arg2' 3.arg3 5.arg4");
            ////player.ProcessCommand("testoog 2."); // INVALID
            ////player.ProcessCommand("testoog ."); // INVALID
            ////player.ProcessCommand("testoog '2.arg1'");
            ////player.ProcessCommand("unknown INVALID"); // INVALID
            ////player.ProcessCommand("/testoog");
            ////player.GoOutOfGame(); player.ProcessCommand("/testoog");
            ////player.GoInGame(entity); player.ProcessCommand("/testoog");
            ////player.GoOutOfGame(); player.ProcessCommand("testoog");
            ////player.GoInGame(entity); player.ProcessCommand("testoog INVALID"); // INVALID
            ////player.GoOutOfGame(); player.ProcessCommand("testig INVALID"); // INVALID
            ////player.GoInGame(entity); player.ProcessCommand("testig");
            ////entity.ProcessCommand("testoog INVALID"); // INVALID
            ////entity.ProcessCommand("testig");

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

            IPlayer player = new Player();
            player.ProcessCommand("test");
            player.ProcessCommand("test arg1");
            player.ProcessCommand("test 'arg1' 'arg2' 'arg3' 'arg4'");
            player.ProcessCommand("test 'arg1 arg2' 'arg3 arg4'");
            player.ProcessCommand("test 'arg1 arg2\" arg3 arg4");
            player.ProcessCommand("test 3.arg1");
            player.ProcessCommand("test 2.'arg1'");
            player.ProcessCommand("test 2'.arg1'");
            player.ProcessCommand("test 2.'arg1 arg2' 3.arg3 5.arg4");
            player.ProcessCommand("test 2."); // INVALID
            player.ProcessCommand("test ."); // INVALID
            player.ProcessCommand("test '2.arg1'");
            player.ProcessCommand("unknown"); // INVALID
            player.ProcessCommand("/test");

            ICharacter character = new Character();
            character.ProcessCommand("test");
            character.ProcessCommand("unknown"); // INVALID
        }
    }
}
