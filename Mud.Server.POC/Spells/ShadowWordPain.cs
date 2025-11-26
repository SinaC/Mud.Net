using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Aura;
using Mud.Server.POC.Affects;
using Mud.Server.Random;

namespace Mud.Server.POC.Spells
{
    [Spell(SpellName, AbilityEffects.Debuff | AbilityEffects.Damage)]
    public class ShadowWordPain : OffensiveSpellBase
    {
        private const string SpellName = "Shadow Word: Pain";

        private IAuraManager AuraManager { get; }

        public ShadowWordPain(ILogger<OffensiveSpellBase> logger, IRandomManager randomManager, IAuraManager auraManager)
            : base(logger, randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            // direct damage
            var damage = RandomManager.Dice(1, Level);
            Victim.AbilityDamage(Caster, damage, SchoolTypes.Negative, "word of darkness", true);
            // dot
            // TODO: apply only if not already applied
            AuraManager.AddAura(Victim, SpellName, Caster, (3 * Level) / 4, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new ShadowWordPainAffect());
        }
    }
}
