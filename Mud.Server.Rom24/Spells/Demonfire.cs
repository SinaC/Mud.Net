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
    [Spell(SpellName, AbilityEffects.Damage)]
    public class Demonfire : DamageSpellBase
    {
        public const string SpellName = "Demonfire";

        protected override SchoolTypes DamageType => SchoolTypes.Negative;
        protected override string DamageNoun => "torments";
        protected override int DamageValue => RandomManager.Dice(Level, 10);

        public Demonfire(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            string baseSetTargets = base.SetTargets(spellActionInput);
            if (baseSetTargets != null)
                return baseSetTargets;

            // Check alignment
            if (Caster is IPlayableCharacter && !Caster.IsEvil)
            {
                Victim = Caster;
                Caster.Send("The demons turn upon you!");
            }
            return null;
        }

        protected override void Invoke()
        {
            if (Victim != Caster)
            {
                Caster.Act(ActOptions.ToRoom, "{0} calls forth the demons of Hell upon {1}!", Caster, Victim);
                Victim.Act(ActOptions.ToCharacter, "{0} has assailed you with the demons of Hell!", Caster);
                Caster.Send("You conjure forth the demons of hell!");
            }

            base.Invoke();

            Caster.UpdateAlignment(-50);
        }
    }
}
