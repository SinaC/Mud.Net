using Microsoft.Extensions.Logging;
using Mud.Domain;
using Mud.Random;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common.Attributes;
using Mud.Server.Common.Extensions;
using Mud.Server.Common.Helpers;
using Mud.Server.Domain;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("pick", "Ability", "Skill")]
[Syntax(
        "[cmd] <direction>",
        "[cmd] <door>",
        "[cmd] <container>|<portal>")]
[Skill(SkillName, AbilityEffects.None, LearnDifficultyMultiplier = 2)]
[Help(
@"Lock picking is one of the prime skills of thieves, allowing them to gain
access to many secured areas.  Lock picking chances are improved by  
intelligence, and hindered by the difficulty of the lock. Other classes may
learn to pick locks, but they will never find it easy.")]
[OneLineHelp("a useful skill for breaking and entering")]
public class PickLock : SkillBase
{
    private const string SkillName = "Pick Lock";

    public PickLock(ILogger<PickLock> logger, IRandomManager randomManager)
        : base(logger, randomManager)
    {
    }

    protected ICloseable Closeable { get; set; } = default!;

    public override string? Setup(ISkillActionInput skillActionInput)
    {
        var baseSetup = base.Setup(skillActionInput);
        if (baseSetup != null)
            return baseSetup;

        // Look for guards
        var guard = User.Room.NonPlayableCharacters.FirstOrDefault(x => x.Position > Positions.Sleeping && x.Level > User.Level + 5);
        if (guard != null)
            return User.ActPhrase("{0:N} is standing too close to the lock.", guard);

        return null;
    }

    protected override bool Invoke()
    {
        int chance = Learned;
        if (Closeable.IsEasy)
            chance *= 2;
        if (Closeable.IsHard)
            chance /= 2;
        if (!RandomManager.Chance(chance) && !User.ImmortalMode.IsSet("PassThru"))
        {
            User.Send("%W%You failed.%x%");
            return false;
        }
        Closeable.Unlock();
        User.Act(ActOptions.ToAll, "%W%{0:N} picks the lock on {1}.%x%", User, Closeable);
        return true;
    }

    protected override string? SetTargets(ISkillActionInput skillActionInput)
    {
        if (skillActionInput.Parameters.Length == 0)
            return "Pick what?";

        // search item
        var item = FindHelpers.FindItemHere(User, skillActionInput.Parameters[0]);
        if (item != null)
        {
            if (item is not IItemCloseable itemCloseable)
                return "You cannot pick that.";
            var checkItem = CheckCloseable(itemCloseable);
            // item found and can be picked
            return checkItem;
        }

        // search exit
        if (ExitDirectionsExtensions.TryFindDirection(skillActionInput.Parameters[0].Value, out var direction))
        {
            var exit = User.Room[direction];
            if (exit == null)
                return "Nothing special there.";
            var checkExit = CheckCloseable(exit);
            // exit found and can be picked
            return checkExit;
        }

        return "You cannot pick that";
    }

    private string? CheckCloseable(ICloseable closeable)
    {
        if (!closeable.IsCloseable)
            return "You can't do that.";
        if (!closeable.IsClosed)
            return "It's not closed.";
        if (!closeable.IsLockable)
            return "It can't be unlocked.";
        if (closeable.IsPickProof && !User.ImmortalMode.IsSet("PassThru"))
            return "You failed.";
        return null;
    }
}
