using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common.Attributes;
using Mud.Importer;
using Mud.Importer.Rom;
using Mud.Network.Interfaces;
using Mud.Blueprints.Item;
using Mud.Blueprints.Room;
using Mud.Flags;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;

namespace Mud.Server.ConsoleApp;

[Export, Shared]
public class ConsoleServer
{
    private ILogger<ConsoleServer> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IServer Server { get; }
    private IAreaManager AreaManager { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IAdminManager AdminManager { get; }
    private IPlayerManager PlayerManager { get; }
    private ImportOptions ImportOptions { get; }
    private WorldOptions WorldOptions { get; }

    public ConsoleServer(ILogger<ConsoleServer> logger, IServiceProvider serviceProvider, IServer server, IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IAdminManager adminManager, IPlayerManager playerManager, IOptions<ImportOptions> importOptions, IOptions<WorldOptions> worldOptions)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        Server = server;
        AreaManager = areaManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        AdminManager = adminManager;
        PlayerManager = playerManager;
        ImportOptions = importOptions.Value;
        WorldOptions = worldOptions.Value;
    }

    public void Create()
    {
        try
        {
            Logger.LogInformation("Creating world");

            CreateWorld();
        }
        catch (Exception ex)
        {
            Logger.LogError("*** Fatal error. Stopping application ***");
            Logger.LogError(ex.ToString()); //Fatal exception -> stop application
            return;
        }
    }

    public void Run()
    {
        Logger.LogInformation("Starting world");

        //
        var telnetServer = ServiceProvider.GetRequiredService<ITelnetNetworkServer>();
        Server.Initialize([telnetServer]);
        Server.Start();

        var stopped = false;
        while (!stopped)
        {
            if (Console.KeyAvailable)
            {
                var line = Console.ReadLine();
                if (line != null)
                {
                    // server commands
                    if (line.StartsWith("#"))
                    {
                        line = line.Replace("#", string.Empty).ToLower();
                        if (line == "quit")
                        {
                            stopped = true;
                            break;
                        }
                        else if (line == "alist")
                        {
                            Console.WriteLine("Admins:");
                            foreach (var admin in AdminManager.Admins)
                                Console.WriteLine(admin.DisplayName + " " + admin.PlayerState + " " + (admin.Impersonating != null ? admin.Impersonating.DisplayName : "") + " " + (admin.Incarnating != null ? admin.Incarnating.DisplayName : ""));
                        }
                        else if (line == "plist")
                        {
                            Console.WriteLine("players:");
                            foreach (var player in PlayerManager.Players)
                                Console.WriteLine(player.DisplayName + " " + player.PlayerState + " " + (player.Impersonating != null ? player.Impersonating.DisplayName : ""));
                        }
                        else
                            Console.WriteLine("???");
                    }
                    else
                        Console.WriteLine("???");
                }
            }
            else
                Thread.Sleep(100);
        }
    }

    private void CreateWorld()
    {
        var path = ImportOptions.Path;

        Logger.LogInformation("Importing from {path}", path);

        var importer = ServiceProvider.GetRequiredKeyedService<IImporter>(ImportOptions.Importer);
        if (ImportOptions.Lists != null && ImportOptions.Lists.Length > 0)
        {
            foreach (string list in ImportOptions.Lists)
            {
                importer.ImportByList(path, list);
            }
        }
        if (ImportOptions.Areas != null && ImportOptions.Areas.Length > 0)
        {
            importer.Import(path, ImportOptions.Areas);
        }

        // Area
        foreach (var blueprint in importer.Areas)
        {
            AreaManager.AddAreaBlueprint(blueprint);
            AreaManager.AddArea(Guid.NewGuid(), blueprint);
        }

        // Rooms
        foreach (var blueprint in importer.Rooms)
        {
            RoomManager.AddRoomBlueprint(blueprint);
            var area = AreaManager.Areas.FirstOrDefault(x => x.Blueprint.Id == blueprint.AreaId);
            if (area == null)
            {
                Logger.LogError("Area id {id} not found", blueprint.AreaId);
            }
            else
                RoomManager.AddRoom(Guid.NewGuid(), blueprint, area);
        }

        // Exits
        foreach (var room in RoomManager.Rooms)
        {
            foreach (ExitBlueprint exitBlueprint in room.Blueprint.Exits.Where(x => x != null))
            {
                var to = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == exitBlueprint.Destination);
                if (to == null)
                    Logger.LogWarning("Destination room {id} not found for room {blueprintId} direction {dir}", exitBlueprint.Destination, room.Blueprint.Id, exitBlueprint.Direction);
                else
                    RoomManager.AddExit(room, to, exitBlueprint, exitBlueprint.Direction);
            }
        }

        // Characters
        foreach (var blueprint in importer.Characters)
            CharacterManager.AddCharacterBlueprint(blueprint);

        // Items
        foreach (var blueprint in importer.Items)
            ItemManager.AddItemBlueprint(blueprint);

        // MANDATORY ITEMS
        if (ItemManager.GetItemBlueprint(WorldOptions.BlueprintIds.Corpse) == null)
        {
            ItemCorpseBlueprint corpseBlueprint = new()
            {
                Id = WorldOptions.BlueprintIds.Corpse,
                NoTake = true,
                Name = "corpse"
            };
            ItemManager.AddItemBlueprint(corpseBlueprint);
        }
        if (ItemManager.GetItemBlueprint(WorldOptions.BlueprintIds.Coins) == null)
        {
            ItemMoneyBlueprint moneyBlueprint = new()
            {
                Id = WorldOptions.BlueprintIds.Coins,
                NoTake = true,
                Name = "coins"
            };
            ItemManager.AddItemBlueprint(moneyBlueprint);
        }
        // MANDATORY ROOM
        var voidBlueprint = RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.NullRoom);
        if (voidBlueprint == null)
        {
            IArea area = AreaManager.Areas.First();
            Logger.LogError("NullRoom not found -> creation of null room with id {id} in area {name}", WorldOptions.BlueprintIds.NullRoom, area.DisplayName);
            voidBlueprint = new RoomBlueprint
            {
                Id = WorldOptions.BlueprintIds.NullRoom,
                Name = "The void",
                RoomFlags = new RoomFlags("NoRecall", "NoScan", "NoWhere")
            };
            RoomManager.AddRoomBlueprint(voidBlueprint);
            RoomManager.AddRoom(Guid.NewGuid(), voidBlueprint, area);
        }
    }
}
