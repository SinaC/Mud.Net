﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;
using System.Collections.Generic;
using System.Linq;

namespace Mud.POC.Abilities2
{
    public abstract class TransportationSpellBase : SpellBase
    {
        protected ICharacter Victim { get; set; }

        protected TransportationSpellBase(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override IEnumerable<IEntity> AvailableTargets(ICharacter caster) => Enumerable.Empty<IEntity>();

        protected override string SetTargets(SpellActionInput spellActionInput)
        {
            Victim = FindHelpers.FindChararacterInWorld(Caster, spellActionInput.Parameters[0]);
            if (Victim == null || !IsVictimValid())
                return "You failed.";
            return null;
        }

        protected virtual bool IsVictimValid()
        {
            INonPlayableCharacter npcVictim = Victim as INonPlayableCharacter;
            if (Victim == Caster
                || Victim.Room == null
                || !Caster.CanSee(Victim.Room)
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Caster.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Safe)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Private)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.Solitary)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.ImpOnly)
                || Victim.Level >= Level + 3
                // TODO: clan check
                // TODO: hero level check 
                || (npcVictim != null && npcVictim.Immunities.HasFlag(IRVFlags.Summon))
                || (npcVictim != null && Victim.SavesSpell(Level, SchoolTypes.Other)))
                return false;
            return true;
        }
    }
}
