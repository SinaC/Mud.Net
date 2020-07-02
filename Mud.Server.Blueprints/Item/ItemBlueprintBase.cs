using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Mud.Domain;
using Mud.Server.Flags.Interfaces;

namespace Mud.Server.Blueprints.Item
{
    [DataContract]
    [KnownType(typeof(ItemArmorBlueprint))]
    [KnownType(typeof(ItemContainerBlueprint))]
    [KnownType(typeof(ItemCorpseBlueprint))]
    [KnownType(typeof(ItemFurnitureBlueprint))]
    [KnownType(typeof(ItemJewelryBlueprint))]
    [KnownType(typeof(ItemKeyBlueprint))]
    [KnownType(typeof(ItemLightBlueprint))]
    [KnownType(typeof(ItemPortalBlueprint))]
    [KnownType(typeof(ItemQuestBlueprint))]
    [KnownType(typeof(ItemShieldBlueprint))]
    [KnownType(typeof(ItemWeaponBlueprint))]
    public abstract class ItemBlueprintBase
    {
        [DataMember]
        public int Id { get; set; }
        
        [DataMember]
        public string Name { get; set; }
        
        [DataMember]
        public string ShortDescription { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public int Level { get; set; }

        [DataMember]
        public int Weight { get; set; }

        [DataMember]
        public int Cost { get; set; }

        [DataMember]
        public bool NoTake { get; set; }

        [DataMember]
        public Lookup<string, string> ExtraDescriptions { get; set; } // keyword -> descriptions

        // TODO: flags, level, ...

        [DataMember]
        public WearLocations WearLocation { get; set; }

        [DataMember]
        public IItemFlags ItemFlags { get; set; }

        public static Lookup<string,string> BuildExtraDescriptions(IEnumerable<KeyValuePair<string, string>> extraDescriptions)
        {
            return (Lookup<string, string>)extraDescriptions.SelectMany(x => x.Key.Split(' '), (kv, key) => new { key, desc = kv.Value }).ToLookup(x => x.key, x => x.desc);
        }

        public bool Equals(ItemBlueprintBase other)
        {
            return other != null && Id == other.Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as ItemBlueprintBase);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
