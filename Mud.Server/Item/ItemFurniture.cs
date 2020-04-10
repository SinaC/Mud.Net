using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Blueprints.Item;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public class ItemFurniture : ItemBase<ItemFurnitureBlueprint>, IItemFurniture
    {
        public ItemFurniture(Guid guid, ItemFurnitureBlueprint blueprint, IContainer containedInto) : base(guid, blueprint, containedInto)
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

        public bool CanStand => (FurnitureActions & FurnitureActions.Stand) == FurnitureActions.Stand;
        public bool CanSit => (FurnitureActions & FurnitureActions.Sit) == FurnitureActions.Sit;
        public bool CanRest => (FurnitureActions & FurnitureActions.Rest) == FurnitureActions.Rest;
        public bool CanSleep => (FurnitureActions & FurnitureActions.Sleep) == FurnitureActions.Sleep;

        #endregion
    }
}
