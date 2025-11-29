using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects.Character;
using Mud.Server.Affects.Item;
using Mud.Server.Flags.Interfaces;
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

    private IAuraManager AuraManager { get; }
    private IFlagFactory FlagFactory { get; }

    public SpellTest(ILogger<SpellTest> logger, IRandomManager randomManager, IAuraManager auraManager, IFlagFactory flagFactory) 
        : base(logger, randomManager)
    {
        AuraManager = auraManager;
        FlagFactory = flagFactory;
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
            new CharacterFlagsAffect { Modifier = FlagFactory.CreateInstance<ICharacterFlags, ICharacterFlagValues>("Pouet")},
            new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Magic"), Operator = AffectOperators.Or },
            new CharacterIRVAffect { Location = IRVAffectLocations.Immunities, Modifier = FlagFactory.CreateInstance<IIRVFlags, IIRVFlagValues>("Weapon"), Operator = AffectOperators.Or });
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
                new ItemWeaponFlagsAffect { Modifier = FlagFactory.CreateInstance<IWeaponFlags, IWeaponFlagValues>("Flaming", "Frost", "Vampiric", "Sharp", "Vorpal", "Shocking", "Poison") });
        }

        AuraManager.AddAura(item, SpellName, Caster, Level, TimeSpan.FromMinutes(10), AuraFlags.NoDispel, true,
            new ItemFlagsAffect { Modifier = FlagFactory.CreateInstance<IItemFlags, IItemFlagValues>("Glowing", "Humming", "Magic") },
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -Level, Operator = AffectOperators.Add},
            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Characteristics, Modifier = Level, Operator = AffectOperators.Add });
    }
}
