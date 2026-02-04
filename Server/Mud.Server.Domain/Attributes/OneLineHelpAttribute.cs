namespace Mud.Server.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OneLineHelpAttribute(string oneLineHelp) : Attribute
    {
        public string OneLineHelp { get; } = oneLineHelp;
    }
}
