using Mud.Domain;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Affects;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;
using Mud.Server.Rom24.Effects;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Dispel)]
    public class Cancellation : DefensiveSpellBase
    {
        public const string SpellName = "Cancellation";

        private IDispelManager DispelManager { get; }

        public Cancellation(IRandomManager randomManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            if ((Caster is IPlayableCharacter && Victim is INonPlayableCharacter npcVictim && !Caster.CharacterFlags.HasFlag(CharacterFlags.Charm) && npcVictim.Master == Caster)
                || (Caster is INonPlayableCharacter && Victim is IPlayableCharacter))
            {
                Caster.Send("You failed, try dispel magic.");
                return;
            }

            // unlike dispel magic, no save roll
            bool found = DispelManager.TryDispels(Level + 2, Victim);

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (found)
                Caster.Send("Ok.");
            else
                Caster.Send("Spell failed.");
        }
    }
}
