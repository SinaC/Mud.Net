using Mud.Domain;
using Mud.Domain.SerializationData;
using Mud.Server.Blueprints.Character;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;

namespace Mud.Server.Interfaces.Character;

public interface INonPlayableCharacter : ICharacter
{
    void Initialize(Guid guid, CharacterBlueprintBase blueprint, IRoom room); // NPC
    void Initialize(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room); // Pet

    CharacterBlueprintBase Blueprint { get; }

    string DamageNoun { get; }
    SchoolTypes DamageType { get; }
    int DamageDiceCount { get; }
    int DamageDiceValue { get; }
    int DamageDiceBonus { get; }

    IActFlags ActFlags { get; }
    IOffensiveFlags OffensiveFlags { get; }
    IAssistFlags AssistFlags { get; }

    // Special behavior
    ISpecialBehavior? SpecialBehavior { get; }

    bool IsQuestObjective(IPlayableCharacter questingCharacter);

    // Pet/charmies
    IPlayableCharacter? Master { get; } // character allowed to order us
    void ChangeMaster(IPlayableCharacter? master);
    bool Order(string commandLine);

    // Mapping
    PetData MapPetData();
}
