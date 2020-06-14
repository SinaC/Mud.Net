using Mud.POC.Abilities2.ExistingCode;

namespace Mud.POC.Abilities2.Rom24Effects
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
