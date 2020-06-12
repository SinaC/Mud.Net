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
    [Spell(SpellName, AbilityEffects.DamageArea)]
    public class CallLightning : NoTargetSpellBase
    {
        public const string SpellName = "Call Lightning";

        private ITimeManager TimeManager { get; }

        public CallLightning(IRandomManager randomManager, ITimeManager timeManager)
            : base(randomManager)
        {
            TimeManager = timeManager;
        }

        protected override void Invoke()
        {
            INonPlayableCharacter npcCaster = Caster as INonPlayableCharacter;
            int damage = RandomManager.Dice(Level / 2, 8);
            Caster.Send("Mota's lightning strikes your foes!");
            Caster.Act(ActOptions.ToRoom, "{0:N} calls Mota's lightning to strike {0:s} foes!", Caster);
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(Caster.Room.People.Where(x => x != Caster).ToList()); // clone because damage could kill and remove character from list
            foreach (ICharacter victim in clone)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
                if (npcCaster != null ? npcVictim == null : npcVictim != null) // NPC on PC and PC on NPC
                {
                    if (victim.SavesSpell(Level, SchoolTypes.Lightning))
                        victim.AbilityDamage(Caster, damage / 2, SchoolTypes.Lightning, "lightning bolt", true);
                    else
                        victim.AbilityDamage(Caster, damage, SchoolTypes.Lightning, "lightning bolt", true);
                }
            }
            // Inform in area about it
            foreach (ICharacter character in Caster.Room.Area.Characters.Where(x => x.Position > Positions.Sleeping && !x.Room.RoomFlags.HasFlag(RoomFlags.Indoors)))
                character.Send("Lightning flashes in the sky.");
        }

        public override string Setup(ISpellActionInput spellActionInput)
        {
            string baseSetup = base.Setup(spellActionInput);
            if (baseSetup != null)
                return baseSetup;

            if (Caster.Room.RoomFlags.HasFlag(RoomFlags.Indoors))
                return "You must be out of doors.";
            if (TimeManager.SkyState < SkyStates.Raining)
                return "You need bad weather.";
            return null;
        }
    }
}
