using Mud.Domain;
using Mud.Server.Domain;

namespace Mud.Server.Guards.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class GuardAttributeBase : Attribute
{
}

// Actor
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class ActorGuardAttributeBase : GuardAttributeBase
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public class NoArgumentGuardAttribute : ActorGuardAttributeBase
{
    public string? Message { get; set; } // if set, this message is returned when the guard fails, otherwise BuildCommandSyntax is used

    public NoArgumentGuardAttribute()
    {
    }

    public NoArgumentGuardAttribute(string message)
    {
        Message = message;
    }
}

// Character
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class CharacterGuardAttributeBase : GuardAttributeBase
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MinPositionAttribute(Positions minimumPosition) : CharacterGuardAttributeBase
{
    public Positions MinimumPosition { get; } = minimumPosition;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NotInCombatAttribute : CharacterGuardAttributeBase
{
    public string? Message { get; set; }

}
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class InCombatAttribute : CharacterGuardAttributeBase
{
    public string? Message { get; set; }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class ShapesAttribute(Shapes[] shapes) : CharacterGuardAttributeBase
{
    public Shapes[] Shapes { get; } = shapes;
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class NoShapeshiftAttribute : CharacterGuardAttributeBase
{
}

// Player
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class PlayerGuardAttributeBase : GuardAttributeBase
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MustBeImpersonatedAttribute : PlayerGuardAttributeBase
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CannotBeImpersonatedAttribute : PlayerGuardAttributeBase
{
}

// Admin
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public abstract class AdminGuardAttributeBase : GuardAttributeBase
{
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class MinAdminLevelAttribute(AdminLevels minLevel) : AdminGuardAttributeBase
{
    public AdminLevels MinLevel { get; } = minLevel;
}


