using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Class;
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
public class Elf : PlayableRaceBase
{
    public Elf(ILogger<Elf> logger, IServiceProvider serviceProvider, IAbilityManager abilityManager)
        : base(logger, serviceProvider, abilityManager)
    {
        AddAbility("Sneak");
        AddAbility("Hide");
    }

    #region IRace

    public override string Name => "elf";
    public override string ShortName => "Elf";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);

    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider, "Charm");
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider, "Iron");

    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);

    public override int GetStartAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 12;
            case CharacterAttributes.Intelligence: return 14;
            case CharacterAttributes.Wisdom: return 13;
            case CharacterAttributes.Dexterity: return 15;
            case CharacterAttributes.Constitution: return 11;
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
            case CharacterAttributes.Strength: return 16;
            case CharacterAttributes.Intelligence: return 20;
            case CharacterAttributes.Wisdom: return 18;
            case CharacterAttributes.Dexterity: return 21;
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
                Logger.LogError("Unexpected max attribute {attribute} for {name}", attribute, Name);
                return 0;
        }
    }

    public override int ClassExperiencePercentageMultiplier(IClass c) => c is Classes.Cleric ? 125 : 100;

    #endregion
}
