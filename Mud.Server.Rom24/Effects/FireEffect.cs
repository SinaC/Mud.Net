using Microsoft.Extensions.Logging;
using Mud.Common;
using Mud.Domain;
using Mud.Server.Affects;
using Mud.Server.Effects;
using Mud.Server.Flags;
using Mud.Server.Interfaces.Aura;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Effect;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Room;
using Mud.Server.Random;
using System.Collections.ObjectModel;

namespace Mud.Server.Rom24.Effects;

[Effect("Fire")]
public class FireEffect : IEffect<IRoom>, IEffect<ICharacter>, IEffect<IItem>
{
    private ILogger Logger { get; }
    private IServiceProvider ServiceProvider { get; }
    private IRandomManager RandomManager { get; }
    private IAuraManager AuraManager { get; }
    private IItemManager ItemManager { get; }

    public FireEffect(ILogger logger, IServiceProvider serviceProvider, IRandomManager randomManager, IAuraManager auraManager, IItemManager itemManager)
    {
        Logger = logger;
        ServiceProvider = serviceProvider;
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
        // chance of blindness
        if (!victim.CharacterFlags.IsSet("Blind") && !victim.SavesSpell(level / 4 + modifier / 20, SchoolTypes.Fire))
        {
            victim.Send("Your eyes tear up from smoke...you can't see a thing!");
            victim.Act(ActOptions.ToRoom, "{0} is blinded by smoke!", victim);
            int duration = RandomManager.Range(1, level / 10);
            AuraManager.AddAura(victim, auraName, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false, // TODO:
                new CharacterAttributeAffect { Operator = AffectOperators.Add, Modifier = -4, Location = CharacterAttributeAffectLocations.HitRoll },
                new CharacterFlagsAffect { Operator = AffectOperators.Or, Modifier = new CharacterFlags(ServiceProvider, "Blind") });
        }
        // getting thirsty
        (victim as IPlayableCharacter)?.GainCondition(Conditions.Thirst, modifier / 20);
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
        // Affects only container, potion, scroll, staff, wand, food, pill
        if (!(item is IItemContainer || item is IItemFood || item is IItemPotion || item is IItemScroll || item is IItemStaff || item is IItemWand || item is IItemPill))
            return;
        int chance = level / 4 + modifier / 10;
        if (chance > 25)
            chance = (chance - 25) / 2 + 25;
        if (chance > 50)
            chance = (chance - 50) / 2 + 50;
        if (item.ItemFlags.IsSet("Bless"))
            chance -= 5;
        chance -= item.Level * 2;
        if (item is IItemPotion)
            chance += 25;
        else if (item is IItemScroll)
            chance += 50;
        else if (item is IItemStaff)
            chance += 10;
        chance = chance.Range(5, 95);
        if (RandomManager.Range(1, 100) > chance)
            return;
        // display msg
        string msg;
        switch (item)
        {
            case IItemContainer _:
                msg = "{0} ignites and burns!";
                break;
            case IItemPotion _:
                msg = "{0} bubbles and boils!";
                break;
            case IItemScroll _:
                msg = "{0} crackles and burns!";
                break;
            case IItemStaff _:
                msg = "{0} smokes and chars!";
                break;
            case IItemWand _:
                msg = "{0} sparks and sputters!";
                break;
            case IItemFood _:
                msg = "{0} blackens and crisps!";
                break;
            case IItemPill _:
                msg = "{0} melts and drips!";
                break;
            default:
                Logger.LogError("FireEffect: default message for unexpected item type {itemType}", item.GetType());
                msg = "{0} burns."; 
                break;
        }
        var viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
        viewer?.Act(ActOptions.ToAll, msg, item);
        // destroy container, dump the contents and apply fire effect on them
        if (item is IItemContainer itemContainer) // get rid of content and apply fire effect on it
        {
            IRoom? dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
            if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                dropItemTargetRoom = roomContainer;
            else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                dropItemTargetRoom = character.Room;
            else if (item.EquippedBy?.Room != null) // if container is equipped, unequip and drop content to room
            {
                item.ChangeEquippedBy(null, true);
                dropItemTargetRoom = item.EquippedBy.Room;
            }
            var clone = new ReadOnlyCollection<IItem>(itemContainer.Content.ToList());
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
