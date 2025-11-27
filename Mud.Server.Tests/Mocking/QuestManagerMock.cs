using Mud.Domain;
using Mud.Server.Blueprints.Quest;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Quest;

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

        public IQuest AddQuest(QuestBlueprint questBlueprint, IPlayableCharacter pc, INonPlayableCharacter questGiver)
        {
            throw new NotImplementedException();
        }

        public IQuest AddQuest(CurrentQuestData questData, IPlayableCharacter pc)
        {
            throw new NotImplementedException();
        }
    }
}
