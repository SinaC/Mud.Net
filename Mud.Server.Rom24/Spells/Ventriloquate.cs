﻿using Mud.Common;
using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Common;
using Mud.Server.Input;
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
    [Spell(SpellName, AbilityEffects.None)]
    public class Ventriloquate : SpellBase
    {
        public const string SpellName = "Ventriloquate";

        protected ICharacter Victim { get; set; }
        protected string Phrase { get; set; }

        public Ventriloquate(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        protected override void Invoke()
        {
            string phraseSuccess = $"%g%{Victim.DisplayName} says '%x%{Phrase ?? ""}%g%'%x%.";
            string phraseFail = $"Someone makes %g%{Victim.DisplayName} say '%x%{Phrase ?? ""}%g%'%x%.";

            foreach (ICharacter character in Caster.Room.People.Where(x => x != Victim && x.Position > Positions.Sleeping))
            {
                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                if (character.SavesSpell(Level, SchoolTypes.Other))
                    character.Send(phraseFail);
                else
                    character.Send(phraseSuccess);
            }
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            if (spellActionInput.Parameters.Length < 2)
                return "Make who saying what?";

            Victim = FindHelpers.FindByName(Caster.Room.People, spellActionInput.Parameters[0]);
            if (Victim == null)
                return "They aren't here.";
            if (Victim == Caster)
                return "Just say it.";
            Phrase = CommandHelpers.JoinParameters(spellActionInput.Parameters.Skip(1));
            return null;
        }

        protected override void SaySpell()
        {
            return;
        }
    }
}
