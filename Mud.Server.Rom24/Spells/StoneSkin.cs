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
    [Spell(SpellName, AbilityEffects.Buff, PulseWaitTime = 18)]
    [AbilityCharacterWearOffMessage("Your skin feels soft again.")]
    [AbilityDispellable("{0:N}'s skin regains its normal texture.")]
    public class StoneSkin : DefensiveSpellBase
    {
        public const string SpellName = "Stone Skin";

        private IAuraManager AuraManager { get; }

        public StoneSkin(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.GetAura(SpellName) != null)
            {
                if (Victim == Caster)
                    Caster.Send("Your skin is already as hard as a rock.");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} is already as hard as can be.", Victim);
                return;
            }

            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -40, Operator = AffectOperators.Add });
            Caster.Act(ActOptions.ToAll, "{0:P} skin turns to stone.", Victim);
        }
    }
}
