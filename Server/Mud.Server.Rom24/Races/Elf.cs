using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Flags;
using Mud.Flags.Interfaces;
using Mud.Server.Ability.Interfaces;
using Mud.Server.Class.Interfaces;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Race;
using Mud.Server.Race.Interfaces;

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
    public Elf(ILogger<Elf> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddNaturalAbility("sneak");
        AddNaturalAbility("hide");
    }

    #region IRace/IPlayableRace

    public override string Name => "elf";
    public override string ShortName => "Elf";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => new CharacterFlags();

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Charm");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Iron");

    public override IBodyForms BodyForms => new BodyForms( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => true;
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
