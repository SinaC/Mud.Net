using Mud.Common;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell(SpellName, AbilityEffects.Buff)]
    public class ChangeSex : DefensiveSpellBase
    {
        public const string SpellName = "Change Sex";

        private IAuraManager AuraManager { get; }
        public ChangeSex(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(SpellName) != null)
            {
                if (Victim == Caster)
                    Caster.Send("You've already been changed.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} has already had {0:s} sex changed.", Victim);
                return;
            }

            if (Victim.SavesSpell(Level, SchoolTypes.Other))
                return;

            Sex newSex = RandomManager.Random(EnumHelpers.GetValues<Sex>().Where(x => x != Victim.Sex));
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromHours(2 * Level), AuraFlags.None, true,
                new CharacterSexAffect { Value = newSex });
            Victim.Send("You feel different.");
            Victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like {0:m}self anymore...", Victim);
        }
    }
}
