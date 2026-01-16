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
    public Dwarf(ILogger<Dwarf> logger, IAbilityManager abilityManager)
        : base(logger, abilityManager)
    {
        AddNaturalAbility(1, "berserk", [(ResourceKinds.Mana, 50, CostAmountOperators.Fixed), (ResourceKinds.MovePoints, 50, CostAmountOperators.PercentageCurrent)], 5);
    }

    #region IRace/IPlayableRace

    public override string Name => "dwarf";
    public override string ShortName => "Dwa";

    public override Sizes Size => Sizes.Small;

    public override ICharacterFlags CharacterFlags => new CharacterFlags("Infrared");

    public override IIRVFlags Immunities => new IRVFlags();
    public override IIRVFlags Resistances => new IRVFlags("Poison", "Disease");
    public override IIRVFlags Vulnerabilities => new IRVFlags("Drowning");

    public override IBodyForms BodyForms => new BodyForms("Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts( "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags();
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags();
    public override IAssistFlags AssistFlags => new AssistFlags();

    public override bool SelectableDuringCreation => true;
    public override int CreationPointsStartValue => 8;

    public override int GetStartAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 14;
            case BasicAttributes.Intelligence: return 12;
            case BasicAttributes.Wisdom: return 14;
            case BasicAttributes.Dexterity: return 10;
            case BasicAttributes.Constitution: return 15;
            default:
                Logger.LogError("Unexpected start attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int GetMaxAttribute(BasicAttributes attribute)
    {
        switch (attribute)
        {
            case BasicAttributes.Strength: return 20;
            case BasicAttributes.Intelligence: return 16;
            case BasicAttributes.Wisdom: return 19;
            case BasicAttributes.Dexterity: return 14;
            case BasicAttributes.Constitution: return 21;
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
