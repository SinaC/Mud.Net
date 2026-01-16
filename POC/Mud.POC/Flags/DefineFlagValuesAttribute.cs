namespace Mud.POC.Flags
{

    [AttributeUsage(AttributeTargets.Class,  AllowMultiple = false)]
    public class DefineFlagValuesAttribute : Attribute
    {
        public Type FlagInterfaceType { get; }

        public DefineFlagValuesAttribute(Type flagInterfaceType)
        {
            FlagInterfaceType = flagInterfaceType;
        }
    }
}
