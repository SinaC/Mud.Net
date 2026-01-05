using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Quest;
using Mud.Server.Options;
using Mud.Server.Random;

namespace Mud.Server.Quest;

[Export(typeof(IQuestManager)), Shared]
public class QuestManager : IQuestManager
{
    private ILogger<QuestManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private ICharacterManager CharacterManager { get; }
    private IRandomManager RandomManager { get; }
    private QuestOptions QuestOptions { get; }

    private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

    public QuestManager(ILogger<QuestManager> logger, IServiceProvider serviceProvider, ICharacterManager characterManager, IRandomManager randomManager, IOptions<QuestOptions> questOptions)
    {
        _questBlueprints = [];

        Logger = logger;
        ServiceProvider = serviceProvider;

        CharacterManager = characterManager;
        RandomManager = randomManager;
        QuestOptions = questOptions.Value;
    }

    public IReadOnlyCollection<QuestBlueprint> QuestBlueprints
        => _questBlueprints.Values.ToList().AsReadOnly();

    public QuestBlueprint? GetQuestBlueprint(int id)
        => _questBlueprints.GetValueOrDefault(id);

    public void AddQuestBlueprint(QuestBlueprint blueprint)
    {
        if (!_questBlueprints.TryAdd(blueprint.Id, blueprint))
            Logger.LogError("Quest blueprint duplicate {blueprintId}!!!", blueprint.Id);
    }

    public IPredefinedQuest? AddQuest(QuestBlueprint questBlueprint, IPlayableCharacter pc, INonPlayableCharacter questGiver)
    {
        var quest = ServiceProvider.GetRequiredService<IPredefinedQuest>();
        quest.Initialize(questBlueprint, pc, questGiver);
        pc.AddQuest(quest);

        return quest;
    }

    public IPredefinedQuest? AddQuest(CurrentQuestData questData, IPlayableCharacter pc)
    {
        var questBlueprint = GetQuestBlueprint(questData.QuestId);
        if (questBlueprint == null)
        {
            Logger.LogError("Quest blueprint id {blueprintId} not found!!!", questData.QuestId);
            return null;
        }

        var quest = ServiceProvider.GetRequiredService<IPredefinedQuest>();
        var initialized = quest.Initialize(questBlueprint, questData, pc);
        if (!initialized)
            return null;
        pc.AddQuest(quest);
        return quest;
    }

    public IGeneratedQuest? GenerateQuest(IPlayableCharacter pc, INonPlayableCharacter questGiver)
    {
        var target = RandomManager.Random(CharacterManager.NonPlayableCharacters.Where(x => IsValidQuestTarget(x, pc)));
        if (target == null)
            return null;
        var chance = RandomManager.Next(100);
        // quest item on target
        if (chance <= 10)
        {
            var timeLimit = 10 + RandomManager.Next(20);
            var itemId = RandomManager.Random(QuestOptions.ItemToFindBlueprintIds);
            var quest = ServiceProvider.GetRequiredService<IGeneratedQuest>();
            var initialized = quest.Initialize(pc, questGiver, itemId, target, target.Room, timeLimit);
            if (!initialized)
                return null;
            pc.AddQuest(quest);
            return quest;
        }
        // quest item on the floor
        else if (chance <= 35)
        {
            var timeLimit = 10 + RandomManager.Next(20);
            var itemId = RandomManager.Random(QuestOptions.ItemToFindBlueprintIds);
            var quest = ServiceProvider.GetRequiredService<IGeneratedQuest>();
            var initialized = quest.Initialize(pc, questGiver, itemId, target.Room, target.Level, timeLimit);
            if (!initialized)
                return null;
            pc.AddQuest(quest);
            return quest;
        }
        // kill target
        else
        {
            var timeLimit = 10 + RandomManager.Next(20);
            var quest = ServiceProvider.GetRequiredService<IGeneratedQuest>();
            var initialized = quest.Initialize(pc, questGiver, target, target.Room, timeLimit);
            if (!initialized)
                return null;
            pc.AddQuest(quest);
            return quest;
        }
    }

    private static bool IsValidQuestTarget(INonPlayableCharacter target, IPlayableCharacter pc)
    {
        var room = target.Room;
        var levelDiff = target.Level - pc.Level;
        return
            levelDiff >= -7 && levelDiff <= 6
            && pc.IsSafe(target) == null
            && !target.CharacterFlags.HasAny("charm", "pet")
            && !room.RoomFlags.HasAny("private", "safe", "petshop", "imponly", "godsonly", "heroesonly", "newbiesonly");
            //&& room.IsLinked()); // TODO: should check if a path can be made to that room
    }

