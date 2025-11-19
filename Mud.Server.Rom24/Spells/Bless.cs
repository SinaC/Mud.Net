using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Buff | AbilityEffects.Dispel)]
[AbilityCharacterWearOffMessage("You feel less righteous.")]
[AbilityItemWearOffMessage("{0}'s holy aura fades.")]
public class Bless : ItemOrDefensiveSpellBase
{
    private const string SpellName = "Bless";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }
    private IDispelManager DispelManager { get; }

    public Bless(ILogger<Bless> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager) 
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke(ICharacter victim)
    {
       BlessEffect effect = new (AuraManager);
       effect.Apply(victim, Caster, SpellName, Level, 0);
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
            new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Bless"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToAll, "{0} glows with a holy aura.", item);
    }
}
