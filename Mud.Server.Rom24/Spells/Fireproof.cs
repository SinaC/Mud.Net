using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Item;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Enchantment)]
[AbilityItemWearOffMessage("{0:N}'s protective aura fades.")]
[Syntax("cast [spell] <object>")]
[Help(
@"The fireproof spell creates a short-lived protective aura around an object,
to protect it from the harmful effects of acid and flame.  Items protected
by this spell are not harmed by acid, fire, or the heat metal spell.
Although inexpensive to use, the spell's short duration makes it impractical
for protecting large numbers of objects.")]
[OneLineHelp("shields items from the harmful effects of fire and acid")]
public class Fireproof : ItemInventorySpellBase
{
    private const string SpellName = "Fireproof";

    private IFlagFactory<IItemFlags, IItemFlagValues> ItemFlagFactory { get; }
    private IAuraManager AuraManager { get; }

    public Fireproof(ILogger<Fireproof> logger, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory, IRandomManager randomManager, IAuraManager auraManager)
        : base(logger, randomManager)
    {
        ItemFlagFactory = itemFlagFactory;
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
            new ItemFlagsAffect { Modifier = ItemFlagFactory.CreateInstance("BurnProof"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToCharacter, "You protect {0:N} from fire.", Item);
        Caster.Act(ActOptions.ToRoom, "{0:N} is surrounded by a protective aura.", Item);
    }
}
