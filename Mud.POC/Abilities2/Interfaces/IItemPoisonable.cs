namespace Mud.POC.Abilities2.Interfaces
{
    public interface IItemPoisonable : IItem
    {
        bool IsPoisoned { get; }

        void Poison();
        void Cure();
    }
}
