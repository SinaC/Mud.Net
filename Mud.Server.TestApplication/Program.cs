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

            WorldTest world = WorldTest.Instance as WorldTest;
            
            IPlayer player1 = world.AddPlayer(Guid.NewGuid(), "Player1");
            IPlayer player2 = world.AddPlayer(Guid.NewGuid(), "Player2");

            IRoom room1 = world.AddRoom(Guid.NewGuid(), "Room1");
            IRoom room2 = world.AddRoom(Guid.NewGuid(), "Room2");

            ICharacter mob1 = world.AddCharacter(Guid.NewGuid(), "Mob1", room1);
            ICharacter mob2 = world.AddCharacter(Guid.NewGuid(), "Mob2", room1);
            ICharacter mob3 = world.AddCharacter(Guid.NewGuid(), "Mob3", room2);
            ICharacter mob4 = world.AddCharacter(Guid.NewGuid(), "Mob4", room2);
            ICharacter mob5 = world.AddCharacter(Guid.NewGuid(), "Mob5", room2);

            player1.ProcessCommand("impersonate mob1");
            player1.ProcessCommand("order"); // not controlling anyone
            player1.ProcessCommand("charm mob2");
            player1.ProcessCommand("test");
            player1.ProcessCommand("order test");

            player2.ProcessCommand("gossip Hellow :)");
            player2.ProcessCommand("tell player1 Tsekwa =D");

            player2.ProcessCommand("impersonate mob3");
            player2.ProcessCommand("charm mob2"); // not in same room
            player2.ProcessCommand("charm mob3"); // cannot charm itself (player2 is impersonated in mob3)
            player2.ProcessCommand("charm mob4");

            player1.ProcessCommand("say Hello World!");

            player2.ProcessCommand("order charm mob5");
        }

        private static void TestCommandParsing()
        {
            WorldTest world = WorldTest.Instance as WorldTest;
            IRoom room = world.AddRoom(Guid.NewGuid(), "Room");

            IPlayer player = world.AddPlayer(Guid.NewGuid(), "Player");
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

            ICharacter character = world.AddCharacter(Guid.NewGuid(), "Character", room);
            character.ProcessCommand("look");
            character.ProcessCommand("tell"); // INVALID because Player commands are not accessible by Character
            character.ProcessCommand("unknown"); // INVALID

            player.ProcessCommand("impersonate"); // INVALID to un-impersonate, player already must be impersonated
            player.ProcessCommand("impersonate character");
            player.ProcessCommand("/tell");
            player.ProcessCommand("tell"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
            player.ProcessCommand("look");

            player.ProcessCommand("impersonate"); // INVALID because OOG is not accessible while impersonating unless if command starts with /
            player.ProcessCommand("/impersonate");
            player.ProcessCommand("/tell");
            player.ProcessCommand("tell");
            player.ProcessCommand("look"); // INVALID because Character commands are not accessible by Player unless if impersonating

            IAdmin admin = world.AddAdmin(Guid.NewGuid(), "Admin");
            admin.ProcessCommand("incarnate");
            admin.ProcessCommand("unknown"); // INVALID
        }
    }
}
