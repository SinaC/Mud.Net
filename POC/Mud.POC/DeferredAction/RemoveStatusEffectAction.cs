using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class RemoveStatusEffectAction : IGameAction
{
    private readonly Mob _target;
    private readonly StatusEffect.StatusEffect _effect;

    public RemoveStatusEffectAction(Mob target, StatusEffect.StatusEffect effect)
    {
        _target = target;
        _effect = effect;
    }

    public void Execute(World world)
    {
        if (_target.StatusEffects.Contains(_effect))
        {
            _target.StatusEffects.Remove(_effect);
            world.Enqueue(new ScriptAction(ctx => ctx.Notify($"{_effect.GetType().Name} has worn off from {_target}.")));
        }
    }
}
