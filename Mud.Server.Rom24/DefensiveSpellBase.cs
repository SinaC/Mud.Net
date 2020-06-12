﻿using Mud.Server.Common;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Random;
using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Rom24
{
    public abstract class DefensiveSpellBase : SpellBase, ITargetedAction
    {
        protected ICharacter Victim { get; set; }

        protected DefensiveSpellBase(IRandomManager randomManager) 
            : base(randomManager)
        {
        }

        public IEnumerable<IEntity> ValidTargets(ICharacter caster) => caster.Room.People.Where(caster.CanSee);

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.IsCastFromItem && spellActionInput.CastFromItemOptions.PredefinedTarget != null)
            {
                Victim = spellActionInput.CastFromItemOptions.PredefinedTarget as ICharacter;
                if (Victim == null)
                    Victim = Caster;
                return null;
            }

            if (spellActionInput.Parameters.Length < 1)
                Victim = Caster;
            else
            {
                Victim = FindHelpers.FindByName(Caster.Room.People.Where(Caster.CanSee), spellActionInput.Parameters[0]);
                if (Victim == null)
                    return "They aren't here.";
            }
            // victim found
            return null;
        }
    }
}
