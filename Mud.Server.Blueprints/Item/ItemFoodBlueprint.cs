using System.Runtime.Serialization;

namespace Mud.Server.Blueprints.Item
{
    [DataContract]
    public class ItemFoodBlueprint : ItemBlueprintBase
    {
        [DataMember]
        public int FullHour { get; set; }

        [DataMember]
        public int HungerHour { get; set; }

        [DataMember]
        public bool IsPoisoned { get; set; }
    }
}
