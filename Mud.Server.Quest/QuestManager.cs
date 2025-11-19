using Microsoft.Extensions.Logging;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest;

public class QuestManager : IQuestManager
{
    private ILogger<QuestManager> Logger { get; }

    private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

    public QuestManager(ILogger<QuestManager> logger)
    {
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
}
