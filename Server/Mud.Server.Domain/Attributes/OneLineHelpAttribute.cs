namespace Mud.Server.Domain.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OneLineHelpAttribute : Attribute
    {
        public string OneLineHelp { get; }

        public OneLineHelpAttribute(string oneLineHelp)
        {
            OneLineHelp = oneLineHelp;
        }
    }
}
