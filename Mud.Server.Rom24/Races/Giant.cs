using Microsoft.Extensions.Logging;
using Mud.Common.Attributes;
using Mud.Domain;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
using Mud.Server.Interfaces.Race;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races;

[Help(
@"Giants are the largest of the races, ranging from 9-12 feet in height.  They
are stronger than any other race, and almost as durable as the dwarves.  
They aren't too bright, however, and their huge size makes them more clumsy
than the other races.  Giants make the best warriors of any race, but are
ill-suited for any other profession.

Giants resist heat and cold with nary a mark, due to their huge mass.  However,
their slow minds make them extremely vulnerable to mental attacks.  Giants,
due to their size and stamina, receive the fast healing and bash skills for
free. (Only giant warriors receive bash).")]
[Export(typeof(IRace)), Shared]
public class Giant : PlayableRaceBase
{
    public Giant(ILogger<Giant> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddNaturalAbility("bash");
        AddNaturalAbility("fast healing");
    }

    #region IRace/IPlayableRace

    public override string Name => "giant";
    public override string ShortName => "Gia";

    public override Sizes Size => Sizes.Large;

    public override ICharacterFlags CharacterFlags => new CharacterFlags();

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Fire", "Cold");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Mental", "Lightning");

    public override IBodyForms BodyForms => new BodyForms( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override int CreationPointsStartValue => 6;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 16;
            case BasicAttributes.Intelligence: return 11;
            case BasicAttributes.Wisdom: return 13;
            case BasicAttributes.Dexterity: return 11;
            case BasicAttributes.Constitution: return 14;
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
            case BasicAttributes.Intelligence: return 15;
            case BasicAttributes.Wisdom: return 18;
            case BasicAttributes.Dexterity: return 15;
            case BasicAttributes.Constitution: return 20;
            default:
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass? c)
        => c switch
        {
            Classes.Mage => 200,
            Classes.Cleric => 150,
            Classes.Thief => 150,
            Classes.Warrior => 105,
            _ => 100,
        };

    #endregion
}
