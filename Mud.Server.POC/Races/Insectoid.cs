using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Race;

namespace Mud.Server.POC.Races;

public class Insectoid : PlayableRaceBase // 4-arms
{
    public Insectoid(ILogger<Insectoid> logger, IServiceProvider serviceProvider, IAbilityManager abilityManager)
        : base(logger, serviceProvider, abilityManager)
    {
        AddAbility(1, "Test", null, 0, CostAmountOperators.None, 0);
        AddAbility(1, "Dual Wield", null, 0, CostAmountOperators.None, 0);
        AddAbility(1, "Third Wield", null, 0, CostAmountOperators.None, 0); // only if warrior
        AddAbility(1, "Fourth Wield", null, 0, CostAmountOperators.None, 0); // only if warrior

        // Test race with all spells
        foreach (var abilityInfo in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Spell))
            AddAbility(1, abilityInfo.Name, ResourceKinds.Mana, 5, CostAmountOperators.Percentage, 1);
    }

    #region IRace

    private readonly List<EquipmentSlots> _slots =
    [
        Domain.EquipmentSlots.Light,
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Amulet,
        Domain.EquipmentSlots.Amulet,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Cloak,
        Domain.EquipmentSlots.Waist,
        // --> 2 pair of hands
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Arms,
        Domain.EquipmentSlots.Arms,
        Domain.EquipmentSlots.Hands,
        Domain.EquipmentSlots.Hands,
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Ring,
        // <--
        Domain.EquipmentSlots.Legs,
        Domain.EquipmentSlots.Feet,
        // 4 hands
        Domain.EquipmentSlots.MainHand,
        Domain.EquipmentSlots.OffHand,
        Domain.EquipmentSlots.MainHand,
        Domain.EquipmentSlots.OffHand,
        // no float as malus
    ];

    public override string Name => "insectoid";
    public override string ShortName => "Ins";

    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider, "Haste");

    public override IEnumerable<EquipmentSlots> EquipmentSlots => _slots;

    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Bash", "Slash", "Poison", "Disease", "Acid");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Pierce", "Fire", "Cold");

    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Poison", "Sentient", "Biped", "Insect", "FourArms");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider, "Fast");
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);

    public override int GetStartAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 16;
            case CharacterAttributes.Intelligence: return 16;
            case CharacterAttributes.Wisdom: return 16;
            case CharacterAttributes.Dexterity: return 16;
            case CharacterAttributes.Constitution: return 16;
            case CharacterAttributes.MaxHitPoints: return 100;
            case CharacterAttributes.SavingThrow: return 0;
            case CharacterAttributes.HitRoll: return 0;
            case CharacterAttributes.DamRoll: return 0;
            case CharacterAttributes.MaxMovePoints: return 100;
            case CharacterAttributes.ArmorBash: return 100;
            case CharacterAttributes.ArmorPierce: return 100;
            case CharacterAttributes.ArmorSlash: return 100;
            case CharacterAttributes.ArmorExotic: return 100;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 22;
            case CharacterAttributes.Intelligence: return 22;
            case CharacterAttributes.Wisdom: return 22;
            case CharacterAttributes.Dexterity: return 22;
            case CharacterAttributes.Constitution: return 22;
            case CharacterAttributes.MaxHitPoints: return 100;
            case CharacterAttributes.SavingThrow: return 0;
            case CharacterAttributes.HitRoll: return 0;
            case CharacterAttributes.DamRoll: return 0;
            case CharacterAttributes.MaxMovePoints: return 100;
            case CharacterAttributes.ArmorBash: return 100;
            case CharacterAttributes.ArmorPierce: return 100;
            case CharacterAttributes.ArmorSlash: return 100;
            case CharacterAttributes.ArmorExotic: return 100;
            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass c) => 250;

    #endregion
}
