namespace Mud.POC.Abilities2.ExistingCode
{
    public interface IItemPortal : IItem
    {
        void ChangeDestination(IRoom destination);
        void SetCharge(int current, int max);
    }
}
