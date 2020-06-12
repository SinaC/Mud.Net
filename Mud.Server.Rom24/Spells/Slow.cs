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
    public class Slow : OffensiveSpellBase
    {
        public const string SpellName = "Slow";

        private IAuraManager AuraManager { get; }
        private IDispelManager DispelManager { get; }

        public Slow(IRandomManager randomManager, IAuraManager auraManager, IDispelManager dispelManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
            DispelManager = dispelManager;
        }

        protected override void Invoke()
        {
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Slow)
                || Victim.GetAura(SpellName) != null)
            {
                if (Victim == Caster)
                    Caster.Send("You can't move any slower!");
                else
                    Caster.Act(ActOptions.ToCharacter, "{0:N} can't get any slower than that.", Victim);
                return;
            }

            if (Victim.Immunities.HasFlag(IRVFlags.Magic)
                || Victim.SavesSpell(Level, SchoolTypes.Other))
            {
                if (Victim != Caster)
                    Caster.Send("Nothing seemed to happen.");
                Victim.Send("You feel momentarily lethargic.");
                return;
            }

            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Haste))
            {
                if (DispelManager.TryDispel(Level, Victim, Haste.SpellName) != TryDispelReturnValues.Dispelled)
                {
                    if (Victim != Caster)
                        Caster.Send("Spell failed.");
                    Victim.Send("You feel momentarily slower.");
                    return;
                }
                Victim.Act(ActOptions.ToRoom, "{0:N} is moving less quickly.", Victim);
                return;
            }

            int duration = Level / 2;
            int modifier = -1 - (Level >= 18 ? 1 : 0) - (Level >= 25 ? 1 : 0) - (Level >= 32 ? 1 : 0);
            AuraManager.AddAura(Victim, SpellName, Caster, Level, TimeSpan.FromMinutes(duration), AuraFlags.None, true,
                new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Dexterity, Modifier = modifier, Operator = AffectOperators.Add },
                new CharacterFlagsAffect { Modifier = CharacterFlags.Slow, Operator = AffectOperators.Or });
            Victim.Recompute();
            Victim.Send("You feel yourself slowing d o w n...");
            Caster.Act(ActOptions.ToRoom, "{0} starts to move in slow motion.", Victim);
        }
    }
}
