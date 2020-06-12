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
    [Spell(SpellName, AbilityEffects.Buff)]
    [AbilityCharacterWearOffMessage("The white aura around your body fades.")]
    [AbilityDispellable("The white aura around {0:n}'s body vanishes.")]
    public class Sanctuary : CharacterFlagsSpellBase
    {
        public const string SpellName = "Sanctuary";

        public Sanctuary(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override CharacterFlags CharacterFlags => CharacterFlags.Sanctuary;
        protected override TimeSpan Duration => TimeSpan.FromMinutes(Level / 6);
        protected override string SelfAlreadyAffected => "You are already in sanctuary.";
        protected override string NotSelfAlreadyAffected => "{0:N} is already in sanctuary.";
        protected override string SelfSuccess => "You are surrounded by a white aura.";
        protected override string NotSelfSuccess => "{0:N} is surrounded by a white aura.";
    }
}
