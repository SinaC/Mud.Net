using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_patrolman")]
    public class PatrolMan : ISpecialBehavior
    {
        private IRandomManager RandomManager { get; }
        private DangerousNeighborhood Options { get; }

        public PatrolMan(IRandomManager randomManager, IOptions<Rom24Options> rom24Options)
        {
            RandomManager = randomManager;
            Options = rom24Options.Value.DangerousNeighborhood;
        }

        public bool Execute(INonPlayableCharacter npc)
        {
            // must be awake, not charmed neither calm
            if (!npc.IsValid || npc.Room == null
                || npc.Position <= Positions.Sleeping || npc.CharacterFlags.IsSet("Charm") || npc.CharacterFlags.IsSet("Calm")
                || npc.Fighting != null)
                return false;

            // look for a fight in the room
            var count = 0;
            ICharacter? victim = null;
            foreach (var ch in npc.Room.People.Where(x => x != npc && x.Fighting != null))
            {
                if (RandomManager.Range(0, count) == 0)
                    victim = ch.Level > ch.Fighting!.Level // attack highest level between ch and ch.Fighting
                        ? ch
                        : ch.Fighting;
                count++;
            }
            //
            if (victim == null || (victim is INonPlayableCharacter npcVictim && npcVictim.Blueprint.SpecialBehavior == npc.Blueprint.SpecialBehavior))
                return false;
            // if holding whistle, use it
            var whistle = npc.GetEquipment(EquipmentSlots.Amulet);
            if (whistle?.Blueprint.Id == Options.Whistle)
            {
                npc.Act(ActOptions.ToCharacter, "You blow down hard on {0}.", whistle);
                npc.Act(ActOptions.ToRoom, "{0:N} blows on on {1}, ***WHEEEEEEEEEEEET***", npc, whistle);

                // warn everyone in the area except for this room
                foreach (var ch in npc.Room.Area.Characters.Where(x => x.Room != npc.Room))
                    ch.Send("You hear a shrill whistling sound.");
            }
            // say something, then raise hell
            var message = RandomManager.Range(0, 6) switch
            {
                0 => "{0:N} yells 'All roit! All roit! break it up!'",
                1 => "{0:N} says 'Society's to blame, but what's a bloke to do?'.",
                2 => "{0:N} mumbles 'bloody kids will be the death of us all.'",
                3 => "{0:N} shouts 'Stop that! Stop that!' and attacks.",
                4 => "{0:N} pulls out his billy and goes to work.",
                5 => "{0:N} sighs in resignation and proceeds to break up the fight.",
                6 => "{0:N} says 'Settle down, you hooligans!'",
                _ => "{0:N} attacks."
            };

            npc.Act(ActOptions.ToAll, message, npc);
            npc.MultiHit(victim);

            return true;
        }
    }
}
