using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Affects.Character;
using Mud.Server.Common;
using Mud.Server.Effects;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.Effects;

[Effect("Acid")]
public class AcidEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
{
    private ILogger<AcidEffect> Logger { get; }
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public AcidEffect(ILogger<AcidEffect> logger, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
    {
        Logger = logger;
        RandomManager = randomManager;
        AuraManager = auraManager;
        ItemManager = itemManager;
    }

    public void Apply(IRoom room, IEntity source, string auraName, int level, int modifier)
    {
        if (!room.IsValid)
            return;
        var clone = new ReadOnlyCollection<IItem>(room.Content.ToList());
        foreach (var itemInRoom in clone)
            Apply(itemInRoom, source, auraName, level, modifier);
        room.Recompute();
    }

    public void Apply(ICharacter victim, IEntity source, string auraName, int level, int modifier)
    {
        if (!victim.IsValid)
            return;
        // let's toast some gear
        var clone = new ReadOnlyCollection<IItem>(victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item!)).ToList());
        foreach (var itemOnVictim in clone)
            Apply(itemOnVictim, source, auraName, level, modifier);
        victim.Recompute();
    }

    public void Apply(IItem item, IEntity source, string auraName, int level, int modifier)
    {
        if (!item.IsValid)
            return;
        if (item.ItemFlags.HasAny("BurnProof", "NoPurge")
                || RandomManager.Range(0, 4) == 0)
            return;
        // Affects only corpse, container, armor, clothing, wand, staff and scroll
        if (!(item is IItemCorpse || item is IItemContainer || item is IItemArmor || item is IItemWand || item is IItemStaff || item is IItemScroll || item is IItemClothing))
            return;
        int chance = level / 4 + modifier / 10;
        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;
        if (item.ItemFlags.IsSet("Bless"))
            chance -= 5;
        chance -= item.Level * 2;
        if (item is IItemStaff || item is IItemWand)
            chance -= 10;
        else if (item is IItemScroll)
            chance += 10;
        chance = chance.Range(5, 95);
        if (RandomManager.Range(1, 100) > chance)
            return;
        string msg;
        switch (item)
        {
            case IItemCorpse _:
            case IItemContainer _:
                msg = "{0} fumes and dissolves.";
                break;
            case IItemArmor _:
                msg = "{0} is pitted and etched.";
                break;
            case IItemClothing _:
                msg = "{0} is corroded into scrap.";
                break;
            case IItemWand _:
            case IItemStaff _:
                msg = "{0} corrodes and breaks.";
                break;
            case IItemScroll _:
                msg = "{0} is burned into waste.";
                break;
            default:
                Logger.LogError("AcidEffect: default message for unexpected item type {itemType}", item.GetType());
                msg = "{0} burns.";
                break;
        }
        var viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
        viewer?.Act(ActOptions.ToAll, msg, item);
        if (item is IItemArmor) // etch it
        {
            var existingAura = item.GetAura(auraName);
            if (existingAura != null)
                existingAura.AddOrUpdateAffect(
                    x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                    () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add },
                    x => x.Modifier += 1);
            else
                AuraManager.AddAura(item, auraName, source, level, Pulse.Infinite, AuraFlags.Permanent, false,
                    new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add });
            item.Recompute();
            return;
        }
        // destroy container, dump the contents and apply fire effect on them
        if (item is IContainer container) // get rid of content and apply acid effect on it
        {
            IRoom? dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
            if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                dropItemTargetRoom = roomContainer;
            else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                dropItemTargetRoom = character.Room;
            else if (item.EquippedBy?.Room != null) // if container is in equipment, unequip and drop content to room
            {
                item.ChangeEquippedBy(null, true);
                dropItemTargetRoom = item.EquippedBy.Room;
            }
            var clone = new ReadOnlyCollection<IItem>(container.Content.ToList());
            foreach (var itemInContainer in clone)
            {
                if (dropItemTargetRoom != null) // drop and apply acid effect
                {
                    itemInContainer.ChangeContainer(dropItemTargetRoom);
                    Apply(itemInContainer, source, auraName, level / 2, modifier / 2);
                }
                else // if item is nowhere, destroy it
                    ItemManager.RemoveItem(itemInContainer);
            }
            ItemManager.RemoveItem(item); // destroy item
            dropItemTargetRoom?.Recompute();
            return;
        }
        //
        ItemManager.RemoveItem(item); // destroy item
    }
}
