using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Flags;
using Mud.Server.GameAction;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;

namespace Mud.Server.Rom24.Spells;

[Spell(SpellName, AbilityEffects.Debuff)]
[AbilityCharacterWearOffMessage("The curse wears off.")]
[AbilityItemWearOffMessage("{0} is no longer impure.")]
[AbilityDispellable("{0} is no longer impure.")]
[Syntax("cast [spell] <character>")]
[Help(
@"This spell reduces the character's to-hit roll and save versus spells.
It also renders the character unclean in the eyes of Mota and
unable to RECALL. Curse may be used to fill equipment with evil power,
allowing (for example) weapons to do more damage to particularly holy
opponents.")]
public class Curse : ItemOrOffensiveSpellBase
{
    private const string SpellName = "Curse";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }
    private IDispelManager DispelManager { get; }

    public Curse(ILogger<Curse> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
        DispelManager = dispelManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        CurseEffect effect = new (ServiceProvider, AuraManager);
        effect.Apply(victim, Caster, SpellName, Level, 0);
    }

    protected override void Invoke(IItem item)
    {
        if (item.ItemFlags.IsSet("Evil"))
        {
            Caster.Act(ActOptions.ToCharacter, "{0} is already filled with evil.", item);
            return;
        }
        if (item.ItemFlags.IsSet("Bless"))
        {
            var blessAura = item.GetAura("Bless");
            if (!DispelManager.SavesDispel(Level, blessAura?.Level ?? item.Level, 0))
            {
                if (blessAura != null)
                    item.RemoveAura(blessAura, false);
                Caster.Act(ActOptions.ToAll, "{0} glows with a red aura.", item);
                item.RemoveBaseItemFlags(true, "Bless");
                return;
            }
            else
                Caster.Act(ActOptions.ToCharacter, "The holy aura of {0} is too powerful for you to overcome.");
            return;
        }
        AuraManager.AddAura(item, AbilityInfo.Name, Caster, Level, TimeSpan.FromMinutes(2 * Level), AuraFlags.None, true,
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.SavingThrow, Modifier = 1, Operator = AffectOperators.Add },
            new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Evil"), Operator = AffectOperators.Or });
        Caster.Act(ActOptions.ToAll, "{0} glows with a malevolent aura.", item);
    }
}
