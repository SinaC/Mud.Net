using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff)]
public class ChangeSex : DefensiveSpellBase
{
    private const string SpellName = "Change Sex";

    private IAuraManager AuraManager { get; }
    public ChangeSex(ILogger<ChangeSex> logger, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Victim.GetAura(SpellName) != null)
        {
            if (Victim == Caster)
                Caster.Send("You've already been changed.");
            else
                Caster.Act(ActOptions.ToCharacter, "{0:N} has already had {0:s} sex changed.", Victim);
            return;
        }

        if (Victim.SavesSpell(Level, SchoolTypes.Other))
            return;

        var newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != Victim.Sex));
        AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(2 * Level), AuraFlags.None, true,
            new CharacterSexAffect { Value = newSex });
        Victim.Send("You feel different.");
        Victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like {0:m}self anymore...", Victim);
    }
}
