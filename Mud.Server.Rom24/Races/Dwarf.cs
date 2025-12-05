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
@"Dwarves are short, stocky demi-humans, known for foul temper and great
stamina.  Dwarves have high strength and constitution, but poor dexterity.
They are not as smart as humans, but are usually wiser due to their long 
lifespans.  Dwarves make excellent fighters and priests, but are very poor
mages or thieves.  

Dwarves are very resistant to poison and disease, but cannot swim, and so
are very vulnerable to drowning.  They recieve the berserk skill for free
(if warriors), and can see in the dark with infravision.")]

[Export(typeof(IRace)), Shared]
public class Dwarf : PlayableRaceBase
{
    public Dwarf(ILogger<Dwarf> logger, IFlagFactory flagFactory, IAbilityManager abilityManager)
        : base(logger, flagFactory, abilityManager)
    {
        AddAbility(1, "berserk", ResourceKinds.Mana, 50, CostAmountOperators.Fixed, 5);
    }

    #region IRace/IPlayableRace

    public override string Name => "dwarf";
    public override string ShortName => "Dwa";

    public override Sizes Size => Sizes.Small;

    public override ICharacterFlags CharacterFlags => FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>("Infrared");

    public override IIRVFlags Immunities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>();
    public override IIRVFlags Resistances => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Poison", "Disease");
    public override IIRVFlags Vulnerabilities => FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Drowning");

    public override IBodyForms BodyForms => FlagFactory.CreateInstance<IBodyForms, IBodyFormValues>( "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => FlagFactory.CreateInstance<IBodyParts, IBodyPartValues>( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => FlagFactory.CreateInstance<IActFlags, IActFlagValues>();
    public override IOffensiveFlags OffensiveFlags => FlagFactory.CreateInstance<IOffensiveFlags, IOffensiveFlagValues>();
    public override IAssistFlags AssistFlags => FlagFactory.CreateInstance<IAssistFlags, IAssistFlagValues>();

    public override int GetStartAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 14;
            case CharacterAttributes.Intelligence: return 12;
            case CharacterAttributes.Wisdom: return 14;
            case CharacterAttributes.Dexterity: return 10;
            case CharacterAttributes.Constitution: return 15;
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
            case CharacterAttributes.Strength: return 20;
            case CharacterAttributes.Intelligence: return 16;
            case CharacterAttributes.Wisdom: return 19;
            case CharacterAttributes.Dexterity: return 14;
            case CharacterAttributes.Constitution: return 21;
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
            Classes.Mage => 150,
            Classes.Cleric => 100,
            Classes.Thief => 125,
            Classes.Warrior => 100,
            _ => 100,
        };

    #endregion
}
