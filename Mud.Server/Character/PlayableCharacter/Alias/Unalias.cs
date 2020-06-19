using Mud.Server.GameAction;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.Character.PlayableCharacter.Alias
{
    // TODO: exactly the same code in Mud.Server.Player.Alias
    [PlayableCharacterCommand("unmacro", "Misc")]
    [PlayableCharacterCommand("unalias", "Misc")]
    [Syntax("[cmd] <word>")]
    public class Unalias : PlayableCharacterGameAction
    {
        public string TargetAlias { get; protected set; }

        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (actionInput.Parameters.Length == 0)
                return "Unalias what?";
            TargetAlias = actionInput.Parameters[0].Value.ToLowerInvariant().Trim();
            if (!Actor.Aliases.ContainsKey(TargetAlias))
                return "No alias of that name to remove.";
            return null;
        }

        public override void Execute(IActionInput actionInput)
        {
            Actor.RemoveAlias(TargetAlias);
            Actor.Send("Alias removed.");
        }
    }
}
