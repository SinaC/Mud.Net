using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common;
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
    public Human(ILogger<Human> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
    {
    }

    #region IRace/IPlayableRace

    public override string Name => "human";
    public override string ShortName => "Hum";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>();

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();

    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>();
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

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
