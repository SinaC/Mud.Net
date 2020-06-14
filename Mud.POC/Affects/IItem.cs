namespace Mud.POC.Affects
{
    public interface IItem : IEntity
    {
        IEntity ContainedIn { get; }
        ICharacter EquippedBy { get; }

        ItemFlags BaseItemFlags { get; }
        ItemFlags CurrentItemFlags { get; }

        void ApplyAffect(ItemFlagsAffect affect);
    }
}
