using Mud.Domain;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public abstract class CharacterGameActionBase<TCharacter, TCharacterGameActionInfo> : GameActionBase<TCharacter, TCharacterGameActionInfo>
        where TCharacter: class, ICharacter
        where TCharacterGameActionInfo: class, ICharacterGameActionInfo
    {
        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            // When hiding, anything will break it
            if (Actor.CharacterFlags.IsSet("Hide"))
            {
                Actor.RemoveBaseCharacterFlags("Hide");
                Actor.Recompute();
            }

            // Check minimum position
            if (Actor.Position < GameActionInfo.MinPosition)
            {
                switch (Actor.Position)
                {
                    case Positions.Dead:
                        return "Lie still; you are DEAD.";
                    case Positions.Mortal:
                    case Positions.Incap:
                        return "You are hurt far too bad for that.";
                    case Positions.Stunned:
                        return "You are too stunned to do that.";
                    case Positions.Sleeping:
                        return "In your dreams, or what?";
                    case Positions.Resting:
                        return "Nah... You feel too relaxed...";
                    case Positions.Sitting:
                        return "Better stand up first.";
                    case Positions.Fighting:
                        return "No way!  You are still fighting!";
                }
            }

            return null;
        }
    }
}
