using Mud.Blueprints.Quest;
using System.Runtime.Serialization;

namespace Mud.Blueprints.Character;

[DataContract]
public class CharacterQuestorBlueprint : CharacterBlueprintBase
{
    [DataMember]
    public QuestBlueprint[] QuestBlueprints { get; set; } = default!;
}
