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
    [AbilityCharacterWearOffMessage("Your force shield shimmers then fades away.")]
    [AbilityDispellable("The shield protecting {0:n} vanishes.")]
    public class Shield : CharacterBuffSpellBase
    {
        public const string SpellName = "Shield";

        public Shield(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager, auraManager)
        {
        }

        protected override string SelfAlreadyAffectedMessage => "You are already shielded from harm.";
        protected override string NotSelfAlreadyAffectedMessage => "{0:N} is already protected by a shield.";
        protected override string VictimAffectMessage => "You are surrounded by a force shield.";
        protected override string CasterAffectMessage => "{0:N} {0:b} surrounded by a force shield.";
        protected override (int level, TimeSpan duration, IAffect[] affects) AuraInfo
            => (Level, TimeSpan.FromMinutes(8 + Level),
            new IAffect[]
            {
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = -20, Operator = AffectOperators.Add }
            });
    }
}
