using System.Linq;
using Mud.Domain;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.Communication
{
    [CharacterCommand("whisper", "Communication", MinPosition = Positions.Standing)]
    [Syntax("[cmd] <character> <message>")]
    public class Whisper : CharacterGameAction
    {
        public ICharacter Whom { get; protected set; }
        public string What { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length <= 1)
                return "Whisper whom what?";
                
            Whom = FindHelpers.FindByName(Actor.Room.People, actionInput.Parameters[0]);
            if (Whom == null)
                return StringHelpers.CharacterNotFound;

            What = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.Act(ActOptions.ToCharacter, "You whisper '{0}' to {1:n}.", What, Whom);
            if (Actor != Whom)
                Whom.Act(ActOptions.ToCharacter, "{0:n} whispers you '{1}'.", this, What); // TODO: when used on ourself (player is pouet), pouet whispers you 'blabla'
            Actor.ActToNotVictim(Whom, "{0:n} whispers something to {1:n}.", this, Whom);
            // ActOptions.ToAll cannot be used because 'something' is sent except for 'this' and 'whom'
        }
    }
}
