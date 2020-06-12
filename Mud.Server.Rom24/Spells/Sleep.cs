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
    [Spell(SpellName, AbilityEffects.Debuff)]
    [AbilityCharacterWearOffMessage("You feel less tired.")]
    [AbilityDispellable]
    public class Sleep : OffensiveSpellBase
    {
        public const string SpellName = "Sleep";

        private IAuraManager AuraManager { get; }

        public Sleep(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Sleep)
                || (Victim is INonPlayableCharacter npcVictim && npcVictim.ActFlags.HasFlag(ActFlags.Undead))
                || Level + 2 < Victim.Level
                || Victim.SavesSpell(Level - 4, SchoolTypes.Charm))
                return;

            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(4 + Level), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -10, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Sleep, Operator = AffectOperators.Or });

            if (Victim.Position > Positions.Sleeping)
            {
                Victim.Send("You feel very sleepy ..... zzzzzz.");
                Victim.Act(ActOptions.ToRoom, "{0:N} goes to sleep.", Victim);
                Victim.ChangePosition(Positions.Sleeping);
            }
        }
    }
}
