using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.POC.Races;

[Export(typeof(IRace)), Shared]
public class Insectoid : PlayableRaceBase // 4-arms
{
    public Insectoid(ILogger<Insectoid> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
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
    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>();

    public override IEnumerable<EquipmentSlots> EquipmentSlots => _slots;

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Bash", "Slash", "Poison", "Disease", "Acid");
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Pierce", "Fire", "Cold");

    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>("Poison", "Sentient", "Biped", "Insect", "FourArms");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>("Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>("Fast");
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

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
