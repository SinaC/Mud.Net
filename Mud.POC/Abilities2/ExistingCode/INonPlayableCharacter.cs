using Mud.POC.Abilities2.Domain;
using Mud.Server.Blueprints.Character;

namespace Mud.POC.Abilities2.ExistingCode
{
    public interface INonPlayableCharacter : ICharacter
    {
        CharacterBlueprintBase Blueprint { get; }

        IPlayableCharacter Master { get; }

        ActFlags ActFlags { get; }
        OffensiveFlags OffensiveFlags { get; }
    }
}
