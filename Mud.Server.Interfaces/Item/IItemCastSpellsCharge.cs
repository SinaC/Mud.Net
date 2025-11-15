namespace Mud.Server.Interfaces.Item;

public interface IItemCastSpellsCharge : IItem
{
    int SpellLevel { get; }
    int MaxChargeCount { get; }
    int CurrentChargeCount { get; }
    string SpellName { get; }
    bool AlreadyRecharged { get; }

    void Use();
    void Recharge(int currentChargeCount, int maxChargeCount);
}
