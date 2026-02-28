namespace Mud.POC.DeferredAction;

public abstract class Entity
{
    public Guid Id { get; } = Guid.NewGuid();
    public bool IsDeleted { get; set; }
}
