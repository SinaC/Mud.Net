using Mud.Common.Attributes;

namespace Mud.Server.Item
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ItemAttribute : ExportAttribute // every affect will be exported without ContractType
    {
        public Type BlueprintType { get; set; }
        public Type ItemDataType { get; set; }

        public ItemAttribute(Type blueprintType, Type itemDataType)
        {
            BlueprintType = blueprintType;
            ItemDataType = itemDataType;
        }
    }
}
