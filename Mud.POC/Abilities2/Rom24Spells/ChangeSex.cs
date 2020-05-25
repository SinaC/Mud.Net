using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class ChangeSex : DefensiveSpellBase
    {
        public override int Id => 13;
        public override string Name => "Change Sex";
        public override AbilityEffects Effects => AbilityEffects.Buff;

        private IAuraManager AuraManager { get; }
        public ChangeSex(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        public override void Action(ICharacter caster, int level, ICharacter victim)
        {
            if (victim.GetAura(this) != null)
            {
                if (victim == caster)
                    caster.Send("You've already been changed.");
                else
                    caster.Act(ActOptions.ToCharacter, "{0:N} has already had {0:s} sex changed.", victim);
                return;
            }

            if (victim.SavesSpell(level, SchoolTypes.Other))
                return;

            Sex newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != victim.Sex));
            AuraManager.AddAura(victim, this, caster, level, TimeSpan.FromHours(2 * level), AuraFlags.None, true,
                new CharacterSexAffect { Value = newSex });
            victim.Send("You feel different.");
            victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like {0:m}self anymore...", victim);
        }
    }
}
