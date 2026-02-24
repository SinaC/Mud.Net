using Mud.Domain;
using Mud.Blueprints.Character;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Room;
using Mud.Server.Interfaces.Special;
using Mud.Domain.SerializationData.Avatar;
using Mud.Server.Interfaces.Item;
using Mud.Server.Domain;

namespace Mud.Server.Interfaces.Character;

public interface INonPlayableCharacter : ICharacter
{
    void Initialize(Guid guid, CharacterBlueprintBase blueprint, IRoom room); // NPC
    void Initialize(Guid guid, CharacterBlueprintBase blueprint, PetData petData, IRoom room); // Pet

    CharacterBlueprintBase Blueprint { get; }

    IRoom SpawnRoom { get; }

    string DamageNoun { get; }
    SchoolTypes DamageType { get; }
    int DamageDiceCount { get; }
    int DamageDiceValue { get; }
    int DamageDiceBonus { get; }

    IActFlags ActFlags { get; }
    IOffensiveFlags OffensiveFlags { get; }
    IAssistFlags AssistFlags { get; }

    bool IsAlive { get; }

    // Special behavior
    ISpecialBehavior? SpecialBehavior { get; }

    bool IsQuestObjective(IPlayableCharacter questingCharacter, bool checkCompleted);

    // Pet/charmies
    bool IsPetOrCharmie { get; }
    IPlayableCharacter? Master { get; } // character allowed to order us
    void ChangeMaster(IPlayableCharacter? master);
    bool Order(string commandLine);

    //
    bool CastSpell(string spellName, IEntity? target);

    // MobProgram
    int MobProgramDelay { get; }
    void ResetMobProgramDelay();
    void DecreaseMobProgramDelay();
    void SetMobProgramDelay(int pulseCount);
    ICharacter? MobProgramTarget { get; }
    void SetMobProgramTarget(ICharacter? target);
    int MobProgramDepth { get; }
    void IncreaseMobProgramDepth();
    void DecreaseMobProgramDepth();

    // MobProgram triggers
    bool OnAct(ICharacter triggerer, string text);
    bool OnBribe(ICharacter triggerer, long amount);
    bool OnGive(ICharacter triggerer, IItem item);
    bool OnSocial(ICharacter triggerer, SocialDefinition socialDefinition);
    bool OnSpeech(ICharacter triggerer, string text);
    bool OnEntry();
    bool OnGreet(ICharacter triggerer);
    bool OnExit(ICharacter triggerer, ExitDirections direction);
    bool OnKill(ICharacter triggerer);
    bool OnDeath(ICharacter? killer);
    bool OnFight(ICharacter fighting);
    bool OnHitPointPercentage(ICharacter fighting);
    bool OnRandom();
    bool OnDelay();

    // Mapping
    PetData MapPetData();
}
