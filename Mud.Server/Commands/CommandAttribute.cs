using System;

namespace Mud.Server.Commands
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandOutOfGameAttribute : Attribute
    {
        public Type AllowedEntityType { get; private set; }

        public CommandOutOfGameAttribute(Type allowedEntityType)
        {
            AllowedEntityType = allowedEntityType;
        }
    }
}
