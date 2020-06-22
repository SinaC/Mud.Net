using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects
{
    public class RefreshEffect : IEffect<ICharacter>
    {
        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            victim.UpdateMovePoints(level);
            if (victim.MovePoints == victim.MaxMovePoints)
                victim.Send("You feel fully refreshed!");
            else
                victim.Send("You feel less tired.");
            if (source != victim)
                victim.Send("Ok");
        }
    }
}
