using Mud.Domain;
using Mud.Server.Blueprints.Character;

namespace Mud.Server
{
    public interface INonPlayableCharacter : ICharacter
    {
        CharacterBlueprintBase Blueprint { get; }

        ActFlags ActFlags { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);
    }
}
