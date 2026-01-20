using Mud.Domain;
using Mud.Server.Domain;
using Mud.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect.Character;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Combat;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Race;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Interfaces.Character;

public enum ActOptions
{
    /// <summary>
    /// Everyone in the room except Character
    /// </summary>
    ToRoom,
    /// <summary>
    /// Only to Character
    /// </summary>
    ToCharacter,
    /// <summary>
    /// Everyone in the room
    /// </summary>
    ToAll,
    /// <summary>
    /// Everyone in the group
    /// </summary>
    ToGroup
}

public interface ICharacter : IEntity, IContainer
{
    IRoom Room { get; }
    ICharacter? Fighting { get; }
    ImmortalModeFlags ImmortalMode { get; }

    IEnumerable<IEquippedItem> Equipments { get; }
    IEnumerable<IItem> Inventory { get; } // same as IContainer.Content
    int MaxCarryWeight { get; }
    int MaxCarryNumber { get; }
    int CarryWeight { get; }
    int CarryNumber { get; }

    int GlobalCooldown { get; } // delay (in Pulse) before next manual action
    void DecreaseGlobalCooldown(); // decrease one by one
    void SetGlobalCooldown(int pulseCount); // set global cooldown delay (in pulse), can only increase

    int Daze { get; } // delay (in Pulse) before automatic and manual action
    void DecreaseDaze(); // decrease one by one
    void SetDaze(int pulseCount); // set daze delay (in pulse), can only increase

    // Money
    long SilverCoins { get; }
    long GoldCoins { get; }
    (long silverSpent, long goldSpent) DeductCost(long cost);
    void UpdateMoney(long silverCoins, long goldCoins);
    (long stolenSilver, long stolenGold) StealMoney(long silverCoins, long goldCoins);

    // Furniture (sleep/sit/stand)
    IItemFurniture? Furniture { get; }

    // Position
    Positions Position { get; }
    int Stunned { get; }

    // Class/Race
    IClass Class { get; }
    IRace Race { get; }

    // Attributes/Resources/Flags
    int Level { get; }

    ICharacterFlags BaseCharacterFlags { get; }
    ICharacterFlags CharacterFlags { get; }

    IIRVFlags BaseImmunities { get; }
    IIRVFlags Immunities { get; }

    IIRVFlags BaseResistances { get; }
    IIRVFlags Resistances { get; }

    IIRVFlags BaseVulnerabilities { get; }
    IIRVFlags Vulnerabilities { get; }

    IShieldFlags BaseShieldFlags { get; }
    IShieldFlags ShieldFlags { get; }

    Sex BaseSex { get; }
    Sex Sex { get; }

    Sizes BaseSize { get; }
    Sizes Size { get; }

    int Alignment { get; }
    bool IsEvil { get; }
    bool IsGood { get; }
    bool IsNeutral { get; }

    int this[CharacterAttributes attribute] { get; }
    int this[BasicAttributes attribute] { get; }
    int this[Armors armor] { get; }
    int HitRoll { get; }
    int DamRoll { get; }

    int this[ResourceKinds resource] { get; }
    IEnumerable<ResourceKinds> CurrentResourceKinds { get; }

    IBodyForms BaseBodyForms { get; }
    IBodyForms BodyForms { get; }
    IBodyParts BaseBodyParts { get; }
    IBodyParts BodyParts { get; }

    // Shape
    Shapes BaseShape { get; }
    Shapes Shape { get; }

    // Abilities
    IEnumerable<IAbilityLearned> LearnedAbilities { get; }

    // Followers
    ICharacter? Leader { get; } // character we are following, different from group leader
    void AddFollower(ICharacter character);
    void RemoveFollower(ICharacter character);
    void ChangeLeader(ICharacter? character);

    // Group
    IEnumerable<IPlayableCharacter> GetPlayableCharactersImpactedByKill();
    bool IsSameGroupOrPet(ICharacter character);

    // Act
    void Act(ActOptions option, string format, params object[] arguments);
    void Act(ActOptions option, Positions minPosition, string format, params object[] arguments);
    void ActToNotVictim(ICharacter victim, string format, params object[] arguments); // to everyone except this and victim
    void ActToNotVictim(ICharacter victim, Positions minPosition, string format, params object[] arguments); // to everyone except this and victim
    string ActPhrase(string format, params object[] arguments);

    // Equipments
    bool Unequip(IItem item);
    bool Equip(IItem item);

    // Furniture
    bool ChangeFurniture(IItemFurniture? furniture);

    // Position
    bool StandUpInCombatIfPossible();
    bool ChangePosition(Positions position);
    void DisplayChangePositionMessage(Positions oldPosition, Positions newPosition, IItemFurniture? furniture);

