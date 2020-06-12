using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 24)]
    [AbilityCharacterWearOffMessage("You are no longer invisible.")]
    [AbilityDispellable("{0:N} fades into existence.")]
    public class MassInvis : NoTargetSpellBase
    {
        public const string SpellName = "Mass Invis";

        private IAuraManager AuraManager { get; }

        public MassInvis(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            foreach (ICharacter victim in Caster.Room.People.Where(Caster.CanSee))
            {
                if (Caster.IsSameGroupOrPet(victim))
                {
                    victim.Act(ActOptions.ToAll, "{0:N} slowly fade{0:v} out of existence.", victim);

                    AuraManager.AddAura(victim, SpellName, Caster, Level / 2, TimeSpan.FromMinutes(24), AuraFlags.None, true,
                        new CharacterFlagsAffect { Modifier = CharacterFlags.Invisible, Operator = AffectOperators.Or });
                }
            }
            Caster.Send("Ok.");
        }
    }
}
