﻿namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemCastSpellsCharge : IItem
    {
        int SpellLevel { get; }
        int MaxChargeCount { get; }
        int CurrentChargeCount { get; }
        IAbility Spell { get; }
        bool AlreadyRecharged { get; }

        void Use();
        void Recharge(int currentChargeCount, int maxChargeCount);
    }
}
