using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Common;
using System;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Change Sex", AbilityEffects.Buff)]
    public class ChangeSex : DefensiveSpellBase
    {
        private IAuraManager AuraManager { get; }
        public ChangeSex(IRandomManager randomManager, IWiznet wiznet, IAuraManager auraManager)
            : base(randomManager, wiznet)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(AbilityInfo.Name) != null)
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
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, Level, TimeSpan.FromHours(2 * Level), AuraFlags.None, true,
                new CharacterSexAffect { Value = newSex });
            Victim.Send("You feel different.");
            Victim.Act(ActOptions.ToRoom, "{0:N} doesn't look like {0:m}self anymore...", Victim);
        }
    }
}
