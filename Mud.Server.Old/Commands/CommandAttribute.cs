using System;

namespace Mud.Server.Old.Commands
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
