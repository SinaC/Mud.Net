using Microsoft.Extensions.Options;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Domain.Attributes;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Special;

namespace Mud.Server.Rom24.Specials;

[SpecialBehavior("spec_troll_member")]
public class TrollMember : ISpecialBehavior
{
    private IRandomManager RandomManager { get; }
    private DangerousNeighborhood Options { get; }

    public TrollMember(IRandomManager randomManager,  IOptions<Rom24Options> rom24Options)
    {
        RandomManager = randomManager;
        Options = rom24Options.Value.DangerousNeighborhood;
    }

    public bool Execute(INonPlayableCharacter npc)
    {
        // must be awake, not charmed neither calm
        if (!npc.IsValid ||  npc.Room == null
            || npc.Position <= Positions.Sleeping || npc.CharacterFlags.IsSet("Charm") || npc.CharacterFlags.IsSet("Calm")
            || npc.Fighting != null)
            return false;
        // if patrol man in the room, dont do anything
        if (npc.Room.People.OfType<INonPlayableCharacter>().Any(x => x != npc && x.Blueprint.Id == Options.PatrolMan))
            return false;
        // find an ogre to beat up
        var victims = npc.Room.People.OfType<INonPlayableCharacter>().Where(victim => victim != npc && victim.Blueprint.Group == Options.Ogres && npc.Level > victim.Level - 2 && victim.IsSafe(npc) == null).ToList();
        if (victims.Count == 0)
            return false;
        var victim = RandomManager.Random(victims);
        if (victim == null)
            return false;

        // say something, then raise hell
        var message = RandomManager.Range(0, 6) switch
        {
            0 => "{0:N} yells 'I've been looking for you, punk!'",
            1 => "With a scream of rage, {0:N} attacks {1:N}.",
            2 => "{0:N} says 'What's Troll filth like you doing around here?'",
            3 => "{0:N} cracks his knuckles and says 'Do ya feel lucky?'",
            4 => "{0:N} says 'There's no cops to save you this time!'",
            5 => "{0:N} says 'Time to join your brother, spud.'",
            6 => "{0:N} says 'Let's rock.'",
            _ => "{0:N} attacks {1:N}"
        };

        npc.Act(ActOptions.ToAll, message, npc, victim);
        npc.MultiHit(victim);

        return true;
    }
}
