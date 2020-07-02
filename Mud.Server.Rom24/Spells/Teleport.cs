using Mud.Domain;
using Mud.Logger;
using Mud.Server.Ability;
using Mud.Server.Ability.Spell;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Spells
{
    [Spell(SpellName, AbilityEffects.Transportation)]
    public class Teleport : TransportationSpellBase
    {
        public const string SpellName = "Teleport";

        private IRoomManager RoomManager { get; }

        public Teleport(IRandomManager randomManager, IRoomManager roomManager, ICharacterManager characterManager)
            : base(randomManager, characterManager)
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

            Victim.Act(ActOptions.ToRoom, "{0:N} vanishes.", Victim);
            Victim.ChangeRoom(destination);
            Victim.Act(ActOptions.ToRoom, "{0:N} slowly fades into existence.", Victim);
            AutoLook(Victim);
        }

        protected override bool IsVictimValid()
        {
            if (Victim.Room == null
                || Victim.Room.RoomFlags.IsSet("NoRecall")
                || (Victim != Caster && Victim.Immunities.IsSet("Summon"))
                || (Victim is IPlayableCharacter pcVictim && pcVictim.Fighting != null)
                || (Victim != Caster && Victim.SavesSpell(Level - 5, SchoolTypes.Other)))
                return false;
            return true;
        }
    }
}
