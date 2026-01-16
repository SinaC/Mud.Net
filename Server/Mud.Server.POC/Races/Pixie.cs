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
public class Pixie : PlayableRaceBase
{
    public Pixie(ILogger<Pixie> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
    }

    public override string Name => "pixie";
    public override string ShortName => "Pix";

    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => new CharacterFlags("Flying", "DetectMagic", "Infrared");

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Charm", "Mental");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Iron");

    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Light,
        Mud.Domain.EquipmentSlots.Head,
        Mud.Domain.EquipmentSlots.Amulet, // 2 amulets
        Mud.Domain.EquipmentSlots.Amulet,
        Mud.Domain.EquipmentSlots.Chest,
        Mud.Domain.EquipmentSlots.Cloak,
        Mud.Domain.EquipmentSlots.Waist,
        Mud.Domain.EquipmentSlots.Wrists, // 2 wrists
        Mud.Domain.EquipmentSlots.Wrists,
        Mud.Domain.EquipmentSlots.Arms,
        Mud.Domain.EquipmentSlots.Hands,
        Mud.Domain.EquipmentSlots.Ring, // 2 rings
        Mud.Domain.EquipmentSlots.Ring,
        Mud.Domain.EquipmentSlots.Legs,
        Mud.Domain.EquipmentSlots.Feet,
        Mud.Domain.EquipmentSlots.MainHand, // 2 hands
        Mud.Domain.EquipmentSlots.OffHand,
        Mud.Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => new BodyForms( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Body", "Arms", "Legs", "Heart", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Wings");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => false;
    public override int CreationPointsStartValue => 6;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 10;
            case BasicAttributes.Intelligence: return 15;
            case BasicAttributes.Wisdom: return 15;
            case BasicAttributes.Dexterity: return 15;
            case BasicAttributes.Constitution: return 10;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 14;
            case BasicAttributes.Intelligence: return 21;
            case BasicAttributes.Wisdom: return 21;
            case BasicAttributes.Dexterity: return 20;
            case BasicAttributes.Constitution: return 14;
            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass? c) => 125;
}
