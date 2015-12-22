using System;

namespace Mud.Server
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandWithTargetAttribute : CommandAttribute
    {
    }
}
