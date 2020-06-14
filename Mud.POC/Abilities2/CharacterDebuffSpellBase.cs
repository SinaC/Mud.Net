﻿using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Random;
using System;

namespace Mud.POC.Abilities2
{
    public abstract class CharacterDebuffSpellBase : OffensiveSpellBase
    {
        protected IAuraManager AuraManager { get; }

        protected CharacterDebuffSpellBase(IRandomManager randomManager, IAuraManager auraManager)
            : base(randomManager)
        {
            AuraManager = auraManager;
        }

        protected override void Invoke()
        {
            if (!CanAffect)
                return;
            var auraInfo = AuraInfo;
            AuraManager.AddAura(Victim, AbilityInfo.Name, Caster, auraInfo.level, auraInfo.duration, AuraFlags.None, true, auraInfo.affects);
            Victim.Act(ActOptions.ToCharacter, VictimAffectMessage, Caster);
            Victim.Act(ActOptions.ToRoom, RoomAffectMessage, Victim);
        }

        protected abstract SchoolTypes DebuffType { get; }
        protected abstract string VictimAffectMessage { get; }
        protected abstract string RoomAffectMessage { get; }

        protected abstract (int level, TimeSpan duration, IAffect[] affects) AuraInfo { get; }

        protected virtual bool CanAffect => Victim.GetAura(AbilityInfo.Name) == null && !Victim.SavesSpell(Level, DebuffType);
    }
}
