using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.Movement
{
    [CharacterCommand("wake", "Movement", MinPosition = Positions.Sleeping)]
    [Syntax(
            "[cmd]",
            "[cmd] <character>")]
    public class Wake : CharacterGameAction
    {
        private IGameActionManager GameActionManager { get; }

        public ICharacter Whom { get; protected set; }

        public Wake(IGameActionManager gameActionManager)
        {
            GameActionManager = gameActionManager;
        }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return null;

            if (Actor.Position <= Positions.Sleeping)
                return "You are asleep yourself!";

            Whom = FindHelpers.FindByName(Actor.Room.People.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;
            if (Whom.Position > Positions.Sleeping)
                return Actor.ActPhrase("{0:N} is already awake.", Whom);
            if (Whom.CharacterFlags.IsSet("Sleep"))
                return Actor.ActPhrase("You can't wake {0:m}!", Whom);
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            if (Whom == null)
            {
                string executionResults = GameActionManager.Execute<Stand, ICharacter>(Actor, null);
                if (executionResults != null)
                    Actor.Send(executionResults);
            }
            else
            {
                Whom.Act(ActOptions.ToCharacter, "{0:N} wakes you.", Actor);

                string executionResults = GameActionManager.Execute<Stand, ICharacter>(Whom, null);
                if (executionResults != null)
                    Actor.Send(executionResults);
            }
        }
    }
}
