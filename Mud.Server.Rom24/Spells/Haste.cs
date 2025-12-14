using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
[AbilityCharacterWearOffMessage("You feel yourself slow down.")]
[AbilityDispellable("{0:N} is no longer moving so quickly.")]
[Syntax("cast [spell] <target>")]
[Help(
@"The haste spell increases the speed and agility of the recipient, allowing
an extra attack (or even a backstab) in combat, and improving evasive
abilities in combat.  However, it produces a great strain on the system,
such that recuperative abilities are halved.  Haste is capable of negating
the slow spell. (see 'help slow').")]
[OneLineHelp("doubles the speed of the target, but slows down healing")]
public class Haste : DefensiveSpellBase
{
    private const string SpellName = "Haste";

    private IAuraManager AuraManager { get; }
    private IDispelManager DispelManager { get; }

    public Haste(ILogger<Haste> logger, IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke()
    {
        if (Victim.CharacterFlags.IsSet("Haste")
            || Victim.GetAura(SpellName) != null
            || (Victim is INonPlayableCharacter npcVictim && npcVictim.OffensiveFlags.IsSet("Fast")))
        {
            if (Victim == Caster)
                Caster.Send("You can't move any faster!");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} is already moving as fast as {0:e} can.", Victim);
            return;
        }
        if (Victim.CharacterFlags.IsSet("Slow"))
        {
            if (DispelManager.TryDispel(Level, Victim, "Slow") != TryDispelReturnValues.Dispelled)
            {
                if (Victim != Caster)
                    Caster.Send("Spell failed.");
                Victim.Send("You feel momentarily faster.");
                return;
            }
            Victim.Act(ActOptions.ToRoom, "{0:N} is moving less slowly.", Victim);
            return;
        }
        var duration = Victim == Caster
            ? Level / 2
            : Level / 4;
        var modifier = 1 + (Level >= 18 ? 1 : 0) + (Level >= 25 ? 1 : 0) + (Level >= 32 ? 1 : 0);
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
            new CharacterFlagsAffect { Modifier = new CharacterFlags("Haste"), Operator = AffectOperators.Or });
        Victim.Send("You feel yourself moving more quickly.");
        Victim.Act(ActOptions.ToRoom, "{0:N} is moving more quickly.", Victim);
        if (Caster != Victim)
            Caster.Send("Ok.");
    }
}
