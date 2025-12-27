using Mud.Domain;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;

namespace Mud.Server.Rom24.Effects;

[Effect("Shock")]
public class ShockEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
{
    private IRandomManager RandomManager { get; }
    private IItemManager ItemManager { get; }

    public ShockEffect(IRandomManager randomManager, IItemManager itemManager)
    {
        RandomManager = randomManager;
        ItemManager = itemManager;
    }

    public void Apply(IRoom room, IEntity source, string auraName, int level, int modifier)
    {
        if (!room.IsValid)
            return;
        var roomContent = room.Content.ToArray();
        foreach (var itemInRoom in roomContent)
            Apply(itemInRoom, source, auraName, level, modifier);
        room.Recompute();
    }

    public void Apply(ICharacter victim, IEntity source, string auraName, int level, int modifier)
    {
        if (!victim.IsValid)
            return;
        if (!victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Lightning))
        {
            victim.Send("Your muscles stop responding.");
            var daze = Math.Max(level + 4 + modifier / 20, 12);
            victim.SetDaze(daze);
        }
        // toast some gear
        var inventoryAndEquipments = victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!)).ToArray();
        foreach (var itemOnVictim in inventoryAndEquipments)
            Apply(itemOnVictim, source, auraName, level, modifier);
        victim.Recompute();
    }

    public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
    {
        if (!item.IsValid)
            return;
        int chance = level / 4 + modifier / 10;
        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;
        if (item.ItemFlags.IsSet("Bless"))
            chance -= 5;
        chance -= item.Level * 2;
        chance = Math.Clamp(chance, 5, 95);
        if (RandomManager.Range(1, 100) > chance)
            return;
        // unequip and destroy item
        IEntity? itemContainedInto;
        if (item.EquippedBy != null) // if item is equipped: unequip 
        {
            item.ChangeEquippedBy(null, true);
            itemContainedInto = item.EquippedBy;
        }
        else
            itemContainedInto = item.ContainedInto;
        //
        ItemManager.RemoveItem(item); // destroy item
        itemContainedInto?.Recompute();
    }
}
