using Mud.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;

namespace Mud.Server.Rom24.Effects;

[Effect("Refresh")]
public class RefreshEffect : IEffect<ICharacter>
{
    public void Apply(ICharacter victim, IEntity source, string abilityName, int level, int _)
    {
        victim.UpdateResource(ResourceKinds.MovePoints, level);
        if (victim[ResourceKinds.MovePoints] == victim.MaxResource(ResourceKinds.MovePoints))
            victim.Send("You feel fully refreshed!");
        else
            victim.Send("You feel less tired.");
        if (source != victim)
            source.Send("Ok");
    }
}
