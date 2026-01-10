using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mud.Blueprints.Quest;
using Mud.Common.Attributes;
using Mud.Domain.SerializationData.Avatar;
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
    private GeneratedQuestOptions GeneratedQuestOptions { get; }

    private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

    public QuestManager(ILogger<QuestManager> logger, IServiceProvider serviceProvider, ICharacterManager characterManager, IRandomManager randomManager, IOptions<GeneratedQuestOptions> generatedQuestOptions)
    {
        _questBlueprints = [];

        Logger = logger;
        ServiceProvider = serviceProvider;

        CharacterManager = characterManager;
        RandomManager = randomManager;
        GeneratedQuestOptions = generatedQuestOptions.Value;
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
            var itemId = RandomManager.Random(GeneratedQuestOptions.ItemToFindBlueprintIds);
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
            var itemId = RandomManager.Random(GeneratedQuestOptions.ItemToFindBlueprintIds);
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

}

*/
}
