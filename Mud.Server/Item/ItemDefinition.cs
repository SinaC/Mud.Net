using System.Reflection;

namespace Mud.Server.Item
{
    public class ItemDefinition
    {
        public Type ItemType { get; }
        public Type BlueprintType { get; }
        public Type ItemDataType { get;  }
        public MethodInfo InitializeWithoutItemDataMethod { get; }
        public MethodInfo InitializeWithItemDataMethod { get; }

        public ItemDefinition(Type itemType, Type blueprintType, Type itemDataType, MethodInfo initializeWithoutItemDataMethod, MethodInfo initializeWithItemDataMethod)
        {
            ItemType = itemType;
            BlueprintType = blueprintType;
            ItemDataType = itemDataType;
            InitializeWithoutItemDataMethod = initializeWithoutItemDataMethod;
            InitializeWithItemDataMethod = initializeWithItemDataMethod;
        }
    }
}
