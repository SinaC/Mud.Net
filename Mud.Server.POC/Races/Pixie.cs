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
public class Pixie : PlayableRaceBase
{
    public Pixie(ILogger<Pixie> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
    {
    }

    public override string Name => "pixie";
    public override string ShortName => "Pix";

    public override Sizes Size => Sizes.Tiny;
    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>("Flying", "DetectMagic", "Infrared");

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Charm", "Mental");
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Iron");

    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Domain.EquipmentSlots.Light,
        Domain.EquipmentSlots.Head,
        Domain.EquipmentSlots.Amulet, // 2 amulets
        Domain.EquipmentSlots.Amulet,
        Domain.EquipmentSlots.Chest,
        Domain.EquipmentSlots.Cloak,
        Domain.EquipmentSlots.Waist,
        Domain.EquipmentSlots.Wrists, // 2 wrists
        Domain.EquipmentSlots.Wrists,
        Domain.EquipmentSlots.Arms,
        Domain.EquipmentSlots.Hands,
        Domain.EquipmentSlots.Ring, // 2 rings
        Domain.EquipmentSlots.Ring,
        Domain.EquipmentSlots.Legs,
        Domain.EquipmentSlots.Feet,
        Domain.EquipmentSlots.MainHand, // 2 hands
        Domain.EquipmentSlots.OffHand,
        Domain.EquipmentSlots.Float,
    ];
    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>( "Head", "Body", "Arms", "Legs", "Heart", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Wings");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>();
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

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
