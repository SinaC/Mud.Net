using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;
using Mud.Server.Random;
using Mud.Server.Specials;

namespace Mud.Server.Rom24.Specials
{
    [SpecialBehavior("spec_thief")]
    public class Thief : ISpecialBehavior
    {
        private IRandomManager RandomManager { get; }

        public Thief(IRandomManager randomManager)
        {
            RandomManager = randomManager;
        }

        public bool Execute(INonPlayableCharacter npc)
        {
            // must be standing
            if (!npc.IsValid || npc.Room == null
                || npc.Position != Positions.Standing)
                return false;

            // search a PC victim
            var victim = npc.Room.People.OfType<IPlayableCharacter>().FirstOrDefault(x => !x.IsImmortal && RandomManager.Next(32) == 0 && npc.CanSee(x));
            if (victim == null)
                return false;

            // failed: funny message
            if (victim.Position >= Positions.Sleeping && RandomManager.Range(0, npc.Level) == 0)
            {
                victim.Act(ActOptions.ToCharacter, "You discover $n's hands in your wallet!", npc);
                npc.ActToNotVictim(victim, "{0:N} discovers {1:N}'s hands in {0:s} wallet!", victim, npc);
                return true;
            }
            // succeed: get some gold
            else
            {
                var goldTheft = Math.Min(victim.GoldCoins * Math.Min(RandomManager.Range(1, 20), npc.Level / 2) / 100, npc.Level * npc.Level * 10);
                var availableGold = victim.DecrementGold(goldTheft);
                npc.IncrementGold(availableGold);

                var silverTheft = Math.Min(victim.SilverCoins * Math.Min(RandomManager.Range(1, 20), npc.Level / 2) / 100, npc.Level * npc.Level * 25);
                var availableSilver = victim.DecrementSilver(goldTheft);
                npc.IncrementSilver(availableGold);
                return true;
            }
        }
    }
}
