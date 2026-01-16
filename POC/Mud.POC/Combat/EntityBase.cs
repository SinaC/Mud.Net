namespace Mud.POC.Combat
{
    public class EntityBase : IEntity
    {
        public bool IsValid { get; protected set; }
    }

    public interface IEntity
    {
        bool IsValid { get; }
    }
}
