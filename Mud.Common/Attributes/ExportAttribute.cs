namespace Mud.Common.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ExportAttribute : Attribute
    {
        public ExportAttribute() : this(null, null)
        {
        }

        public ExportAttribute(Type contractType) : this(null, contractType)
        {
        }

        public ExportAttribute(string contractName) : this(contractName, null)
        {
        }

        public ExportAttribute(string? contractName, Type? contractType)
        {
            ContractName = contractName;
            ContractType = contractType;
        }

        public string? ContractName { get; }

        public Type? ContractType { get; }
    }
}
