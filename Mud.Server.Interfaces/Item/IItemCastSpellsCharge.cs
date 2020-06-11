using Mud.Server.Interfaces.Ability;

namespace Mud.Server.Interfaces.Item
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
