using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races;

[Help(
@"Elves are slightly taller than humans, but have a much lighter build.  They
lack the strength and stamina of the other races, but are for more agile,
both in body and mind.  Elves are superb mages and thieves, but have at
best fair talent as warriors or priests.

Elves resist charm spells most effectively, due to their magical nature.
However, they are burned by the touch of iron, and so are barred from the
use of iron or steel in their adventuring careers.  Elves are notoriously 
hard to spot, and so elven warriors and thieves recieve the sneak and hiding
automatically. They may see in the dark with infravision.")]
[Export(typeof(IRace)), Shared]
public class Elf : PlayableRaceBase
{
    public Elf(ILogger<Elf> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
    {
        AddNaturalAbility("sneak");
        AddNaturalAbility("hide");
    }

    #region IRace/IPlayableRace

    public override string Name => "elf";
    public override string ShortName => "Elf";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>();

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Charm");
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Iron");

    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>();
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

    public override int CreationPointsStartValue => 5;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 12;
            case BasicAttributes.Intelligence: return 14;
            case BasicAttributes.Wisdom: return 13;
            case BasicAttributes.Dexterity: return 15;
            case BasicAttributes.Constitution: return 11;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 16;
            case BasicAttributes.Intelligence: return 20;
            case BasicAttributes.Wisdom: return 18;
            case BasicAttributes.Dexterity: return 21;
            case BasicAttributes.Constitution: return 15;

            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass? c)
        => c switch
        {
            Classes.Mage => 100,
            Classes.Cleric => 125,
            Classes.Thief => 100,
            Classes.Warrior => 120,
            _ => 100,
        };

    #endregion
}
