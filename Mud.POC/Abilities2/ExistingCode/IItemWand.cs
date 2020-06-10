namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemWand : IItem
    {
        int SpellLevel { get; }
        int MaxChargeCount { get; }
        int CurrentChargeCount { get; }
        string SpellName { get; }
        bool AlreadyRecharged { get; }

        void Use();
        void Recharge(int currentChargeCount, int maxChargeCount);
    }
}
