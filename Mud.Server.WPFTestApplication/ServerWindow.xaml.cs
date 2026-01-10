using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Common;
using Mud.Domain;
using Mud.Importer;
using Mud.Importer.Rom;
using Mud.Network.Interfaces;
using Mud.Blueprints.Character;
using Mud.Blueprints.Item;
using Mud.Blueprints.LootTable;
using Mud.Blueprints.Quest;
using Mud.Blueprints.Room;
using Mud.Server.Flags;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Area;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Interfaces.Room;
using Mud.Server.Options;
using Mud.Server.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Mud.Server.WPFTestApplication;

/// <summary>
/// Interaction logic for ServerWindow.xaml
/// </summary>
public partial class ServerWindow : Window, INetworkServer
{
    private static ServerWindow _serverWindowInstance;

    private ILogger<ServerWindow> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IServer Server { get; }
    private IServerAdminCommand ServerPlayerCommand { get; }
    private IPlayerManager PlayerManager { get; }
    private IAdminManager AdminManager { get; }
    private IAreaManager AreaManager { get; }
    private IRoomManager RoomManager { get; }
    private ICharacterManager CharacterManager { get; }
    private IItemManager ItemManager { get; }
    private IQuestManager QuestManager { get; }
    private IRandomManager RandomManager { get; }
    private ImportOptions ImportOptions { get; }
    private WorldOptions WorldOptions { get; }

    public ServerWindow(ILogger<ServerWindow> logger, IServiceProvider serviceProvider, IServer server, IServerAdminCommand serverAdminCommand, IPlayerManager playerManager, IAdminManager adminManager, IAreaManager areaManager, IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, IQuestManager questManager, IRandomManager randomManager,
        IOptions<ImportOptions> importOptions, IOptions<WorldOptions> worldOptions)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
        Server = server;
        ServerPlayerCommand = serverAdminCommand;
        PlayerManager = playerManager;
        AdminManager = adminManager;
        AreaManager = areaManager;
        RoomManager = roomManager;
        CharacterManager = characterManager;
        ItemManager = itemManager;
        QuestManager = questManager;
        RandomManager = randomManager;
        ImportOptions = importOptions.Value;
        WorldOptions = worldOptions.Value;

        _serverWindowInstance = this;

        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void TestLootTable()
    {
        var logger = ServiceProvider.GetRequiredService<ILogger<TreasureTable<int>>>();

        TreasureTable<int> tableSpider = new(logger, RandomManager)
        {
            Name = "TreasureList_Spider",
            Entries =
            [
                new() {
                    Value = 1, // Spider venom
                    Occurancy = 25,
                    MaxOccurancy = 1
                },
                new() {
                    Value = 2, // Spider webbing
                    Occurancy = 65,
                    MaxOccurancy = 1
                },
                new() {
                    Value = 3, // Severed spider leg
                    Occurancy = 10,
                    MaxOccurancy = 1
                }
            ]
        };
        TreasureTable<int> tableRareLoot = new(logger, RandomManager)
        {
            Name = "TreasureList_RareLoot",
            Entries =
            [
                new() {
                    Value = 4, // Ubber-sword
                    Occurancy = 1,
                    MaxOccurancy = 1,
                }
            ]
        };
        TreasureTable<int> tableEmpty = new(logger, RandomManager)
        {
            Name = "TreasureList_Empty",
        };
        CharacterLootTable<int> spiderTable = new(ServiceProvider.GetRequiredService<ILogger<CharacterLootTable<int>>>(), RandomManager)
        {
            MinLoot = 1,
            MaxLoot = 3,
            Entries =
            [
                new() {
                    Value = tableSpider,
                    Occurancy = 45,
                    Max = 2
                },
                new() {
                    Value = tableRareLoot,
                    Occurancy = 5,
                    Max = 1
                },
                new() {
                    Value = tableEmpty,
                    Occurancy = 50,
                    Max = 1
                }
            ],
            //AlwaysDrop = new List<int>
            //{
            //    99
            //}
        };
        for (int i = 0; i < 10; i++)
        {
            List<int> loots = spiderTable.GenerateLoots();
            Logger.LogInformation("***LOOT {idx}: {loots}", i, string.Join(",", loots));
        }
    }

    private void OnLoaded(object sender, RoutedEventArgs routedEventArgs)
    {
        Loaded -= OnLoaded;

        if (PendingLogs.Count > 0)
        {
            foreach (var (level, message) in PendingLogs)
                LogMessage(level, message);
            PendingLogs.Clear();
        }

        //
        //TestFlagFactory();
        //TestLootTable();

        try
        {
            Logger.LogInformation("Creating world");

            CreateWorld();
        }
        catch (Exception ex)
        {
            Logger.LogError("*** Fatal error. Stopping application ***");
            Logger.LogError(ex.ToString()); //Fatal exception -> stop application
            Application.Current.Shutdown(0);
            return;
        }

        Logger.LogInformation("Starting world");

        //
        var telnetServer = ServiceProvider.GetRequiredService<ITelnetNetworkServer>();
        Server.Initialize([telnetServer, this]);
        Server.Start();

        //CreateNewClientWindow();
        InputTextBox.Focus();
    }

