using Mud.POC.MobProgram.AnotherMobProgram2;

namespace Mud.POC.Tests.MobProgram;

[TestClass]
public class AnotherMobProgramTests2
{
    [TestMethod]
    public void Sample0()
    {
        var program = @"
say AAAAAAAAARRRGGGHHHHH
aargh
scream
emote drops the frog and quickly squashes it to death with his foot.
emote sighs with relief.
say the mage did this to me didn't he???
say he was probably plotting with that cleric...
say go over to him and  tell him to stuff it.
say Naah. i have a better idea. The cleric is allegic to a certain flower.
say Go to New Thalos.
say Then go to the Floral Shop there.
say say ""fire' to the florist and he should give one to you.
say then go back and give the flower to that awful old geezer of a cleric.
cackle
mob junk frog
mob oload 5812
mob otransfer guts 5859
";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin" },
            Triggerer = new Mob { Name = "Hero" },
            Secondary = new Mob { Name = "AnotherMob" }
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context);
        evaluator.Evaluate(nodes);
    }

    [TestMethod]
    public void Sample1()
    {
        var program = @"
echo Start program
if rand 500
    if hour >= 20
    or hour <= 5
        *comment1
        echo $n outside business hours
    else
        *comment2
        echo $n inside business hours
    endif
  break
endif
echo This will not run if break Triggerer
";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin" },
            Triggerer = new Mob { Name = "Hero" },
            Secondary = new Mob { Name = "AnotherMob"}
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context);
        evaluator.Evaluate(nodes);
    }

    [TestMethod]
    public void Sample2()
    {
        var program = @"
if objhere 27507
*comment
 if has $i 'trash'
  enter portal
  if mobs > 1
   mob remember $r
   if ispc $q
   or carries $q 'prize'
   or carries $q 'shard2'
   or carries $q 'shard4'
   or carries $q 'quest'
   or carries $q 'shard1'
    mob forget
    break
   else
    give <<qsv>> $q
    mob forget
    get portal
    mob go 27506
    mob echo $I waddles in from $l journeys.
     drop all.portal
    break
   endif
  else
   mob forget
   break
  endif
 else
  if rand 25
   mob oload 27803
  else
   if rand 25
    mob oload 27804
   else
    mob oload 27805
   endif
  endif
  mob call 27801 null null null
  break
 endif
else
 mob oload 27507 1 room
 mob call 27801 null null null
 break
endif
";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin", Level = 15 },
            Triggerer = new Mob { Name = "Hero", IsPc = true, Level = 10 },
            Secondary = new Mob { Name = "AnotherMob", Level = 50 }
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context, variables);
        evaluator.Evaluate(nodes);
    }

    [TestMethod]
    public void Sample3()
    {
        var program = @"
if isnpc $n
   chuckle
   poke $n
   break
else
   if level $n <= 5
   or isgood $n
      tell $n I would rather you didnt poke me.
   else
      if level $n > 30
	 scream
	 say Ya know $n. I hate being poked!!!
	 if mobhere guard
	    mob force guard kill $n
	 endif
	 kill $n
	 break
      endif
      slap $n
      shout MOMMY!!! $N is poking me.
   endif
endif";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin", Level = 15 },
            Triggerer = new Mob { Name = "Hero", IsPc = true, Level = 33, Alignment = 0 },
            Secondary = new Mob { Name = "AnotherMob", Level = 50 }
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context, variables);
        evaluator.Evaluate(nodes);
    }

    [TestMethod]
    public void Sample4()
    {
        var program = @"
if hour >= 20
or hour <= 5
  if rand 50
    mob echo $I {5murmurs{0 in $l sleep.
  else
    mob echo $I {6opens{0 an {4e{8y{4e{0 and looks around.
  endif
endif
if hour >= 6
and hour <= 8
  if rand 10
    mob echo $I {^slips{0 out the hatch to get breakfeast.
  else
    mob echo $I pulls out her {1w{8e{1a{8p{1o{8n{1s{0 and goes through $k {#morn{&ing{0 exercises.
  endif
endif
if hour >= 9
and hour <= 11
  if rand 50
    mob echo $I pulls a {1curtain{0 open and curls up in a spot of {#sun{&light{0.
  else
    mob echo $I {5walks{0 around {5straightening{0 up the room.
  endif
endif
if hour == 12
  mob echo $I {^slips out the hatch and brings back a {&f{7i{&s{7h{0.
  mob oload 16050
  eat fish
endif
if hour >= 13
and hour <= 16
  if rand 10
    mob echo $I curls up for a nap.
  else
    mob echo $I goes out for a quick {Bswim{0.
  endif
endif
if hour >= 17
and hour <= 19
  if rand 10
    mob echo $I {^slips{0 out the hatch and brings back a {&f{7i{&s{7h{0.
    mob oload 16050
    eat fish
  else
    mob echo $I {5opens{0 a {1curtain{0 and watches the {#sun{%set{0.
  endif
endif";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin", Level = 15 },
            Triggerer = new Mob { Name = "Hero", IsPc = true, Level = 35, Alignment = 0 },
            Secondary = new Mob { Name = "AnotherMob", Level = 50 }
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context, variables);
        evaluator.Evaluate(nodes);
    }

    [TestMethod]
    public void Sample5()
    {
        var program = @"
if name $n 'saphyre'
*unavailable or name $n == Khalandar
or name $n 'fnor'
or name $n 'ramoth'
  say Why certainly, $N.
  give holder $n
else
  if isimmort $n
    say Sorry, better ask Saphyre to get that for you.
  else
    say I don't think so, $N.
    mob kill $n
  endif
endif";

        var context = new MobContext
        {
            Self = new Mob { Name = "Goblin", Level = 15 },
            Triggerer = new Mob { Name = "fnor", IsPc = true, Level = 35, Alignment = 0 },
            Secondary = new Mob { Name = "AnotherMob", Level = 50 }
        };
        var variables = new Dictionary<string, int> { ["hour"] = 10 };

        var parser = new MobProgramParser(program);
        var nodes = parser.Parse();

        var evaluator = new MobProgramEvaluator(context, variables);
        evaluator.Evaluate(nodes);
    }
}