    /*
     * class questor extends Mob {
  checkItem() {
    // Check if rewardList and objquest exists
    var i = 0;
    while ( i < this.rewardList.size() ) {
      var vnum = this.rewardList[i][2];
      if ( vnum > 0 ) {
	var obj = this.oLoad( vnum );
	if ( obj == NULL )
	  log("BUG IN QUESTOR: rewardList["+i+"] vnum: "+vnum+" doesn't exist.");
	else
	  obj.destroy();
      }
      i = i+1;
    }
    while ( i < this.objquest.size() ) {
      var vnum = this.objquest[i];
      var obj = this.oLoad( vnum );
      if ( obj == NULL )
	log("BUG IN QUESTOR: objquest["+i+"] vnum: "+vnum+" doesn't exist.");
      else
	obj.destroy();
      i = i+1;
    }
  }
  onCreate() {
    objvar this:rewardList = 
      [[ // reward name, reward handle, reward vnum, reward cost
	[[ "{yCloak of {rM{gy{bs{mt{ce{yr{ry{x", "cloak", 40, 2500 ]],
	[[ "{yBreastplate of {rM{gy{bs{mt{ce{yr{ry{x", "breastplate", 44, 2000 ]],
	//	[[ "{ySword of {rM{gy{bs{mt{ce{yr{ry{x", "sword", 26, 1000 ]],
	//	[[ "{yDagger of {rM{gy{bs{mt{ce{yr{ry{x", "dagger", 26, 1000 ]],
	//	[[ "{yMace of {rM{gy{bs{mt{ce{yr{ry{x", "mace", 26, 1000 ]],
	//	[[ "{yAxe of {rM{gy{bs{mt{ce{yr{ry{x", "axe", 26, 1000 ]],
	//	[[ "{yFlail of {rM{gy{bs{mt{ce{yr{ry{x", "flail", 26, 1000 ]],
	//	[[ "{yWhip of {rM{gy{bs{mt{ce{yr{ry{x", "whip", 26, 1000 ]],
	[[ "{yRing of {rM{gy{bs{mt{ce{yr{ry{x", "ring", 27, 850 ]],
	[[ "{yAmulet of {rM{gy{bs{mt{ce{yr{ry{x", "amulet", 28, 750 ]],
	[[ "{yShield of {rM{gy{bs{mt{ce{yr{ry{x", "shield", 29, 750 ]],
	[[ "{yDecanter of Endless Water{x", "decanter", 31, 550 ]],
	[[ "3500 gold pieces{x", "gold", 0, 500 ]],
	[[ "30{y Practices{x", "practice", 0, 500 ]]
       ]];

    // Item the questor asks to find
    objvar this:objquest = [[ 32, 33, 34, 35, 36, 41 ]];

    checkItem();
  }

  onSpeech( act, msg ) {
    if ( !act.isNPC() ) {
      act.nextquest = 0;    

      var w = msg.words();
      if ( "help" in w )
        questHelp( act );
      else if ( "list" in w )
        questList( act );
      else if ( "time" in w )
        questTime( act );
      else if ( "info" in w )
        questInfo( act );
      else if ( "giveup" in w )
        questGiveup( act );
      else if ( "complete" in w )
        questComplete( act );
      else if ( "request" in w )
        questRequest( act );
      else if ( "points" in w )
        questPoints( act );
      else if ( w[0] %= "buy" && w.size() > 1 )
        questBuy( act, w[1] );
    }
  }

  // generate
  isAcceptable( mob, act ) {
    var room = mob.room();
    var levelDiff = mob.level() - act.level();
    result = !(
           !mob.isNPC() ||
           levelDiff >= 6 || levelDiff <= -7 ||
           !room.linked() ||
           room.name() == "" ||
           act.isSafe(mob) ||
           mob.checkAffect( "charm" ) ||
	   mob.checkAct( "reserved" ) ||
           room.checkAttr( "flags", "private" ) ||
           room.checkAttr( "flags", "safe" ) ||
           room.checkAttr( "flags", "imp_only" ) ||
           room.checkAttr( "flags", "gods_only" ) ||
           room.checkAttr( "flags", "heroes_only" ) ||
           room.checkAttr( "flags", "newbies_only" ) ||
           room.checkAttr( "flags", "arena" ) ||
           room.checkAttr( "flags", "pet_shop" ) 
              );
  }
  generateQuest( act ) {
    var victim = any[[ mob <- getCharList(), isAcceptable( mob, act ) ]];
    if ( victim != NULL ) {
      // even if the player gets an obj quest, we search an inrange area to drop the item
      if ( chance(35) ) {          // obj quest
        var obj = victim.room().oLoad(this.objquest.random());
        //var whichOne = this.objquest[random(this.objquest.size())];
        //var obj = victim.room().oLoad( whichOne ); // load item in the victim's room

        //log("item quest: "+obj);

        obj.setTimer(100); // about 50 minutes
        act.questobj = obj;
        act.questobjloc = obj.room().vnum();
        `dirsay `act` Vile pilferers have stolen `obj.shortDescr()` from the royal treasury!
        `dirsay `act` My court wizardess, with her magic mirror, has pinpointed its location.
        `dirsay `act` Look in the general area of `obj.room().areaName()` for `obj.shortDescr()`
      }
      else {                       // mob quest
        //log("mob quest: "+victim);

	victim.toggleAct("reserved"); // set reserved flag ON (toggle)

        act.questmob = victim;
        if ( random(2) == 0 ) {
          `dirsay `act` An enemy of mine, `victim.shortDescr()` is making vile threats against the crown.
          `dirsay `act` This threat must be eliminated!
        }
        else {
          `dirsay `act` Rune's most heinous criminal `victim.shortDescr()` has escaped from the dungeon!
          `dirsay `act` Since the escape, `victim.shortDescr()` has murdered `2+random(18)` civillians!
          `dirsay `act` The penalty for this crime is death, and you are to deliver the sentence!
        }
        `dirsay `act` Seek `victim.shortDescr()` somewhere in the vicinity of `victim.room().name()`
        `dirsay `act` That location is in the general area of `victim.room().areaName()`
      }
    }
    else {
      `dirsay `act` I'm sorry, but I don't have any quests for you at this time.
      `dirsay `act` Try again later.
      act.nextquest = 2;
    }
  }
  // request
  questRequest( act ) {
    if ( act.onquest )
      `dirsay `act` But you're already on a quest!
    else if ( act.nextquest > 0 ) {
      `dirsay `act` You're very brave, `act.name()`, but let someone else have a chance.
      `dirsay `act` Come back later.
    }
    else {
      `dirsay `act` Thank you, brave `act.name()`
      generateQuest(act);

      if ( act.questmob != NULL || act.questobj != NULL ) {
        act.questgiver = this;

        act.countdown = 10+random(20);
        act.onquest = 1;
        `dirsay `act` You have `act.countdown` minutes to complete this quest.
        `dirsay `act` May the gods go with you!
      }
    }
  }

  // points
  questPoints( act ) {
    act.sendTo("You have `act.questpoints` quest points.");
  }

  // list
  questList( act ) {
    `qdirsay `act` Current Quest Items available for Purchase:
    var i = 0;
    while ( i < this.rewardList.size() ) {
      var row;
      row = "{x" +
            this.rewardList[i][3].pad(4)+
            "qp.........."+
            this.rewardList[i][0];
      i = i+1;
      act.sendTo(row);
    }
    `qdirsay `act` To buy an item, type 'say buy <item name>'
  }

  // giveup
  questGiveup( act ) {
    if ( act.questgiver != this )
      `dirsay `act` I never sent you on a quest! Perhaps you're thinking of someone else.
    else if ( act.onquest ) {
      if ( act.questpoints > 1 ) {
	act.questpoints = act.questpoints - 1;
	`dirsay `act` You surprise me `act`.
        `dirsay `act` I thought you could handle a such simple task.
      }
      else
        `dirsay `act` Perhaps it was too difficult for you, `act`.
      if ( act.questobj != NULL )
        act.questobj.destroy(); // destroy object
      act.questClear();
      act.nextquest = 15;
    }
    else
      `dirsay `act` But you aren't on a quest, `act`
  }
 
  // help
  questHelp( act ) {
    `qdirsay `act` I'm a questmaster, use one of the following commands:
    `qdirsay `act` say list      list of quest items
    `qdirsay `act` say time      remaining time until end of quest
    `qdirsay `act` {x            or until next quest
    `qdirsay `act` say info      informations about your actual quest
    `qdirsay `act` "say giveup    give up your actual quest
    `qdirsay `act` say complete  to tell me your quest is completed
    `qdirsay `act` say request   request a new quest
    `qdirsay `act` say points    quest points
    `qdirsay `act` say buy       buy quest item
    `qdirsay `act` You can have some quest info at any time using: showinfo quest
  }

  // time
  questTime( act ) {
    if ( !act.onquest ) {
      act.sendTo("You aren't currently on a quest.");
      if ( act.nextquest > 1 )
	act.sendTo("There are `act.nextquest` minutes remaining until you can go on another quest.");
      else if (act.nextquest == 1)
	act.sendTo("There is less than a minute remaining until you can go on another quest.");
    }
    else if ( act.countdown > 0 )
      act.sendTo("Time left for current quest: `act.countdown`");
  }

  // info
  questInfo( act ) {
    if ( act.onquest )
      if ( act.questcomplete == 1 )
        act.sendTo("Your quest is ALMOST complete!");
      else if ( act.questobj != NULL )
        act.sendTo("You are on a quest to recover the fabled `act.questobj`");
      else if ( act.questmob != NULL )
        act.sendTo("You are on a quest to slay the dreaded `act.questmob.shortDescr()`");
      else
        act.sendTo("You aren't currently on a quest.");
    else
      act.sendTo("You aren't currently on a quest.");
  }

  // buy reward
  findIndex( name ) {
    var i = 0;
    result = -1;
    while ( i < this.rewardList.size() && result == -1 ) {
      if ( this.rewardList[i][1].startsWith(name) )
        result = i;
      i = i+1;
    }
  }
  questBuySpecial( act, i ) {
    if ( this.rewardList[i][1] %= "practice" ) {
      `dirsay `act` This is your 30 practices.
      act.actTo("$n gives you 30 practices.",this);
      act.addPractice( 30 );
    }
    else if ( this.rewardList[i][1] %= "gold" ) {
      `dirsay `act` This is your money, 3500 gold.
      act.actTo("$n gives you 3500 gold.",this);
      act.addGold( 3500 );
    }
    else {
      `dirsay `act` I don't have that item.
      log("BUG: trying to buy quest reward #"+i );
    }    
  }
  questBuy( act, name ) {
    var i = findIndex( name );
    if ( i < 0 )
      `dirsay `act` I don't have that item.
    else {
      if ( this.rewardList[i][3] > act.questpoints )
        `dirsay `act` You don't have enough questpoints to buy `this.rewardList[i][0]`
      else {
        if ( this.rewardList[i][2] != 0 ) {
          // we can't create item on questor and give it to player because
	  // quest reward are no_drop/stay_death, so we immediately load it on the player
	  // an simulate a give
           var obj = act.oLoad(this.rewardList[i][2]);
           obj.setLevel(act.level());
           act.actAll("$n gives $n to $n.",[[this,obj,act]]);
        }
        else
          questBuySpecial( act, i );
        act.questpoints = act.questpoints - this.rewardList[i][3];
      }
    }
  }

  // complete
  questComplete( act ) {
    if (act.questgiver != this)
      `dirsay `act` I never sent you on a quest! Perhaps you're thinking of someone else.
    else if ( act.onquest && act.countdown > 0 ) {
      if ( ( act.questobj != NULL && act.questobj.carriedBy() == act )
             || act.questcomplete == 1 ) {

        if ( act.questobj != NULL ) { // obj quest
          force act `give `act.questobj` `this`
          act.questobj.destroy();
        }

  	var reward = 1500 + random(13500);
        var pointreward = 5 + random(20);

        `dirsay `act` Congratulations on completing your quest!
        `dirsay `act` As a reward, I am giving you `pointreward` quest points, and `reward` silver.
	if ( chance(8) ) {
	  var pracreward = 1+random(7);
	  `dirsay `act` You gain `pracreward` practices!
	  act.addPractice(pracreward);
	}

        act.questClear();
	act.nextquest = 20;

        var gold = reward / 100;
        var silver = reward - ( gold * 100 );
        act.addSilver( silver );
	act.addGold( gold );
	act.questpoints = act.questpoints + pointreward;
      }
      else if ( ( act.questmob != NULL || act.questobj != NULL ) )
        `dirsay `act` You haven't completed the quest yet, but there is still time!
    }
    else if ( act.nextquest > 0 )
      `dirsay `act` But you didn't complete your quest in time!
    else 
      `dirsay `act` You have to REQUEST a quest first, `act`.
  }

  // Fix
  questFix( act ) {
    act.onquest = 0;
    act.questgiver = NULL;
    act.countdown = 0;
    act.questmob = NULL;
    act.questobj = NULL;
    act.questobjloc = 0;
    act.nextquest = 2;
    act.questcomplete = 0;
  }
}

*/
}
