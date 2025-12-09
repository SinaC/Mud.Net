using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Dispel)]
[Syntax("cast [spell] <character>")]
[Help(
@"Dispel magic removes magical effects from the target, has a reduced chance
of working, and is considering an attack spell.
Cancellation can only be used on allies, but is much more effective and does
not provoke attack.  Unfortunately, the spells do not discriminate between
harmful and benign spells.
 
The chance of dispelling is based on the level of the spell. Permanent spells
(such as mobile sanctuary) are much harder to remove.  Not all spells may
be dispelled, notable examples are poison and plague.")]
[OneLineHelp("removes spells from enemies, not as effective as cancellation")]
public class DispelMagic : OffensiveSpellBase
{
    private const string SpellName = "Dispel Magic";

    private IDispelManager DispelManager { get; }

    public DispelMagic(ILogger<DispelMagic> logger, IRandomManager randomManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        DispelManager = dispelManager;
    }

    protected override void Invoke()
    {
        if (Victim.IsSafeSpell(Caster, false))
        {
            Victim.Send("You feel a brief tingling sensation.");
            Caster.Send("You failed.");
            return;
        }

        bool found = DispelManager.TryDispels(Level, Victim);

        if (found)
            Caster.Send("Ok.");
        else
            Caster.Send("Spell failed.");
    }
}
