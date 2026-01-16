using Microsoft.Extensions.Logging;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Common.Attributes;
using Mud.Server.Domain;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Dispel)]
[Help(
@"Cancellation removes magical effects from the target, can only be used on
allies, but is much more effective and does not provoke attack.
Unfortunately, the spells do not discriminate between harmful and benign spells.
 
The chance of dispelling is based on the level of the spell. Permanent spells
(such as mobile sanctuary) are much harder to remove.  Not all spells may
be dispelled, notable examples are poison and plague.")]
[OneLineHelp("a powerful dispel, used for removing spells from friends")]
public class Cancellation : DefensiveSpellBase
{
    private const string SpellName = "Cancellation";

    private IDispelManager DispelManager { get; }

    public Cancellation(ILogger<Cancellation> logger, IRandomManager randomManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        DispelManager = dispelManager;
    }

    protected override void Invoke()
    {
        if ((Caster is IPlayableCharacter && Victim is INonPlayableCharacter npcVictim && !Caster.CharacterFlags.IsSet("Charm") && npcVictim.Master == Caster)
            || (Caster is INonPlayableCharacter && Victim is IPlayableCharacter))
        {
            Caster.Send("You failed, try dispel magic.");
            return;
        }

        // unlike dispel magic, no save roll
        bool found = DispelManager.TryDispels(Level + 2, Victim);

        if (found)
            Caster.Send("Ok.");
        else
            Caster.Send("Spell failed.");
    }
}
