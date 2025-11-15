using Mud.Server.Blueprints.Quest;
using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Character;

[DataContract]
public class CharacterQuestorBlueprint : CharacterBlueprintBase
{
    [DataMember]
    public QuestBlueprint[] QuestBlueprints { get; set; } = default!;
}
