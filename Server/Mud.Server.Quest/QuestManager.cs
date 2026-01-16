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

    public IPredefinedQuest? AddQuest(ActiveQuestData questData, IPlayableCharacter pc)
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

    public ICompletedQuest? AddCompletedQuest(CompletedQuestData questData, IPlayableCharacter pc)
    {
        var questBlueprint = _questBlueprints.GetValueOrDefault(questData.QuestId);
        if (questBlueprint == null)
            Logger.LogError("Complete quest blueprint id {blueprintId} not found!!!", questData.QuestId);
        var completedQuest = new CompletedQuest
        {
            QuestId = questData.QuestId,
            QuestBlueprint = questBlueprint,
            StartTime = questData.StartTime,
            CompletionTime = questData.CompletionTime,
        };
        pc.AddCompletedQuest(completedQuest);
        return completedQuest;
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
}
