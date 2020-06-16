using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.GameAction;

namespace Mud.Server.GameAction
{
    public abstract class PlayableCharacterGameAction : CharacterGameActionBase<IPlayableCharacter, IPlayableCharacterGameActionInfo>
    {
        public override string Guards(IActionInput actionInput)
        {
            string baseGuards = base.Guards(actionInput);
            if (baseGuards != null)
                return baseGuards;

            if (Actor.ImpersonatedBy == null)
                return $"You must be impersonated to use '{GameActionInfo.Name}'.";

            if (Actor.ImpersonatedBy.IsAfk)
                Actor.ImpersonatedBy.ToggleAfk();

            return null;
        }
    }
}
