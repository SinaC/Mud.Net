using Mud.Domain;
using Mud.Blueprints.Character;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;
using Mud.Domain.SerializationData.Avatar;

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

    bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted);

    // Pet/charmies
    IPlayableCharacter? Master { get; } // character allowed to order us
    void ChangeMaster(IPlayableCharacter? master);
    bool Order(string commandLine);

    //
    bool CastSpell(string spellName, IEntity target);

    // Mapping
    PetData MapPetData();
}
