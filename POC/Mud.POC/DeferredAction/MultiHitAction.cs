using System;
using System.Collections.Generic;
using System.Text;

namespace Mud.POC.DeferredAction;

public class MultiHitAction : IGameAction
{
    private readonly Mob _attacker;

    public MultiHitAction(Mob attacker)
    {
        _attacker = attacker;
    }

    public void Execute(World world)
    {
        _attacker.BreakStealth(world);

        int attacks = _attacker.GetNumberOfAttacks();
        for (int i = 0; i < attacks; i++)
        {
            var target = _attacker.CurrentTarget;

            if (target == null || target.IsDead)
                break;

            world.Enqueue(new AttackAction(_attacker, target));
        }
    }
}


//public class MultiHitAction : IGameAction
//{
//    private readonly Mob _attacker;

//    public MultiHitAction(Mob attacker)
//    {
//        _attacker = attacker;
//    }

//    public void Execute(World world)
//    {
//        if (!_attacker.CanAct) return;

//        int attacks = _attacker.GetNumberOfAttacks();

//        _attacker.BreakStealth(world);

//        for (int i = 0; i < attacks; i++)
//        {
//            var target = _attacker.CurrentTarget;

//            if (target == null || target.IsDead)
//                break;

//            world.Enqueue(new AttackAction(_attacker, target));
//        }
//    }

//}

//public class MultiHitAction : IGameAction
//{
//    private readonly Mob _attacker;

//    public MultiHitAction(Mob attacker)
//    {
//        _attacker = attacker;
//    }

//    public void Execute(World world)
//    {
//        if (!_attacker.CanAct)
//            return;

//        var victim = _attacker.CurrentTarget;

//        if (!IsValidCombatPair(_attacker, victim))
//            return;

//        // ROM: primary attack always happens
//        world.Enqueue(new SingleHitAction(_attacker, victim, isOffhand: false));

//        // ROM-style extra attacks
//        TryEnqueueExtraAttacks(world, victim);
//    }

//    private void TryEnqueueExtraAttacks(World world, Mob victim)
//    {
//        int extraAttacks = CalculateExtraAttacks(_attacker);

//        for (int i = 0; i < extraAttacks; i++)
//        {
//            world.Enqueue(new ConditionalFollowupAttack(_attacker));
//        }
//    }

//    private bool IsValidCombatPair(Mob ch, Mob victim)
//    {
//        if (victim == null) return false;
//        if (victim.IsDead) return false;
//        if (victim.CurrentRoom != ch.CurrentRoom) return false;
//        return true;
//    }

//    private int CalculateExtraAttacks(Mob ch)
//    {
//        int attacks = 0;

//        // Replace with ROM-style skill rolls later
//        attacks += ch.HasSecondAttack ? 1 : 0;
//        attacks += ch.HasThirdAttack ? 1 : 0;
//        attacks += ch.HasFourthAttack ? 1 : 0;

//        if (ch.IsHasted)
//            attacks += 1;

//        return attacks;
//    }
//}