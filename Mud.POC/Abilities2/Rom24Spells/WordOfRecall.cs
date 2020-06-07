using Mud.Logger;
using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Common;

namespace Mud.POC.Abilities2.Rom24Spells
{
    [Spell("Word of Recall", AbilityEffects.Transportation)]
    public class WordOfRecall : SpellBase
    {
        protected IPlayableCharacter Victim { get; set; }
        protected IRoom RecallRoom { get; set; }
        public WordOfRecall(IRandomManager randomManager, IWiznet wiznet)
            : base(randomManager, wiznet)
        {
        }

        protected override void Invoke()
        {
            IRoom recallRoom = Victim.RecallRoom;
            if (recallRoom == null)
            {
                Log.Default.WriteLine(LogLevels.Error, "No recall room found for {0}", Victim.DebugName);
                Caster.Act(ActOptions.ToCharacter, "{0:N} {0:b} completely lost.", Victim);
                return;
            }

            if (Victim.Fighting != null)
                Victim.StopFighting(true);

            Victim.UpdateMovePoints(-Victim.MovePoints / 2); // half move
            Victim.Act(ActOptions.ToRoom, "{0:N} disappears", Victim);
            Victim.ChangeRoom(recallRoom);
            Victim.Act(ActOptions.ToRoom, "{0:N} appears in the room.", Victim);
            Victim.AutoLook();

            // Pets follows
            foreach (INonPlayableCharacter pet in Victim.Pets)
            {
                // no recursive call because Spell has been coded for IPlayableCharacter
                if (pet.CharacterFlags.HasFlag(CharacterFlags.Curse))
                    continue; // pet failing doesn't impact return value
                if (pet.Fighting != null)
                {
                    if (!RandomManager.Chance(80))
                        continue;// pet failing doesn't impact return value
                    pet.StopFighting(true);
                }

                pet.Act(ActOptions.ToRoom, "{0:N} disappears", pet);
                pet.ChangeRoom(recallRoom);
                pet.Act(ActOptions.ToRoom, "{0:N} appears in the room.", pet);
                pet.AutoLook();
            }
        }

        protected override string SetTargets(AbilityActionInput abilityActionInput)
        {
            ICharacter victim;
            if (abilityActionInput.Parameters.Length < 1)
                victim = Caster;
            else
            {
                victim = FindHelpers.FindByName(Caster.Room.People, abilityActionInput.Parameters[0]);
                if (victim == null)
                    return "They aren't here.";
            }
            Victim = victim as IPlayableCharacter;
            if (Victim == null)
                return "Spell failed.";
            if (Victim.CharacterFlags.HasFlag(CharacterFlags.Curse)
                || Victim.Room.RoomFlags.HasFlag(RoomFlags.NoRecall))
                return "Spell failed.";
            // victim found, is PC and is not affected by Curse or in NoRecall room
            return null;
        }
    }
}
