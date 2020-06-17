using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Quest;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Tests.Mocking
{
    internal class QuestManagerMock : IQuestManager
    {
        private readonly List<QuestBlueprint> _questBlueprints = new List<QuestBlueprint>();

        public IReadOnlyCollection<QuestBlueprint> QuestBlueprints => _questBlueprints;

        public QuestBlueprint GetQuestBlueprint(int id)
        {
            return _questBlueprints.FirstOrDefault(x => x.Id == id);
        }

        public void AddQuestBlueprint(QuestBlueprint blueprint)
        {
            _questBlueprints.Add(blueprint);
        }
    }
}
