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

[Spell(SpellName, AbilityEffects.Enchantment)]
[AbilityItemWearOffMessage("{0:N}'s protective aura fades.")]
public class Fireproof : ItemInventorySpellBase
{
    private const string SpellName = "Fireproof";

    private IAuraManager AuraManager { get; }

    public Fireproof(IRandomManager randomManager, IAuraManager auraManager)
        : base(randomManager)
    {
        AuraManager = auraManager;
    }

    protected override void Invoke()
    {
        if (Item.ItemFlags.IsSet("BurnProof"))
        {
            Caster.Act(ActOptions.ToCharacter, "{0:N} is already protected from burning.", Item);
            return;
        }

        int duration = RandomManager.Fuzzy(Level / 4);
        AuraManager.AddAura(Item, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
            new ItemFlagsAffect { Modifier = new ItemFlags("BurnProof"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", Item);
        Caster.Act(ActOptions.ToRoom, "{0:N} is surrounded by a protective aura.", Item);
    }
}
