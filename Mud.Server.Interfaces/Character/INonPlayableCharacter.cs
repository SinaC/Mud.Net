using Mud.Domain;
using Mud.Server.Blueprints.Character;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Interfaces.Character;

public interface INonPlayableCharacter : ICharacter
{
    CharacterBlueprintBase Blueprint { get; }

    string DamageNoun { get; }
    SchoolTypes DamageType { get; }
    int DamageDiceCount { get; }
    int DamageDiceValue { get; }
    int DamageDiceBonus { get; }

    IActFlags ActFlags { get; }
    IOffensiveFlags OffensiveFlags { get; }
    IAssistFlags AssistFlags { get; }

    bool IsQuestObjective(IPlayableCharacter questingCharacter);

    // Pet/charmies
    IPlayableCharacter? Master { get; } // character allowed to order us
    void ChangeMaster(IPlayableCharacter? master);
    bool Order(string commandLine);

    // Mapping
    PetData MapPetData();
}
