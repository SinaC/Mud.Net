using Mud.Domain;
using Mud.Server.Blueprints.Character;

namespace Mud.Server
{
    public interface INonPlayableCharacter : ICharacter
    {
        CharacterBlueprintBase Blueprint { get; }

        string DamageNoun { get; }
        SchoolTypes DamageType { get; }
        int DamageDiceCount { get; }
        int DamageDiceValue { get; }
        int DamageDiceBonus { get; }

        ActFlags ActFlags { get; }

        OffensiveFlags OffensiveFlags { get; }

        bool IsQuestObjective(IPlayableCharacter questingCharacter);
    }
}
