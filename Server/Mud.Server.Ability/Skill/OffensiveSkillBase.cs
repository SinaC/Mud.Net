using Microsoft.Extensions.Logging;
using Mud.Server.Common.Helpers;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Random;

namespace Mud.Server.Ability.Skill;

public abstract class OffensiveSkillBase : SkillBase
{
    protected OffensiveSkillBase(ILogger<OffensiveSkillBase> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }
    protected ICharacter Victim { get; set; } = default!;

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length < 1)
        {
            Victim = User.Fighting!;
            if (Victim == null)
                return "Use the skill on whom?";
        }
        else
            Victim = FindHelpers.FindByName(User.Room.People, skillActionInput.Parameters[0])!;
        if (Victim == null)
            return StringHelpers.CharacterNotFound;
        if (User is IPlayableCharacter)
        {
            if (User != Victim)
            {
                var safeResult = Victim.IsSafe(User);
                if (safeResult != null)
                    return safeResult;
            }
            // TODO: check_killer
        }
        if (User is INonPlayableCharacter npcCaster && npcCaster.CharacterFlags.IsSet("Charm") && npcCaster.Master == Victim)
            return "You can't do that on your own follower.";
        // victim found
        return null;
    }

    public override void Execute()
    {
        base.Execute();

        var npcVictim = Victim as INonPlayableCharacter;
        if (Victim != User
            && npcVictim?.Master != User) // avoid attacking caster when successfully charmed
        {
            // check if victim is still in the room and not yet fighting user
            // if victim found, we allow it to multi hit user
            var victimStartFightaingAgainstUser = User.Room.People.FirstOrDefault(x => x == Victim && x.Fighting == null);
            if (victimStartFightaingAgainstUser != null)
            {
                // TODO: check_killer
                victimStartFightaingAgainstUser.MultiHit(User);
            }
        }
    }
}
