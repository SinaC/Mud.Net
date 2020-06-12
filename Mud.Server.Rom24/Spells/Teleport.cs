using Mud.Common;
using Mud.Domain;
using Mud.Logger;
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
    [Spell(SpellName, AbilityEffects.Transportation)]
    public class Teleport : SpellBase
    {
        public const string SpellName = "Teleport";

        private IRoomManager RoomManager { get; }

        protected ICharacter Victim { get; set; }

        public Teleport(IRandomManager randomManager, IRoomManager roomManager)
            : base(randomManager)
        {
            RoomManager = roomManager;
        }

        protected override void Invoke()
        {
            IRoom destination = RoomManager.GetRandomRoom(Caster);
            if (destination == null)
            {
                Log.Default.WriteLine(LogLevels.Warning, "Teleport: no random room available for {0}", Victim.DebugName);
                Caster.Send("Spell failed.");
                return;
            }

            if (Victim != Caster)
                Victim.Send("You have been teleported!");

            Victim.Act(ActOptions.ToRoom, "{0:N} vanishes", Victim);
            Victim.ChangeRoom(destination);
            Victim.Act(ActOptions.ToRoom, "{0:N} slowly fades into existence.", Victim);
            Victim.AutoLook();
        }

        protected override string SetTargets(ISpellActionInput spellActionInput)
        {
            Victim = FindHelpers.FindChararacterInWorld(Caster, spellActionInput.Parameters[0]);
            if (Victim == null || IsVictimValid())
                return "You failed.";
            return null;
        }

        protected virtual bool IsVictimValid()
        {
            if (Victim.Room == null
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall)
                || (Victim != Caster && Victim.Immunities.HasFlag(IRVFlags.Summon))
                || (Victim is IPlayableCharacter pcVictim && pcVictim.Fighting != null)
                || (Victim != Caster && Victim.SavesSpell(Level - 5, SchoolTypes.Other)))
                return false;
            return true;
        }
    }
}