    // Stunned
    void ChangeStunned(int stunned);

    // Visibility
    bool CanSee(ICharacter? victim);
    bool CanSee(IItem? item);
    bool CanSee(IExit? exit);
    bool CanSee(IRoom? room);

    // Loot
    bool CanLoot(IItem? target);

    // Attributes
    int BaseAttribute(CharacterAttributes attribute);
    void UpdateBaseAttribute(CharacterAttributes attribute, int amount);

    // Resources
    int MaxResource(ResourceKinds resourceKind);
    int BaseMaxResource(ResourceKinds resourceKind);
    void SetBaseMaxResource(ResourceKinds resourceKind, int value);
    void UpdateBaseMaxResource(ResourceKinds resourceKind, int amount);
    void SetResource(ResourceKinds resourceKind, int value);
    void UpdateResource(ResourceKinds resourceKind, decimal amount);
    void Regen(int pulseCount);

    // Alignment
    void UpdateAlignment(int amount);
    bool ZapWornItemIfNeeded(IItem item);

    // Character flags
    void AddBaseCharacterFlags(bool recompute, params string[] characterFlags);
    void RemoveBaseCharacterFlags(bool recompute, params string[] characterFlags);

    // Shape
    bool ChangeShape(Shapes shape);

    // Move
    bool Move(ExitDirections direction, bool following, bool forceFollowers);
    bool Enter(IItemPortal portal, bool following, bool forceFollowers);
    void ChangeRoom(IRoom destination, bool recompute);

    // Combat
    bool StartFighting(ICharacter victim);
    bool StopFighting(bool both); // if both is true, every character fighting 'this' stop fighting
    void MultiHit(ICharacter? victim); // 'this' starts a combat with 'victim'
    void MultiHit(ICharacter? victim, IMultiHitModifier? multiHitModifier); // 'this' starts a combat with 'victim' and has been initiated by an ability
    DamageResults AbilityDamage(ICharacter source, int damage, SchoolTypes damageType, string? damageNoun, bool display); // 'this' is dealt damage by 'source' using an ability
    DamageResults HitDamage(ICharacter source, IItemWeapon? wield, int damage, SchoolTypes damageType, string damageNoun, bool display); // 'this' is dealt damage by 'source' using a weapon
    void HandleAutoGold(IItemCorpse corpse);
    void HandleAutoLoot(IItemCorpse corpse);
    void HandleAutoSacrifice(IItemCorpse corpse);
    IItemCorpse? RawKilled(ICharacter? killer, bool payoff);
    bool SavesSpell(int level, SchoolTypes damageType);
    bool IsSafeSpell(ICharacter caster, bool area);
    string? IsSafe(ICharacter aggressor);
    bool Flee();
    void OnDamagePerformed(int damage, DamageSources damageSource);

    // Abilities
    (int percentage, IAbilityLearned? abilityLearned) GetWeaponLearnedAndPercentage(IItemWeapon? weapon);
    (int percentage, IAbilityLearned? abilityLearned) GetAbilityLearnedAndPercentage(string abilityName); // percentage is dynamically computed and can be different than abilityLearned.Learned
    IDictionary<string, int> AbilitiesInCooldown { get; }
    bool HasAbilitiesInCooldown { get; }
    int CooldownPulseLeft(string abilityName); // Return cooldown seconds left for an ability (Int.MinValue if was not in CD)
    void SetCooldown(string abilityName, TimeSpan timeSpan);
    bool DecreaseCooldown(string abilityName, int pulseCount); // return true if timed out
    void ResetCooldown(string abilityName, bool verbose);

    // Equipment
    IItem? GetEquipment(EquipmentSlots slot); // return item found in first non-empty specified slot
    T? GetEquipment<T>(EquipmentSlots slot) // return specific item found in first non-empty specified slot
        where T : IItem;
    IEquippedItem? SearchEquipmentSlot(IItem item, bool replace);

    // Misc
    bool GetItem(IItem item, IContainer container);

    // Display
    StringBuilder Append(StringBuilder sb, ICharacter viewer, bool peekInventory);
    StringBuilder AppendInRoom(StringBuilder sb, ICharacter viewer);

    // Affects
    void ApplyAffect(ICharacterFlagsAffect affect);
    void ApplyAffect(ICharacterIRVAffect affect);
    void ApplyAffect(ICharacterShieldFlagsAffect affect);
    void ApplyAffect(ICharacterAttributeAffect affect);
    void ApplyAffect(ICharacterSexAffect affect);
    void ApplyAffect(ICharacterSizeAffect affect);
    void ApplyAffect(ICharacterResourceAffect affect);

    //
    void OnRemoved(IRoom nullRoom);
}
