using Mud.Domain;
using Mud.Server.Interfaces.Affect;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Random;
using System;

namespace Mud.Server.Rom24
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
