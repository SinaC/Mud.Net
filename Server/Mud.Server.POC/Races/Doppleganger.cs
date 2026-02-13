using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

namespace Mud.Server.POC.Races;

[Export(typeof(IRace)), Shared]
public class Doppleganger : PlayableRaceBase
{
    public Doppleganger(ILogger<Insectoid> logger, IAbilityManager abilityManager)
       : base(logger, abilityManager)
    {
        AddNaturalAbility("Morph", 0);
    }

    #region IRace

    public override IEnumerable<EquipmentSlots> EquipmentSlots =>
    [
        Mud.Domain.EquipmentSlots.Float,
    ];

    public override string Name => "doppleganger";
    public override string ShortName => "Dop";

    public override Sizes Size => Sizes.Medium;
    public override ICharacterFlags CharacterFlags => new CharacterFlags();

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();

    public override IBodyForms BodyForms => new BodyForms("magical", "sentient");
    public override IBodyParts BodyParts => new BodyParts();

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => false;
    public override int CreationPointsStartValue => 20;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 10;
            case BasicAttributes.Intelligence: return 10;
            case BasicAttributes.Wisdom: return 10;
            case BasicAttributes.Dexterity: return 10;
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
            case BasicAttributes.Strength: return 18;
            case BasicAttributes.Intelligence: return 18;
            case BasicAttributes.Wisdom: return 18;
            case BasicAttributes.Dexterity: return 18;
            case BasicAttributes.Constitution: return 18;
            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass? c) => 250;

    #endregion
}
