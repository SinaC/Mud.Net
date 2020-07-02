using Mud.Server.Flags.Interfaces;
using System.Collections.Generic;

namespace Mud.Domain
{
    public abstract class CharacterData
    {
        public string Name { get; set; }

        public string Race { get; set; }

        public string Class { get; set; }

        public int Level { get; set; }

        public Sex Sex { get; set; }

        public Sizes Size { get; set; }

        public int HitPoints { get; set; }

        public int MovePoints { get; set; }

        public Dictionary<ResourceKinds, int> CurrentResources { get; set; }

        public Dictionary<ResourceKinds, int> MaxResources { get; set; }

        public EquippedItemData[] Equipments { get; set; }

        public ItemData[] Inventory { get; set; }

        public AuraData[] Auras { get; set; }

        public ICharacterFlags CharacterFlags { get; set; }

        public IIRVFlags Immunities { get; set; }

        public IIRVFlags Resistances { get; set; }

        public IIRVFlags Vulnerabilities { get; set; }

        public Dictionary<CharacterAttributes, int> Attributes { get; set; }
    }
}
