using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Item;

namespace Mud.Server.Item
{
    public class ItemFurniture : ItemBase<ItemFurnitureBlueprint, ItemData>, IItemFurniture
    {
        public ItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer containedInto)
            : base(guid, blueprint, containedInto)
        {
            MaxPeople = blueprint.MaxPeople;
            MaxWeight = blueprint.MaxWeight;
            FurnitureActions = blueprint.FurnitureActions;
            FurniturePlacePreposition = blueprint.FurniturePlacePreposition;
            HealBonus = blueprint.HealBonus;
            ResourceBonus = blueprint.ResourceBonus;
        }

        public ItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, ItemData itemData, IContainer containedInto)
            : base(guid, blueprint, itemData, containedInto)
        {
            MaxPeople = blueprint.MaxPeople;
            MaxWeight = blueprint.MaxWeight;
            FurnitureActions = blueprint.FurnitureActions;
            FurniturePlacePreposition = blueprint.FurniturePlacePreposition;
            HealBonus = blueprint.HealBonus;
            ResourceBonus = blueprint.ResourceBonus;
        }

        #region IItemFurniture

        // Count number of people in room using 'this' as furniture
        public IEnumerable<ICharacter> People => (ContainedInto as IRoom)?.People?.Where(x => x.Furniture == this);

        public int MaxPeople { get; }
        public int MaxWeight { get; }
        public FurnitureActions FurnitureActions { get; }
        public FurniturePlacePrepositions FurniturePlacePreposition { get; }
        public int HealBonus { get; }
        public int ResourceBonus { get; }

        public bool CanStand => FurnitureActions.HasFlag(FurnitureActions.Stand);
        public bool CanSit => FurnitureActions.HasFlag(FurnitureActions.Sit);
        public bool CanRest => FurnitureActions.HasFlag(FurnitureActions.Rest);
        public bool CanSleep => FurnitureActions.HasFlag(FurnitureActions.Sleep);

        #endregion
    }
}
