using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction;

public abstract class CharacterGameActionBase<TCharacter, TCharacterGameActionInfo> : GameActionBase<TCharacter, TCharacterGameActionInfo>
    where TCharacter: class, ICharacter
    where TCharacterGameActionInfo: class, ICharacterGameActionInfo
{
    public override string? Guards(IActionInput actionInput)
    {
        var baseGuards = base.Guards(actionInput);
        if (baseGuards != null)
            return baseGuards;

        // When hiding, anything will break it
        if (Actor.CharacterFlags.IsSet("Hide"))
        {
            Actor.RemoveBaseCharacterFlags(false, "Hide");
            Actor.Recompute();
        }

        // Check fighting
        if (GameActionInfo.NotInCombat && Actor.Fighting != null)
            return "No way!  You are still fighting!";

        // Check minimum position
        if (Actor.Position < GameActionInfo.MinPosition)
        {
            switch (Actor.Position)
            {
                case Positions.Sleeping:
                    return "In your dreams, or what?";
                case Positions.Resting:
                    return "Nah... You feel too relaxed...";
                case Positions.Sitting:
                    return "Better stand up first.";
            }
        }

        // Check stun
        if (Actor.Stunned > 0)
            return "You're still a little woozy.";

        return null;
    }
}
