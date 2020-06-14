namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemPoisonable : IItem
    {
        bool IsPoisoned { get; }

        void Poison();
        void Cure();
    }
}
