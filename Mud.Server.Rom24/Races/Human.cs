using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races;

[Help(
@"Humans are the most common race in the world, and make up the majority of
adventurers. Although they have no special talents like the other races,
they are more versitile, being skilled in all four classes. Humans may
also train their primary stat higher than any other race, and are able to
gain more benefit from magical devices.")]
[Export(typeof(IRace)), Shared]
public class Human : PlayableRaceBase
{
    public Human(ILogger<Human> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
    }

    #region IRace/IPlayableRace

    public override string Name => "human";
    public override string ShortName => "Hum";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => new CharacterFlags();

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags();
    public override IIRVFlags Vulnerabilities => new IRVFlags();

    public override IBodyForms BodyForms => new BodyForms( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => true;
    public override int CreationPointsStartValue => 0;

    public override bool EnhancedPrimeAttribute => true;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 13;
            case BasicAttributes.Intelligence: return 13;
            case BasicAttributes.Wisdom: return 13;
            case BasicAttributes.Dexterity: return 13;
            case BasicAttributes.Constitution: return 13;
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

    #endregion
}
