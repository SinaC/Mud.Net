using Mud.Domain;
using Mud.Domain.Extensions;
using Mud.Server.Ability;
using Mud.Server.Ability.Skill;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Interfaces;
using Mud.Server.Interfaces.Ability;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Item;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Skills;

[CharacterCommand("pick", "Ability", "Skill")]
[Syntax(
        "[cmd] <direction>",
        "[cmd] <door>",
        "[cmd] <container>|<portal>")]
[Skill(SkillName, AbilityEffects.None, LearnDifficultyMultiplier = 2)]
public class PickLock : SkillBase
{
    private const string SkillName = "Pick Lock";

    public PickLock(IRandomManager randomManager)
        : base(randomManager)
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
        if (!RandomManager.Chance(chance) && (User as IPlayableCharacter)?.IsImmortal != true)
        {
            User.Send("You failed.");
            return false;
        }
        Closeable.Unlock();
        User.Act(ActOptions.ToAll, "{0:N} picks the lock on {1}.", User, Closeable);
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
        if (closeable.IsPickProof && (User as IPlayableCharacter)?.IsImmortal != true)
            return "You failed.";
        return null;
    }
}
