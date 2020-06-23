using Mud.Common;
using Mud.Server.Common;
using Mud.Server.GameAction;
using Mud.Server.Helpers;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;
using System.Linq;

namespace Mud.Server.Character.PlayableCharacter.Group
{
    [PlayableCharacterCommand("order", "Pet")]
    [Syntax(
            "[cmd] <pet|charmie> command",
            "[cmd] all command")]
    public class Order : PlayableCharacterGameAction
    {
        public INonPlayableCharacter[] Whom { get; protected set; }
        public string CommandLine { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length < 2)
                return "Order whom to do what?";

            // Select target(s)
            if (actionInput.Parameters[0].IsAll)
                Whom = Actor.Room.NonPlayableCharacters.Where(x => x.Master == Actor && x.CharacterFlags.HasFlag(Domain.CharacterFlags.Charm)).ToArray();
            else
            {
                INonPlayableCharacter target = FindHelpers.FindByName(Actor.Room.NonPlayableCharacters.Where(x => Actor.CanSee(x)), actionInput.Parameters[0]);
                if (target == null)
                    return StringHelpers.CharacterNotFound;

                if (target.Master != Actor || !target.CharacterFlags.HasFlag(Domain.CharacterFlags.Charm))
                    return "Do it yourself!";
                Whom = target.Yield().ToArray();
            }

            if (Whom.Length == 0)
                return "You don't have followers here.";

            CommandLine = CommandHelpers.JoinParameters(actionInput.Parameters.Skip(1));
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            foreach (INonPlayableCharacter target in Whom)
            {
                Actor.Act(ActOptions.ToCharacter, "You order {0:N} to '{1}'.", target, CommandLine);
                target.Order(CommandLine);
            }

            Actor.ImpersonatedBy?.SetGlobalCooldown(Pulse.PulseViolence);
            Actor.Send("Ok.");
        }
    }
}
