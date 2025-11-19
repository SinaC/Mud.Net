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

namespace Mud.Server.POC.Spells;

[Spell(SpellName, AbilityEffects.Buff, CooldownInSeconds = 1)]
public class SpellTest : ItemOrDefensiveSpellBase
{
    private const string SpellName = "Test";

    private IServiceProvider ServiceProvider { get; }
    private IAuraManager AuraManager { get; }

    public SpellTest(ILogger<SpellTest> logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager) 
        : base(logger, randomManager)
    {
        ServiceProvider = serviceProvider;
        AuraManager = auraManager;
    }

    protected override void Invoke(ICharacter victim)
    {
        if (victim != Caster)
        {
            victim.ChangeStunned(5);
            return;
        }
        if (victim.GetAura(SpellName) != null)
        {
            Caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} already affected by divine aura.", victim);
            return;
        }

        // Immune to all damages
        AuraManager.AddAura(victim, SpellName, Caster, Level, TimeSpan.FromMinutes(1), AuraFlags.NoDispel, true,
            new CharacterFlagsAffect { Modifier = new CharacterFlags(ServiceProvider, "Pouet")},
            new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = new IRVFlags(ServiceProvider,"Magic"), Operator = AffectOperators.Or },
            new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = new IRVFlags(ServiceProvider, "Weapon"), Operator = AffectOperators.Or });
    }

    protected override void Invoke(IItem item)
    {
        if (item.GetAura(SpellName) != null)
        {
            Caster.Act(ActOptions.ToCharacter, "{0} is already affected by divine aura.", item);
            return;
        }

        if (item is IItemWeapon itemWeapon)
        {
            AuraManager.AddAura(itemWeapon, SpellName, Caster, Level, TimeSpan.FromMinutes(10), AuraFlags.NoDispel, true,
                new ItemWeaponFlagsAffect { Modifier = new WeaponFlags(ServiceProvider, "Flaming", "Frost", "Vampiric", "Sharp", "Vorpal", "Shocking", "Poison") });
        }

        AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(10), AuraFlags.NoDispel, true,
            new ItemFlagsAffect { Modifier = new ItemFlags(ServiceProvider, "Glowing", "Humming", "Magic") },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -Level, Operator = AffectOperators.Add},
        new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Characteristics, Modifier = Level, Operator = AffectOperators.Add });
    }
}
