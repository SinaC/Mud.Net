using Mud.Domain;
using Mud.Server.Blueprints.Item;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using System.Text;

namespace Mud.Server.Item;

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
    public IEnumerable<ICharacter>? People
        => (ContainedInto as IRoom)?.People?.Where(x => x.Furniture == this);

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


    public StringBuilder AppendPosition(StringBuilder sb, string verb)
    {
        if (FurniturePlacePreposition == FurniturePlacePrepositions.At)
            sb.AppendFormat(" is {0} at {1}", verb, DisplayName);
        else if (FurniturePlacePreposition == FurniturePlacePrepositions.On)
            sb.AppendFormat(" is {0} on {1}", verb, DisplayName);
        else if (FurniturePlacePreposition == FurniturePlacePrepositions.In)
            sb.AppendFormat(" is {0} in {1}", verb, DisplayName);
        else
            sb.AppendFormat(" is {0} here.", verb);
        return sb;
    }

    #endregion
}
