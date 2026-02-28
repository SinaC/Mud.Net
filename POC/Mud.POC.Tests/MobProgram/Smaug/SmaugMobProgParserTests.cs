using Mud.POC.MobProgram.Smaug;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.Tests.MobProgram.Smaug;

[TestClass]
public class SmaugMobProgParserTests
{
    [TestMethod]
    public void Test1()
    {
        var lines = new List<string>
{
    ">act_prog p bows before you.~",
    "snicker",
    "mer $n While $n is foolishly grovelling, $I raises her sceptre...",
    "mea $n _red You feel a sudden pain as a heavy metal object strikes your head!",
    "mer $n _red With a swift motion, she brings it down on $n's head!",
    "mpslay $n",
    "chuckle",
    "~"
};

        var block = SmaugMobProgParser.ParseBlock(lines);

        Console.WriteLine(block.Header.Type);       // Act
        Console.WriteLine(block.Header.ActFlag);    // p
        Console.WriteLine(block.Header.ActPattern); // bows before you.
        Console.WriteLine("Statements:");
        foreach (var stmt in block.Statements)
            Console.WriteLine(stmt);
    }

    [TestMethod]
    public void Test2()
    {
        var room = new Room();
        var mob = new Mob { Name = "Frupy", CurrentRoom = room };
        var player = new Player("Adventurer") { CurrentRoom = room };
        room.Mobs.Add(mob);
        room.Players.Add(player);

        var registry = new CommandRegistry();
        SmaugCommands.RegisterAll(registry);

        var executor = new MobProgExecutor(registry);
        var dispatcher = new TriggerDispatcher(executor);

        // Example act_prog block
        var blockLines = new List<string>
{
    ">act_prog p bows before you.~",
    "snicker",
    "mpe $n While $n is foolishly grovelling, $I raises her sceptre...",
    "mpslay $n",
    "~"
};
        var block = SmaugMobProgParser.ParseBlock(blockLines);
        dispatcher.Register(block);

        // Fire act trigger
        var ctx = new MobExecutionContext { Self = mob, Target = player };
        dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
    }

    [TestMethod]
    public void Test3()
    {
        var room = new Room { Vnum = 1 };
        var mob = new Mob { Name = "Frupy", CurrentRoom = room };
        var player = new Player("Adventurer") { CurrentRoom = room };
        room.Mobs.Add(mob); room.Players.Add(player);

        var registry = new CommandRegistry();
        SmaugCommands.RegisterAll(registry);

        var executor = new MobProgExecutor(registry);
        var dispatcher = new TriggerDispatcher(executor);

        var blockLines = new List<string>
            {
                ">act_prog p bows before you.~",
                "snicker",
                "mpe $n While $n is foolishly grovelling, $I raises her sceptre...",
                "if ispc($n)",
                "say Hello $n!",
                "else",
                "say I only talk to NPCs.",
                "endif",
                "mpslay $n",
                "~"
            };

        var block = SmaugMobProgParser.ParseBlock(blockLines);
        dispatcher.Register(block);

        var ctx = new MobExecutionContext { Self = mob, Target = player };
        dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
    }

    [TestMethod]
    public void Test4()
    {
        var program = @"
>act_prog p bows before you.~
if ispc($n) && rand(50) && mobinroom(2,'giant weasel')
    say You are lucky!
elseif !ispc($n)
    say I only talk to players.
endif
~";

        var room = new Room { Vnum = 1 };
        var mob = new Mob { Name = "Frupy", CurrentRoom = room };
        var player = new Player("Adventurer") { CurrentRoom = room };
        room.Mobs.Add(mob); room.Players.Add(player);

        var registry = new CommandRegistry();
        SmaugCommands.RegisterAll(registry);

        var executor = new MobProgExecutor(registry);
        var dispatcher = new TriggerDispatcher(executor);

        var blockLines = program.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var block = SmaugMobProgParser.ParseBlock(blockLines);
        dispatcher.Register(block);

        var ctx = new MobExecutionContext { Self = mob, Target = player };
        dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
    }

    [TestMethod]
    public void Test5()
    {
        var room = new Room { Vnum = 1 };
        var mob = new Mob { Name = "Frupy", CurrentRoom = room };
        var player = new Player("Adventurer") { CurrentRoom = room };
        room.Mobs.Add(mob); room.Players.Add(player);

        var registry = new CommandRegistry();
        SmaugCommands.RegisterAll(registry);

        var executor = new MobProgExecutor(registry);
        var dispatcher = new TriggerDispatcher(executor);

        var blockLines = new List<string>{
                ">act_prog p bows before you.~",
                "snicker",
                "mpe $n While $n is foolishly grovelling, $I raises her sceptre...",
                "if ispc($n) && rand(50) && 1<5",
                "say Hello $n!",
                "else",
                "say I only talk to NPCs.",
                "endif",
                "mpslay $n",
                "~"
            };

        var block = SmaugMobProgParser.ParseBlock(blockLines);
        dispatcher.Register(block);

        var ctx = new MobExecutionContext { Self = mob, Target = player };
        dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
    }

    [TestMethod]
    public void Test6()
    {
        var program = @"
>act_prog p bows before you.~
snicker
mpe $n While $n is foolishly grovelling, $I raises her sceptre...
if ispc($n) && rand(50)
say Hello $n! You look quite nervous.
elseif isnpc($n)
say I only talk to NPCs.
else
say I see nothing.
endif
mpslay $n
~
>fight_prog 100~
if mobinroom(2,'giant weasel')
mpforce 'giant weasel' attack $n
else
mpdamage $n 50
endif
if rand(25)
c 'fireball' $n
endif
~
>greet_prog 100~
if ispc($n)
say Greetings $n! Welcome to the hall.
else
say I do not greet non-players.
endif
~
>rand_prog 50~
mpe The Queen's weasels flatten their ears as she attempts to sing.
mpasound An indescribably awful wailing assaults your ears!
mpforce weasel moan
~";

        var room = new Room { Vnum = 1 };
        var mob = new Mob { Name = "Frupy", CurrentRoom = room };
        var player = new Player("Adventurer") { CurrentRoom = room };
        room.Mobs.Add(mob); room.Players.Add(player);

        var registry = new CommandRegistry();
        SmaugCommands.RegisterAll(registry);

        var executor = new MobProgExecutor(registry);
        var dispatcher = new TriggerDispatcher(executor);

        var blockLines = program.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        var block = SmaugMobProgParser.ParseBlock(blockLines);
        dispatcher.Register(block);

        var ctx = new MobExecutionContext { Self = mob, Target = player };
        dispatcher.Dispatch("Act", "Adventurer bows before you.", ctx);
    }
}
