using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest;

[Export(typeof(IQuestManager)), Shared]
public class QuestManager : IQuestManager
{
    private ILogger<QuestManager> Logger { get; }
    private IServiceProvider ServiceProvider { get; }

    private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

    public QuestManager(ILogger<QuestManager> logger, IServiceProvider serviceProvider)
    {
        ServiceProvider = serviceProvider;
        Logger = logger;

        _questBlueprints = [];
    }

    public IReadOnlyCollection<QuestBlueprint> QuestBlueprints
        => _questBlueprints.Values.ToList().AsReadOnly();

    public QuestBlueprint? GetQuestBlueprint(int id)
    {
        _questBlueprints.TryGetValue(id, out var blueprint);
        return blueprint;
    }

    public void AddQuestBlueprint(QuestBlueprint blueprint)
    {
        if (_questBlueprints.ContainsKey(blueprint.Id))
            Logger.LogError("Quest blueprint duplicate {blueprintId}!!!", blueprint.Id);
        else
            _questBlueprints.Add(blueprint.Id, blueprint);
    }

    public IQuest AddQuest(QuestBlueprint questBlueprint, IPlayableCharacter pc, INonPlayableCharacter questGiver)
    {
        var quest = ServiceProvider.GetRequiredService<IQuest>();
        quest.Initialize(questBlueprint, pc, questGiver);
        pc.AddQuest(quest);

        return quest;
    }

    public IQuest AddQuest(CurrentQuestData questData, IPlayableCharacter pc)
    {
        var quest = ServiceProvider.GetRequiredService<IQuest>();
        quest.Initialize(questData, pc);
        pc.AddQuest(quest);

        return quest;
    }
}
