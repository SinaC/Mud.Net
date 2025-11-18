using Mud.Logger;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Quest;

namespace Mud.Server.Quest;

public class QuestManager : IQuestManager
{
    private readonly Dictionary<int, QuestBlueprint> _questBlueprints;

    public QuestManager()
    {
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
            Log.Default.WriteLine(LogLevels.Error, "Quest blueprint duplicate {0}!!!", blueprint.Id);
        else
            _questBlueprints.Add(blueprint.Id, blueprint);
    }
}
