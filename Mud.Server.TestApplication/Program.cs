using System;
using System.Configuration;
using Mud.Importer.Mystery;
using Mud.Logger;
using Mud.Network;
using Mud.Network.Socket;

namespace Mud.Server.TestApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Default.Initialize(ConfigurationManager.AppSettings["logpath"], "server.log");

            //TestCommandParsing();
            TestBasicCommands();
            //TestSocketServer();
            //TestSocketServerLogin();
            //TestLoginStateMachine();
            //TestToString();
        }

        private static void TestBasicCommands()
        {
            World world = World.Instance as World;

            IPlayer player1 = world.AddPlayer(new ConsoleClient("Player1"), Guid.NewGuid(), "Player1");
            IPlayer player2 = world.AddPlayer(new ConsoleClient("Player2"), Guid.NewGuid(), "Player2");
            IAdmin admin = world.AddAdmin(new ConsoleClient("Admin1"), Guid.NewGuid(), "Admin1");

            IRoom room1 = world.AddRoom(Guid.NewGuid(), "Room1");
            IRoom room2 = world.AddRoom(Guid.NewGuid(), "Room2");
            world.AddExit(room1, room2, ServerOptions.ExitDirections.North, true);

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

            player2.ProcessCommand("i mob3");
            player2.ProcessCommand("charm mob2"); // not in same room
            player2.ProcessCommand("charm mob3"); // cannot charm itself (player2 is impersonated in mob3)
            player2.ProcessCommand("ch mob4");

            player1.ProcessCommand("say Hello World!");

            player2.ProcessCommand("order charm mob5");

            player2.ProcessCommand("north"); // no exit on north
            player2.ProcessCommand("south");
            player1.ProcessCommand("south"); // no exit on south

            player1.ProcessCommand("/who");

            admin.ProcessCommand("who");

            //player1.ProcessCommand("/commands");
            //player1.ProcessCommand("commands");
            //mob1.ProcessCommand("commands");
            //admin.ProcessCommand("commands");
        }

        private static void TestCommandParsing()
        {
            World world = World.Instance as World;
            IRoom room = world.AddRoom(Guid.NewGuid(), "Room");

            IPlayer player = world.AddPlayer(new ConsoleClient("Player"), Guid.NewGuid(), "Player");
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

            IAdmin admin = world.AddAdmin(new ConsoleClient("Admin"), Guid.NewGuid(), "Admin");
            admin.ProcessCommand("incarnate");
            admin.ProcessCommand("unknown"); // INVALID
        }

        private static void TestSocketServer()
        {
            World world = World.Instance as World;

            INetworkServer server = new SocketServer(11000);
            int i = 1;
            server.NewClientConnected += client =>
            {
                // TODO
                IPlayer player = world.AddPlayer(client, Guid.NewGuid(), "player" + (i++));
            };
            server.Initialize();
            server.Start();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
            server.Stop();
        }

        private static void TestSocketServerLogin()
        {
            World world = World.Instance as World;

            INetworkServer server = new SocketServer(11000);
            int i = 1;
            server.NewClientConnected += client =>
            {
                // TODO
                IPlayer player = new Player.Player(client, Guid.NewGuid());
                player.Send("Why don't you login or tell us the name you wish to be known by?");
            };
            server.Initialize();
            server.Start();
            Console.WriteLine("Press ENTER to continue...");
            Console.ReadLine();
            server.Stop();
        }

        private static void TestImport()
        {
            MysteryImporter importer = new MysteryImporter();
            importer.Load(@"D:\GitHub\OldMud\area\midgaard.are");
            importer.Parse();

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

        private static void TestLoginStateMachine()
        {
            IPlayer player = new Player.Player(new ConsoleClient("Player"), Guid.NewGuid());
            while (true)
            {
                string command = Console.ReadLine();
                player.ProcessCommand(command);
            }
        }

        private static void TestToString()
        {
            World world = World.Instance as World;
            IRoom room = world.AddRoom(Guid.NewGuid(), "Room");
            ICharacter character = world.AddCharacter(Guid.NewGuid(), "Mob1", room);

            //string test = String.Format("n:{0:n} e:{0:n}", character);
            //Console.WriteLine(test);

            IPlayer player1 = world.AddPlayer(new ConsoleClient("Player1"), Guid.NewGuid(), "Player1");
            player1.ProcessCommand("im mob1");
            (character as Character.Character).Act(Character.Character.ActOptions.ToAll, "test {0:n} {1} {2:dd/MM/yyyy} {3:0.00}", character, "param_2", DateTime.Now, 123.4567);
        }
    }
}
