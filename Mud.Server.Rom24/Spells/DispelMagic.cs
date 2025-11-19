using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Dispel)]
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
        if (Victim.SavesSpell(Level, SchoolTypes.Other))
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
