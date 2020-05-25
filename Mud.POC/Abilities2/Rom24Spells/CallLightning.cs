using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.Interfaces;
using Mud.Server.Common;
using Mud.Server.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Spells
{
    public class CallLightning : SpellBase
    {
        public override int Id => 6;

        public override string Name => "Call lightning";

        public override AbilityEffects Effects => throw new NotImplementedException();

        private ITimeManager TimeManager { get; }

        public CallLightning(IRandomManager randomManager, IWiznet wiznet, ITimeManager timeManager) : base(randomManager, wiznet)
        {
            TimeManager = timeManager;
        }

        protected override void Invoke(ICharacter caster, int level, string rawParameters, params CommandParameter[] parameters)
        {
            if (caster.Room == null)
                return;

            if (caster.Room.RoomFlags.HasFlag(RoomFlags.Indoors))
            {
                caster.Send("You must be out of doors.");
                return;
            }

            if (TimeManager.SkyState < SkyStates.Raining)
            {
                caster.Send("You need bad weather.");
                return;
            }

            INonPlayableCharacter npcCaster = caster as INonPlayableCharacter;
            int damage = RandomManager.Dice(level / 2, 8);
            caster.Send("Mota's lightning strikes your foes!");
            caster.Act(ActOptions.ToRoom, "{0:N} calls Mota's lightning to strike {0:s} foes!", caster);
            IReadOnlyCollection<ICharacter> clone = new ReadOnlyCollection<ICharacter>(caster.Room.People.Where(x => x != caster).ToList()); // clone because damage could kill and remove character from list
            foreach (ICharacter victim in clone)
            {
                INonPlayableCharacter npcVictim = victim as INonPlayableCharacter;
                if (npcCaster != null ? npcVictim == null : npcVictim != null) // NPC on PC and PC on NPC
                {
                    if (victim.SavesSpell(level, SchoolTypes.Lightning))
                        victim.AbilityDamage(caster, this, damage / 2, SchoolTypes.Lightning, "lightning bolt", true);
                    else
                        victim.AbilityDamage(caster, this, damage, SchoolTypes.Lightning, "lightning bolt", true);
                }
            }
            // Inform in area about it
            foreach (ICharacter character in caster.Room.Area.Characters.Where(x => x.Position > Positions.Sleeping && !x.Room.RoomFlags.HasFlag(RoomFlags.Indoors)))
                character.Send("Lightning flashes in the sky.");
        }

        protected override AbilityTargetResults SetTargets(ICharacter caster, string rawParameters, params CommandParameter[] parameters) => AbilityTargetResults.Ok;
    }
}
