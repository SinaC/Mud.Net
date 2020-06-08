using Mud.Logger;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Teleport", AbilityEffects.Transportation)]
    public class Teleport : SpellBase
    {
        private IRoomManager RoomManager { get; }

        protected ICharacter Victim { get; set; }

        public Teleport(IRandomManager randomManager, IWiznet wiznet, IRoomManager roomManager)
            : base(randomManager, wiznet)
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

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            Victim = FindHelpers.FindChararacterInWorld(Caster, abilityActionInput.Parameters[0]);
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
