using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Flags;
using Mud.Server.Flags.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Race;

namespace Mud.Server.Rom24.Races;

public class Human : PlayableRaceBase
{
    public Human(ILogger<Human> logger, IServiceProvider serviceProvider, IAbilityManager abilityManager)
        : base(logger, serviceProvider, abilityManager)
    {
    }

    #region IRace

    public override string Name => "human";
    public override string ShortName => "Hum";

    public override Sizes Size => Sizes.Medium;

    public override ICharacterFlags CharacterFlags => new CharacterFlags(ServiceProvider);

    public override IIRVFlags Immunities => new IRVFlags(ServiceProvider);
    public override IIRVFlags Resistances => new IRVFlags(ServiceProvider);
    public override IIRVFlags Vulnerabilities => new IRVFlags(ServiceProvider);

    public override IBodyForms BodyForms => new BodyForms(ServiceProvider, "Edible", "Sentient", "Biped", "Mammal");
    public override IBodyParts BodyParts => new BodyParts(ServiceProvider, "Head", "Arms", "Legs", "Head", "Brains", "Guts", "Hands", "Feet", "Fingers", "Ear", "Eye", "Body");

    public override IActFlags ActFlags => new ActFlags(ServiceProvider);
    public override IOffensiveFlags OffensiveFlags => new OffensiveFlags(ServiceProvider);
    public override IAssistFlags AssistFlags => new AssistFlags(ServiceProvider);

    public override int GetStartAttribute(CharacterAttributes attribute)
    {
        switch (attribute)
        {
            case CharacterAttributes.Strength: return 13;
            case CharacterAttributes.Intelligence: return 13;
            case CharacterAttributes.Wisdom: return 13;
            case CharacterAttributes.Dexterity: return 13;
            case CharacterAttributes.Constitution: return 13;
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
            case CharacterAttributes.Strength: return 18;
            case CharacterAttributes.Intelligence: return 18;
            case CharacterAttributes.Wisdom: return 18;
            case CharacterAttributes.Dexterity: return 18;
            case CharacterAttributes.Constitution: return 18;
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

    #endregion
}
