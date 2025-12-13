using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Common;
using Mud.Server.Flags.Interfaces;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Dispel)]
[AbilityCharacterWearOffMessage("You feel less righteous.")]
[AbilityItemWearOffMessage("{0}'s holy aura fades.")]
[Syntax(
    "cast [spell] <character>",
    "cast [spell] <object>")]
[Help(
@"This spell improves the to-hit roll and saving throw versus spell of the
target character by 1 for every 8 levels of the caster. It may also be
cast on an object to temporarily bless it (blessed weapons, for example,
are more effective against demonic beings).")]
[OneLineHelp("bestows divine favor upon the target")]
public class Bless : ItemOrDefensiveSpellBase
{
    private const string SpellName = "Bless";

    private IFlagFactory<IItemFlags, IItemFlagValues> ItemFlagFactory { get; }
    private IAuraManager AuraManager { get; }
    private IEffectManager EffectManager { get; }
    private IDispelManager DispelManager { get; }

    public Bless(ILogger<Bless> logger, IFlagFactory<IItemFlags, IItemFlagValues> itemFlagFactory, IRandomManager randomManager, IAuraManager auraManager, IEffectManager effectManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        ItemFlagFactory = itemFlagFactory;
        AuraManager = auraManager;
        EffectManager = effectManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        var effect = EffectManager.CreateInstance<ICharacter>("Bless");
       effect?.Apply(victim, Caster, SpellName, Level, 0);
    }

    protected override void Invoke(IItem item)
    {
        if (item.ItemFlags.IsSet("Bless"))
        {
            Caster.Act(ActOptions.ToCharacter, "{0:N} is already blessed.", item);
            return;
        }
        if (item.ItemFlags.IsSet("Evil"))
        {
            var evilAura = item.GetAura("Curse");
            if (!DispelManager.SavesDispel(Level, evilAura?.Level ?? item.Level, 0))
            {
                if (evilAura != null)
                    item.RemoveAura(evilAura, false);
                Caster.Act(ActOptions.ToAll, "{0} glows a pale blue.", item);
                item.RemoveBaseItemFlags(true, "Evil");
                return;
            }
            Caster.Act(ActOptions.ToCharacter, "The evil of {0} is too powerful for you to overcome.", item);
            return;
        }
        AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(6 + Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = -1, Operator = AffectOperators.Add },
            new ItemFlagsAffect(ItemFlagFactory) { Modifier = ItemFlagFactory.CreateInstance("Bless"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToAll, "{0} glows with a holy aura.", item);
    }
}
