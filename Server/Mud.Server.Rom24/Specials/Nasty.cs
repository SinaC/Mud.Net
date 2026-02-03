using Mud.Domain;
using Mud.Random;
using Mud.Server.Commands.Character.Combat;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Special;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_nasty")]
public class Nasty : ISpecialBehavior
{
    private IRandomManager RandomManager { get; }
    private IGameActionManager GameActionManager { get; }

    public Nasty(IRandomManager randomManager, IGameActionManager gameActionManager)
    {
        RandomManager = randomManager;
        GameActionManager = gameActionManager;
    }

    public bool Execute(INonPlayableCharacter npc)
    {
        // must be awake
        if (!npc.IsValid || npc.Room == null
            || npc.Position <= Positions.Sleeping)
            return false;

        // if not fighting, backstab
        if (npc.Fighting == null)
        {
            var pcVictims = npc.Room.People.OfType<IPlayableCharacter>().Where(pc => pc.Level > npc.Level && pc.Level < npc.Level + 10).ToArray();
            var pcVictim = RandomManager.Random(pcVictims);
            if (pcVictim == null)
                return false; // no one to attack

            // try to backstab
            var executeResult = GameActionManager.Execute<Skills.Backstab, ICharacter>(npc, pcVictim.Name); // TODO: what happens if mob doesnt know how to backstab
            return true;
        }
        // if fighting, steal some coins and flee
        else
        {
            var victim = npc.Fighting;
            if (victim == null) // should never happen
                return false;
            var chance = RandomManager.Next(4);
            if (chance == 0) // steal some gold (10%)
            {
                victim.Act(ActOptions.ToCharacter, "{0:N} rips apart your coin purse, spilling your gold!", npc);
                npc.Act(ActOptions.ToCharacter, "You slash apart {0:N}'s coin purse and gather his gold.", victim);
                npc.ActToNotVictim(victim, "{0:N}'s coin purse is ripped apart!", victim);
                var theft = victim.GoldCoins / 10;
                var (_, availableGold) = victim.StealMoney(0, theft);
                victim.UpdateMoney(0, availableGold);
                return true;
            }
            else if (chance == 1) // flee
            {
                var executeResult = GameActionManager.Execute<Flee, ICharacter>(npc, string.Empty);
                return true;
            }
            return false;
        }
    }
}
