using Mud.POC.Abilities2.Domain;
using Mud.POC.Abilities2.ExistingCode;
using Mud.POC.Abilities2.Helpers;
using Mud.Server.Random;
using Mud.Server.GameAction;
using System.Linq;

namespace Mud.POC.Abilities2.Rom24Skills
{
    [Command("pick", "Skills")]
    [Skill(SkillName, AbilityEffects.None, LearnDifficultyMultiplier = 2)]
    public class PickLock : SkillBase
    {
        public const string SkillName = "Pick Lock";

        protected ICloseable Closeable { get; set; }

        public PickLock(IRandomManager randomManager)
            : base(randomManager)
        {
        }

        public override string Setup(ISkillActionInput skillActionInput)
        {
            string baseSetup = base.Setup(skillActionInput);
            if (baseSetup != null)
                return baseSetup;

            // Look for guards
            INonPlayableCharacter guard = User.Room.NonPlayableCharacters.FirstOrDefault(x => x.Position > Positions.Sleeping && x.Level > User.Level + 5);
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

        protected override string SetTargets(ISkillActionInput skillActionInput)
        {
            if (skillActionInput.Parameters.Length == 0)
                return "Pick what?";

            // search item
            IItem item = FindHelpers.FindItemHere(User, skillActionInput.Parameters[0]);
            if (item != null)
            {
                IItemCloseable itemCloseable = item as IItemCloseable;
                string checkItem = CheckCloseable(itemCloseable);
                if (checkItem != null)
                    return checkItem;

                // item found and can be picked
                return null;
            }

            // search exit
            ExitDirections direction;
            if (ExitDirectionsExtensions.TryFindDirection(skillActionInput.Parameters[0].Value, out direction))
            {
                IExit exit = User.Room[direction];
                if (exit == null)
                    return "Nothing special there.";
                string checkExit = CheckCloseable(exit);
                if (checkExit != null)
                    return checkExit;

                // exit found and can be picked
                return null;
            }

            return "You cannot pick that";
        }

        private string CheckCloseable(ICloseable closeable)
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
}
