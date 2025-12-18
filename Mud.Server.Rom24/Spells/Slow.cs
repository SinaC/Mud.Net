using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[Syntax("cast [spell] <target>")]
[Help(
@"Despite popular mythology, slow is not the opposite of haste, but is a spell
with it's own unique set of effects.  When cast on an unfortunate victim,
it slows its movements, making it easier to hit and reducing its rate
of attack.  The effect of slow also double movement costs and halve healing
rates, due to reduced metabolism.")]
[OneLineHelp("slows your enemies down, reducing their rate of attack")]
public class Slow : OffensiveSpellBase
{
    private const string SpellName = "Slow";

    private IAuraManager AuraManager { get; }
    private IDispelManager DispelManager { get; }

    public Slow(ILogger<Slow> logger, IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
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

        var duration = Level / 2;
        var modifier = -1 - (Level >= 18 ? 1 : 0) - (Level >= 25 ? 1 : 0) - (Level >= 32 ? 1 : 0);
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Slow"), Operator = AffectOperators.Or });
        Victim.Recompute();
        Victim.Send("You feel yourself slowing d o w n...");
        Caster.Act(ActOptions.ToRoom, "{0} starts to move in slow motion.", Victim);
    }
}
