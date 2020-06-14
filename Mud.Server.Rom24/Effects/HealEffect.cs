using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects
{
    public class HealEffect : IEffect<ICharacter>
    {
        public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
        {
            victim.UpdateHitPoints(100);
            victim.Send("A warm feeling fills your body.");
            if (source != victim)
                source.Send("Ok.");
        }
    }
}
