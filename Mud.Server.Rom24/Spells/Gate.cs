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
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using Mud.Settings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Transportation)]
    public class Gate : TransportationSpellBase
    {
        public const string SpellName = "Gate";
            
        public Gate(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            Caster.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", Caster);
            Caster.ChangeRoom(Victim.Room);
            Caster.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", Caster);
            Caster.AutoLook();

            // pets follows
            if (Caster is IPlayableCharacter pcCaster)
            {
                foreach (INonPlayableCharacter pet in pcCaster.Pets)
                {
                    pet.Act(ActOptions.ToAll, "{0:N} step{0:v} through a gate and vanish{0:v}.", pet);
                    pet.ChangeRoom(Victim.Room);
                    pet.Act(ActOptions.ToRoom, "{0:N} has arrived through a gate.", pet);
                    pet.AutoLook(); // TODO: needed ?
                }
            }
        }
    }
}
