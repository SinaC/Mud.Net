using System.Runtime.Serialization;
using Mud.Server.Blueprints.Quest;

namespace Mud.Server.Blueprints.Character
{
    [DataContract]
    public class CharacterQuestorBlueprint : CharacterBlueprintBase
    {
        [DataMember]
        public QuestBlueprint[] QuestBlueprints { get; set; }
    }
}
