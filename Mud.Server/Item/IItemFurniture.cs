using System.Collections.Generic;
using Mud.Server.Constants;

namespace Mud.Server.Item
{
    public interface IItemFurniture : IItem // TODO: count people actually on furniture
    {
        IEnumerable<ICharacter> People { get; }

        int MaxPeople { get; }
        int MaxWeight { get; }
        FurnitureActions FurnitureActions { get; }
        FurniturePlacePrepositions FurniturePlacePreposition { get; }
        int HealBonus { get; }
        int ResourceBonus { get; }
    }
}
