using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
public class Slow : OffensiveSpellBase
{
    private const string SpellName = "Slow";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }
    private IDispelManager DispelManager { get; }

    public Slow(ILogger<Slow> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("Slow")
            || Victim.GetAura(SpellName) != null)
        {
            if (Victim == Caster)
                Caster.Send("You can't move any slower!");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} can't get any slower than that.", Victim);
            return;
        }

        if (Victim.Immunities.IsSet("Magic")
            || Victim.SavesSpell(Level, SchoolTypes.Other))
        {
            if (Victim != Caster)
                Caster.Send("Nothing seemed to happen.");
            Victim.Send("You feel momentarily lethargic.");
            return;
        }

        if (Victim.CharacterFlags.IsSet("Haste"))
        {
            if (DispelManager.TryDispel(Level, Victim, "Haste") != TryDispelReturnValues.Dispelled)
            {
                if (Victim != Caster)
                    Caster.Send("Spell failed.");
                Victim.Send("You feel momentarily slower.");
                return;
            }
            Victim.Act(ActOptions.ToRoom, "{0:N} is moving less quickly.", Victim);
            return;
        }

        int duration = Level / 2;
        int modifier = -1 - (Level >= 18 ? 1 : 0) - (Level >= 25 ? 1 : 0) - (Level >= 32 ? 1 : 0);
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Slow"), Operator = AffectOperators.Or });
        Victim.Recompute();
        Victim.Send("You feel yourself slowing d o w n...");
        Caster.Act(ActOptions.ToRoom, "{0} starts to move in slow motion.", Victim);
    }
}
