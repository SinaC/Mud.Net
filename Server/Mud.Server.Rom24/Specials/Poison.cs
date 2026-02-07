using Mud.Domain;
using Mud.Flags;
using Mud.Random;
using Mud.Server.Affects.Character;
using Mud.Server.Domain;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Rom24.Affects;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_poison")]
public class Poison : ISpecialBehavior
{
    private IAuraManager AuraManager { get; }
    private IRandomManager RandomManager { get; }

    public Poison(IAuraManager auraManager, IRandomManager randomManager)
    {
        AuraManager = auraManager;
        RandomManager = randomManager;
    }


    // similar to poison spell
    public bool Execute(INonPlayableCharacter npc)
    {
        if (!npc.IsValid || npc.Room == null
            || npc.Position <= Positions.Sleeping || npc.Fighting == null)
            return false;

        var level = npc.Level;

        if (RandomManager.Next(100) > 2 * level)
            return false;

        // npc bites victim
        var victim = npc.Fighting!;
        npc.Act(ActOptions.ToCharacter, "You bite {0:N}!", victim);
        npc.ActToNotVictim(victim, "{0:N} bites {1:N}!", npc, victim);
        victim.Act(ActOptions.ToCharacter, "{0:N} bites you!", npc);

        // saves spell
        if (victim.SavesSpell(level, SchoolTypes.Poison))
        {
            victim.Act(ActOptions.ToRoom, "{0:N} turns slightly green, but it passes.", victim);
            victim.Send("You feel momentarily ill, but it passes.");
            return false;
        }

        // apply poison
        var duration = level;
        var poisonAura = victim.GetAura("Poison");
        if (poisonAura != null)
            poisonAura.Update(level, TimeSpan.FromMinutes(duration));
        else
            AuraManager.AddAura(victim, "Poison", npc, level, TimeSpan.FromMinutes(duration), true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -2, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = new CharacterFlags("Poison"), Operator = AffectOperators.Or },
                new PoisonDamageAffect());
        victim.Send("You feel very sick.");
        victim.Act(ActOptions.ToRoom, "{0:N} looks very ill.", victim);

        return true;
    }
}
