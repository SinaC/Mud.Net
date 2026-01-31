using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class CharacterGameActionBase<TCharacter, TCharacterGameActionInfo> : ActorGameActionBase<TCharacter, TCharacterGameActionInfo>
    where TCharacter: class, ICharacter
    where TCharacterGameActionInfo: class, ICharacterGameActionInfo
{
    public override string? CanExecute(IActionInput actionInput)
    {
        var baseGuards = base.CanExecute(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // When hiding, anything will break it
        if (Actor.CharacterFlags.IsSet("Hide"))
        {
            Actor.RemoveBaseCharacterFlags(false, "Hide");
            Actor.Recompute();
        }

        // Check stun
        if (Actor.IsStunned)
            return "You're still a little woozy.";

        return null;
    }
}
