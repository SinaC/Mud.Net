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
    public Giant(ILogger<Giant> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
    {
        AddAbility("Bash");
        AddAbility("Fast healing");
    }

    #region IRace/IPlayableRace

    public override string Name => "giant";
    public override string ShortName => "Gia";

    public override Sizes Size => Sizes.Large;

    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>();

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Fire", "Cold");
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Mental", "Lightning");

    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>();
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

    public override int GetStartAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 16;
            case CharacterAttributes.Intelligence: return 11;
            case CharacterAttributes.Wisdom: return 13;
            case CharacterAttributes.Dexterity: return 11;
            case CharacterAttributes.Constitution: return 14;
            case CharacterAttributes.MaxHitPoints: return 100;
            case CharacterAttributes.SavingThrow: return 0;
            case CharacterAttributes.HitRoll: return 0;
            case CharacterAttributes.DamRoll: return 0;
            case CharacterAttributes.MaxMovePoints: return 100;
            case CharacterAttributes.ArmorBash: return 100;
            case CharacterAttributes.ArmorPierce: return 100;
            case CharacterAttributes.ArmorSlash: return 100;
            case CharacterAttributes.ArmorExotic: return 100;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 22;
            case CharacterAttributes.Intelligence: return 15;
            case CharacterAttributes.Wisdom: return 18;
            case CharacterAttributes.Dexterity: return 15;
            case CharacterAttributes.Constitution: return 20;
            case CharacterAttributes.MaxHitPoints: return 100;
            case CharacterAttributes.SavingThrow: return 0;
            case CharacterAttributes.HitRoll: return 0;
            case CharacterAttributes.DamRoll: return 0;
            case CharacterAttributes.MaxMovePoints: return 100;
            case CharacterAttributes.ArmorBash: return 100;
            case CharacterAttributes.ArmorPierce: return 100;
            case CharacterAttributes.ArmorSlash: return 100;
            case CharacterAttributes.ArmorExotic: return 100;
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
