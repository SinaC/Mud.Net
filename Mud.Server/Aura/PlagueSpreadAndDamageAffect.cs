﻿using System;
using System.Linq;
using System.Text;
using Mud.Container;
using Mud.Domain;
using Mud.Server.Common;

namespace Mud.Server.Aura
{
    public class PlagueSpreadAndDamageAffect : ICharacterPeriodicAffect
    {
        private IRandomManager RandomManager => DependencyContainer.Current.GetInstance<IRandomManager>();
        private IWorld World => DependencyContainer.Current.GetInstance<IWorld>();

        public void Append(StringBuilder sb)
        {
            sb.Append("%c%applies %r%disease%x% damage periodically");
        }

        public AffectDataBase MapAffectData()
        {
            return new PlagueSpreadAndDamageAffectData();
        }

        public void Apply(IAura aura, ICharacter character)
        {
            if (aura.Level == 1)
                return;

            character.Act(ActOptions.ToRoom, "{0:N} writhes in agony as plague sores erupt from {0:s} skin.", character);
            character.Send("You writhe in agony from the plague.");

            // spread
            if (character.Room != null)
            {
                foreach (ICharacter victim in character.Room.People.Where(x => !x.CharacterFlags.HasFlag(CharacterFlags.Plague)))
                {
                    if (!victim.SavesSpell(aura.Level - 2, SchoolTypes.Disease)
                        && RandomManager.Chance(6))
                    {
                        victim.Send("You feel hot and feverish.");
                        victim.Act(ActOptions.ToRoom, "{0:N} shivers and looks very ill.", victim);
                        int duration = RandomManager.Range(1, 2 * aura.Level);
                        World.AddAura(victim, aura.Ability, character, aura.Level - 1, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                            new CharacterAttributeAffect {Location = CharacterAttributeAffectLocations.Strength, Modifier = -5, Operator = AffectOperators.Add},
                            new CharacterFlagsAffect {Modifier = CharacterFlags.Plague, Operator = AffectOperators.Or},
                            new PlagueSpreadAndDamageAffect());
                    }
                }
            }

            // damage
            int damage = Math.Min(character.Level, aura.Level / 5 + 1);
            character.UpdateMovePoints(-damage);
            character.UpdateResource(ResourceKinds.Mana, -damage);
            character.UpdateResource(ResourceKinds.Psy, -damage);
            character.AbilityDamage(character, aura.Ability, damage, SchoolTypes.Disease, false);
        }
    }
}