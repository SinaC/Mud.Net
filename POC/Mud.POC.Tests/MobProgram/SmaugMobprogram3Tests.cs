using Mud.POC.MobProgram.SmaugMobprogram3;

namespace Mud.POC.Tests.MobProgram;

[TestClass]
public class SmaugMobprogram3Tests
{
    [TestMethod]
    public void Sample0()
    {
        string script = @"
>greet_prog 100
if ispc($n)
  say Hello $n!
elseif isnpc($n)
  say I only talk to players.
else
  say I see nothing.
endif
~";

        var lexer = new Lexer(script);
        var tokens = lexer.Tokenize();
        var parser = new Parser();
        var programs = parser.Parse(tokens);

        var room = new Room();
        var mob = new Mob("Guardian");
        var player = new Player("Bob");

        room.Enter(mob);
        room.Enter(player);

        var ctx = new POC.MobProgram.SmaugMobprogram3.ExecutionContext
        {
            Self = mob,
            Actor = player
        };

        var evaluator = new Evaluator();
        evaluator.Execute(programs[0], ctx);
    }

    [TestMethod]
    public void Sample1()
    {
        string script = @"
> rand_prog 66~
wield sceptre
mpat 32199 c 'cure blindness'
~
> fight_prog 66~
wield sceptre
if name($n) == serving maid fire giant
say You would attack your own Queen? Perish!
mpe Frupy smashes her sceptre into the servant's face!
mpdamage $n 500
endif
if rand(33)
gouge
disarm
snicker
mpdamage $r 125
else
if rand(45)
mpe _blu Frupy proves herself a master of arcane lore...
c 'faerie fire' $r
c blindness $r
c weaken $r
c 'energy drain' $r
else
if rand(33)
c fireball 
c fireball 
c fireball 
else
mprestore self 100
endif
endif
endif
~
> all_greet_prog 100~
if name($n) == serving maid fire giant
sniff
say I have no need of your services... leave.
mpforce $n eye
say LEAVE!
mpforce $n eep
mpforce $n s
roll
else
if ispc($n)
eye $n
say Adventurer, you come at a most inconvenient time...
frown
say But bow before me in respect, and I shall deal with your requests...
snicker
else
endif
endif
~
> act_prog p bows before you.~
snicker
mer $n While $n is foolishly grovelling, $I raises her sceptre...
mea $n _red You feel a sudden pain as a heavy metal object strikes your head!
mer $n _red With a swift motion, she brings it down on $n's head!
mpslay $n
chuckle
~
> fight_prog 100~
if mobinroom(32008) == 2
mpforce 'giant weasel' murder $n
mpforce '2.giant weasel' murder $n
else
if mobinroom(32008) == 1
mpforce 'giant weasel' murder $n
else
endif
endif
~
> rand_prog 3~
mpe The Queen's weasels flatten their ears as she attempts to sing.
mpasound An indescribably awful wailing assaults your ears!
mpforce weasel moan
|
~";


        var lexer = new Lexer(script);
        var tokens = lexer.Tokenize();
        var parser = new Parser();
        var programs = parser.Parse(tokens);

        var room = new Room();
        var mob = new Mob("Guardian");
        var player = new Player("Bob");

        room.Enter(mob);
        room.Enter(player);

        var ctx = new POC.MobProgram.SmaugMobprogram3.ExecutionContext
        {
            Self = mob,
            Actor = player
        };

        var evaluator = new Evaluator();
        foreach(var program in programs)
            evaluator.Execute(program, ctx);
    }
}
