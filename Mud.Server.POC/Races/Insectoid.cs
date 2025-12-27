using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.POC.Races;

[Export(typeof(IRace)), Shared]
public class Insectoid : PlayableRaceBase // 4-arms
{
    public Insectoid(ILogger<Insectoid> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddNaturalAbility(1, "Test", null, 0, CostAmountOperators.None, 0);
        AddNaturalAbility(1, "Dual Wield", null, 0, CostAmountOperators.None, 0);
        AddNaturalAbility(1, "Third Wield", null, 0, CostAmountOperators.None, 0); // only if warrior
        AddNaturalAbility(1, "Fourth Wield", null, 0, CostAmountOperators.None, 0); // only if warrior

        // Test race with all spells
        foreach (var abilityDefinition in AbilityManager.Abilities.Where(x => x.Type == AbilityTypes.Spell))
            AddNaturalAbility(1, abilityDefinition.Name, ResourceKinds.Mana, 5, CostAmountOperators.Percentage, 1);
    }

    #region IRace

    private readonly List<EquipmentSlots> _slots =
    [
        Mud.Domain.EquipmentSlots.Light,
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Amulet,
        Mud.Domain.EquipmentSlots.Amulet,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Cloak,
        Mud.Domain.EquipmentSlots.Waist,
        // --> 2 pair of hands
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Arms,
        Mud.Domain.EquipmentSlots.Arms,
        Mud.Domain.EquipmentSlots.Hands,
        Mud.Domain.EquipmentSlots.Hands,
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Ring,
        // <--
        Mud.Domain.EquipmentSlots.Legs,
        Mud.Domain.EquipmentSlots.Feet,
        // 4 hands
        Mud.Domain.EquipmentSlots.MainHand,
        Mud.Domain.EquipmentSlots.OffHand,
        Mud.Domain.EquipmentSlots.MainHand,
        Mud.Domain.EquipmentSlots.OffHand,
        // no float as malus
    ];

    public override string Name => "insectoid";
    public override string ShortName => "Ins";

    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();

    public override IEnumerable<EquipmentSlots> EquipmentSlots => _slots;

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Bash", "Slash", "Poison", "Disease", "Acid");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Pierce", "Fire", "Cold");

    public override IBodyForms BodyForms => new BodyForms("Poison", "Sentient", "Biped", "Insect", "FourArms");
    public override IBodyParts BodyParts => new BodyParts("Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags("Fast");
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => false;
    public override int CreationPointsStartValue =>  20;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 16;
            case BasicAttributes.Intelligence: return 16;
            case BasicAttributes.Wisdom: return 16;
            case BasicAttributes.Dexterity: return 16;
            case BasicAttributes.Constitution: return 16;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 22;
            case BasicAttributes.Intelligence: return 22;
            case BasicAttributes.Wisdom: return 22;
            case BasicAttributes.Dexterity: return 22;
            case BasicAttributes.Constitution: return 22;
            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass? c) => 250;

    #endregion
}
