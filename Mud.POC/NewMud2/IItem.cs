namespace Mud.POC.NewMud2
{
    public interface IItem : IEntity
    {
        IEntity Location { get; }
    }
}