    private void InputTextBox_OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Return)
            SendButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); // http://stackoverflow.com/questions/728432/how-to-programmatically-click-a-button-in-wpf
        else if (e.Key == Key.N && Keyboard.Modifiers != ModifierKeys.None) // Alt+N, Ctrl+N
            CreateNewClientWindow();
    }

    private void SendButton_OnClick(object sender, RoutedEventArgs e)
    {
        string input = InputTextBox.Text.ToLower();
        if (input == "exit" || input == "quit" || input == "stop")
        {
            Server.Stop();
            Application.Current.Shutdown();
        }
        else if (input == "alist")
        {
            OutputText("Admins:");
            foreach (IAdmin a in AdminManager.Admins)
                OutputText(a.Name + " " + a.PlayerState + " " + (a.Impersonating != null ? a.Impersonating.DisplayName : "") + " " + (a.Incarnating != null ? a.Incarnating.DisplayName : ""));
        }
        else if (input == "plist")
        {
            OutputText("players:");
            foreach (IPlayer p in PlayerManager.Players)
                OutputText(p.Name + " " + p.PlayerState + " " + (p.Impersonating != null ? p.Impersonating.DisplayName : ""));
        }
        else if (input == "dump")
        {
            Server.Dump();
        }
        else if (input.StartsWith("promote"))
        {
            string[] tokens = input.Split(' ');
            if (tokens.Length <= 1)
            {
                OutputText("A player name must be specified");
            }
            else
            {
                string playerName = tokens[1];
                if (AdminManager.Admins.Any(x => x.Name == playerName))
                {
                    OutputText("Already admin");
                }
                else
                {
                    IPlayer player = PlayerManager.Players.FirstOrDefault(x => x.Name == playerName);
                    if (player == null)
                    {
                        OutputText("No such player");
                    }
                    else
                    {
                        AdminLevels level;
                        if (tokens.Length >= 2)
                        {
                            if (!EnumHelpers.TryFindByName(tokens[2], out level))
                                level = AdminLevels.Angel;
                        }
                        else
                            level = AdminLevels.Angel;
                        OutputText($"Promoting {player.Name} to {level}");
                        ServerPlayerCommand.Promote(player, level);
                    }
                }
            }
        }
        InputTextBox.Focus();
        InputTextBox.SelectAll();
    }

    private void NewClientButton_OnClick(object sender, RoutedEventArgs e)
    {
        CreateNewClientWindow();
    }

    private void CreateNewClientWindow()
    {
        ClientWindow window = new()
        {
            ColorAccepted = true
        };
        NewClientConnected?.Invoke(window);
        window.Closed += (sender, args) =>
        {
            if (window.IsConnected)
                ClientDisconnected?.Invoke(window);
        };
        // Display client window
        window.Show();
    }

    public event NewClientConnectedEventHandler NewClientConnected;
    public event ClientDisconnectedEventHandler ClientDisconnected;

    public void Initialize()
    {
        // NOP
    }

    public void Start()
    {
        // NOP
    }

    public void Stop()
    {
        // NOP
    }

    // logging
    private static List<(string level, string message)> PendingLogs { get; } = [];

    public static void LogInRichTextBox(string level, string message)
    {
        if (_serverWindowInstance == null)
        {
            if (PendingLogs.Count > 500)
                PendingLogs.RemoveAt(0);
            PendingLogs.Add((level, message)); // delay logging until window is ready
        }
        else
        {
            _serverWindowInstance.LogMessage(level, message);
        }
    }

    private void LogMessage(string level, string message)
    {
        Dispatcher.InvokeAsync(() =>
        {
            //_serverWindowInstance.OutputRichTextBox.AppendText(message+Environment.NewLine);
            Brush color;
            if (level == "Error" || level == "Critical")
                color = Brushes.Red;
            else if (level == "Warn" || level == "Warning")
                color = Brushes.Yellow;
            else if (level == "Info" || level == "Information")
                color = Brushes.White;
            else if (level == "Debug")
                color = Brushes.LightGray;
            else if (level == "Trace")
                color = Brushes.DarkGray;
            else
                color = Brushes.Orchid; // should never happen
            Paragraph paragraph = new();
            paragraph.Inlines.Add(new Run(message)
            {
                Foreground = color
            });
            if (_serverWindowInstance.OutputRichTextBox.Document.Blocks.Count > 500)
                _serverWindowInstance.OutputRichTextBox.Document.Blocks.Remove(_serverWindowInstance.OutputRichTextBox.Document.Blocks.FirstBlock);
            _serverWindowInstance.OutputRichTextBox.Document.Blocks.Add(paragraph);
            _serverWindowInstance.OutputScrollViewer.ScrollToBottom();
        }, System.Windows.Threading.DispatcherPriority.Render);
    }

    //
    private void OutputText(string text)
    {
        Paragraph paragraph = new ();
        paragraph.Inlines.Add(text);
        _serverWindowInstance.OutputRichTextBox.Document.Blocks.Add(paragraph);
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
            IArea area = AreaManager.Areas.FirstOrDefault(x => x.Blueprint.Id == blueprint.AreaId);
            if (area == null)
            {
                Logger.LogError("Area id {id} not found", blueprint.AreaId);
            }
            else
                RoomManager.AddRoom(Guid.NewGuid(), blueprint, area);
        }

        foreach (var room in RoomManager.Rooms)
        {
            foreach(ExitBlueprint exitBlueprint in room.Blueprint.Exits.Where(x => x != null))
            {
                IRoom to = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == exitBlueprint.Destination);
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
        foreach(var blueprint in importer.Items)
            ItemManager.AddItemBlueprint(blueprint);

        // Custom blueprint to test
        ItemQuestBlueprint questItem1Blueprint = new()
        {
            Id = 80000,
            Name = "Quest item 1",
            ShortDescription = "Quest item 1",
            Description = "The quest item 1 has been left here."
        };
        ItemManager.AddItemBlueprint(questItem1Blueprint);
        ItemQuestBlueprint questItem2Blueprint = new()
        {
            Id = 80001,
            Name = "Quest item 2",
            ShortDescription = "Quest item 2",
            Description = "The quest item 2 has been left here."
        };
        ItemManager.AddItemBlueprint(questItem2Blueprint);
        CharacterNormalBlueprint construct = new()
        {
            Id = 80000,
            Name = "Construct",
            ShortDescription = "A construct",
            LongDescription = "A construct waiting orders",
            Description = "A construct is here, built from various of gears and springs",
            Sex = Sex.Neutral,
            Level = 40,
            Wealth = 0,
            Alignment = 0,
            DamageNoun = "buzz",
            DamageType = SchoolTypes.Bash,
            DamageDiceCount = 5,
            DamageDiceValue = 10,
            DamageDiceBonus = 10,
            HitPointDiceCount = 20,
            HitPointDiceValue = 30,
            HitPointDiceBonus = 300,
            ManaDiceCount = 0,
            ManaDiceValue = 0,
            ManaDiceBonus = 0,
            HitRollBonus = 10,
            ArmorBash = 300,
            ArmorPierce = 200,
            ArmorSlash = 400,
            ArmorExotic = 0,
            ActFlags = new ActFlags("Pet"),
            OffensiveFlags = new OffensiveFlags("Bash"),
            CharacterFlags = new CharacterFlags("Haste"),
            Immunities = new IRVFlags(),
            Resistances = new IRVFlags("Slash", "Fire"),
            Vulnerabilities = new IRVFlags("Acid"),
            ShieldFlags = new ShieldFlags(),
        };
        CharacterManager.AddCharacterBlueprint(construct);

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
        RoomBlueprint voidBlueprint = RoomManager.GetRoomBlueprint(WorldOptions.BlueprintIds.NullRoom);
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

        // Add dummy mobs and items to allow impersonate :)
        IRoom templeOfMota = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3001);
        IRoom templeSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3005);
        IRoom marketSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3014);
        IRoom commonSquare = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3025);
        IRoom inn = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3006);
        IRoom onTheBridge = RoomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == 3051);

        if (templeOfMota == null || templeSquare == null || marketSquare == null || commonSquare == null || inn == null)
            return;

        // Quest
        QuestKillLootTable<int> quest1KillLoot = ServiceProvider.GetRequiredService<QuestKillLootTable<int>>();
        quest1KillLoot.Name = "Quest 1 kill 1 table";
        quest1KillLoot.Entries =
            [
                new()
                {
                    Value = questItem1Blueprint.Id,
                    Percentage = 80,
                }
            ];
        QuestBlueprint questBlueprint1 = new()
        {
            Id = 1,
            Title = "Complex quest",
            Description = "Kill 3 fido, get 5 quest item 2, get 2 quest item 1 on beggar and explore temple square",
            Level = 50,
            Experience = 50000,
            Gold = 20,
            ShouldQuestItemBeDestroyed = true,
            KillObjectives =
            [
                new QuestKillObjectiveBlueprint
                {
                    Id = 0,
                    CharacterBlueprintId = 3062, // fido
                    Count = 3
                }
            ],
            LootItemObjectives =
            [
                new QuestLootItemObjectiveBlueprint
                {
                    Id = 2,
                    ItemBlueprintId = questItem1Blueprint.Id,
                    Count = 2
                }
            ],
            FloorItemObjectives =
            [
                new QuestFloorItemObjectiveBlueprint
                {
                    Id = 1,
                    ItemBlueprintId = questItem2Blueprint.Id,
                    Count = 5,
                    RoomBlueprintIds = [templeSquare.Blueprint.Id, inn.Blueprint.Id, onTheBridge.Blueprint.Id],
                    SpawnCountOnRequest = 2,
                    MaxInRoom = 1,
                    MaxInWorld = 3,
                }
            ],
            LocationObjectives =
            [
                new QuestLocationObjectiveBlueprint
                {
                    Id = 3,
                    RoomBlueprintId = templeSquare.Blueprint.Id,
                }
            ],
            KillLootTable = new Dictionary<int, QuestKillLootTable<int>> // when killing mob 3065, we receive quest item 1 (80%)
            {
                { 3065, quest1KillLoot } // beggar
            }
            // TODO: rewards
        };
        QuestManager.AddQuestBlueprint(questBlueprint1);

        QuestBlueprint questBlueprint2 = new()
        {
            Id = 2,
            Title = "Simple exploration quest",
            Description = "Explore temple of mota, temple square, market square and common square",
            Level = 10,
            Experience = 10000,
            Gold = 20,
            TimeLimit = 5,
            LocationObjectives =
            [
                new QuestLocationObjectiveBlueprint
                {
                    Id = 0,
                    RoomBlueprintId = templeOfMota.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 1,
                    RoomBlueprintId = templeSquare.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 2,
                    RoomBlueprintId = marketSquare.Blueprint.Id
                },
                new QuestLocationObjectiveBlueprint
                {
                    Id = 3,
                    RoomBlueprintId = commonSquare.Blueprint.Id
                }
            ],
            // TODO: rewards
        };
        QuestManager.AddQuestBlueprint(questBlueprint2);

        CharacterQuestorBlueprint mob10Blueprint = new()
        {
            Id = 10,
            Name = "mob10 questor",
            ShortDescription = "Tenth mob (neutral questor)",
            Description = "Tenth mob (neutral questor) is here",
            Sex = Sex.Neutral,
            Level = 60,
            LongDescription = "Tenth mob (neutral questor) is here"+Environment.NewLine,
            Wealth = 0,
            Alignment = 0,
            DamageNoun = "buzz",
            DamageType = SchoolTypes.Bash,
            DamageDiceCount = 5,
            DamageDiceValue = 10,
            DamageDiceBonus = 10,
            HitPointDiceCount = 20,
            HitPointDiceValue = 30,
            HitPointDiceBonus = 300,
            ManaDiceCount = 0,
            ManaDiceValue = 0,
            ManaDiceBonus = 0,
            HitRollBonus = 10,
            ArmorBash = 300,
            ArmorPierce = 200,
            ArmorSlash = 400,
            ArmorExotic = 0,
            ActFlags = new ActFlags(),
            OffensiveFlags = new OffensiveFlags("Bash"),
            CharacterFlags = new CharacterFlags("Haste"),
            Immunities = new IRVFlags("Magic", "Weapon"),
            Resistances = new IRVFlags("Slash", "Fire"),
            Vulnerabilities = new IRVFlags(),
            ShieldFlags = new ShieldFlags(),
            QuestBlueprints =
                [
                questBlueprint1,
                questBlueprint2
            ]
        };
        CharacterManager.AddCharacterBlueprint(mob10Blueprint);
        ICharacter mob10 = CharacterManager.AddNonPlayableCharacter(Guid.NewGuid(), mob10Blueprint, commonSquare);

        // add blueprint quest item for generated quest
        var questToken = new ItemQuestBlueprint
        {
            Id = 57,
            Name = "sinac armor",
            ShortDescription = "Sinac's %m%Armor%x%",
            Description = "Sinac's %m%Armor%x% lies here.",
            WearLocation = WearLocations.None,
            Level = 1,
            Weight = 1,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags("magic")
        };
        ItemManager.AddItemBlueprint(questToken);

        // add blueprint item with imm to magic/weapon for test purpose
        ItemLightBlueprint lightBlueprint = new()
        {
            Id = 99,
            Name = "oxtal light",
            ShortDescription = "Oxtal's famous light",
            Description = "Oxtal's Light is here",
            WearLocation = WearLocations.Light,
            Level = 1,
            DurationHours = -1,
            Weight = 1,
            Cost = 0,
            NoTake = false,
            ItemFlags = new ItemFlags("glowing", "magic"),
        };
        ItemManager.AddItemBlueprint(lightBlueprint);
    }
}
