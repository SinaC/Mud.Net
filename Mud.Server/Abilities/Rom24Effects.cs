using System;
using System.Linq;
using Mud.Domain;
using Mud.Logger;
using Mud.Server.Aura;
using Mud.Server.Common;
using Mud.Server.Item;

namespace Mud.Server.Abilities
{
    public partial class AbilityManager
    {
        // IAbility is need for AcidEffect because it add AC malus on item
        public void AcidEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "AcidEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability?.Name ?? "???", source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    AcidEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    AcidEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.ItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only corpse, container, armor, clothing, wand, staff and scroll
                if (!(item is IItemCorpse || item is IItemContainer || item is IItemArmor || item is IItemWand || item is IItemStaff || item is IItemScroll)) // TODO: clothing
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
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
                    // TODO: clothing "$p is corroded into scrap."
                    case IItemWand _:
                    case IItemStaff _:
                        msg = "{0} corrodes and breaks.";
                        break;
                    case IItemScroll _:
                        msg = "{0} is burned into waste.";
                        break;
                    default:
                        Wiznet.Wiznet($"AcidEffect: default message for unexpected item type {item.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                if (item is IItemArmor) // etch it
                {
                    IAura existingAura = item.GetAura(ability);
                    if (existingAura != null)
                        existingAura.AddOrUpdateAffect(
                            x => x.Location == CharacterAttributeAffectLocations.AllArmor,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add },
                            x => x.Modifier += 1);
                    else
                        World.AddAura(item, ability, source, level, Pulse.Infinite, AuraFlags.Permanent, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.AllArmor, Modifier = 1, Operator = AffectOperators.Add });
                    item.Recompute();
                    return;
                }
                // destroy container, dump the contents and apply fire effect on them
                if (item is IContainer container) // get rid of content and apply acid effect on it
                {
                    IRoom dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
                    if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                        dropItemTargetRoom = roomContainer;
                    else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                        dropItemTargetRoom = character.Room;
                    else if (item.EquippedBy?.Room != null) // if container is in equipment, unequip and drop content to room
                    {
                        item.ChangeEquippedBy(null, true);
                        dropItemTargetRoom = item.EquippedBy.Room;
                    }
                    foreach (IItem itemInContainer in container.Content)
                    {
                        if (dropItemTargetRoom != null) // drop and apply acid effect
                        {
                            itemInContainer.ChangeContainer(dropItemTargetRoom);
                            AcidEffect(itemInContainer, ability, source, level / 2, damage / 2);
                        }
                        else // if item is nowhere, destroy it
                            World.RemoveItem(itemInContainer);
                    }
                    World.RemoveItem(item); // destroy item
                    dropItemTargetRoom?.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Wiznet.Wiznet($"AcidEffect: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        public void ColdEffect(IEntity target, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ColdEffect: [{0}] [{1}] [{2}] [{3}]", target.DebugName, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ColdEffect(itemInRoom, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // whack a character
            {
                // chill touch effect
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Cold))
                {
                    victim.Send("A chill sinks deep into your bones.");
                    victim.Act(ActOptions.ToRoom, "{0:N} turns blue and shivers.", victim);
                    IAbility chillTouchAbility = this["Chill Touch"];
                    IAura chillTouchAura = victim.GetAura(chillTouchAbility);
                    if (chillTouchAura != null)
                    {
                        chillTouchAura.Update(level, TimeSpan.FromMinutes(6));
                        chillTouchAura.AddOrUpdateAffect(
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                    }
                    else
                        World.AddAura(victim, chillTouchAbility, source, level, TimeSpan.FromMinutes(6), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
                }
                // hunger! (warmth sucked out)
                (victim as IPlayableCharacter)?.GainCondition(Conditions.Hunger, damage / 20);
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    ColdEffect(itemOnVictim, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.ItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only potion and drink container
                if (!(item is IItemPotion || item is IItemDrinkContainer))
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                if (item is IItemPotion)
                    chance += 25;
                else if (item is IItemDrinkContainer)
                    chance += 5;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // display msg
                string msg = "{0} freezes and shatters!";
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                // unequip and destroy item
                IEntity itemContainedInto;
                if (item.EquippedBy != null) // if item equipped: unequip 
                {
                    item.ChangeEquippedBy(null, true);
                    itemContainedInto = item.EquippedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                itemContainedInto?.Recompute();
            }
            Wiznet.Wiznet($"ColdEffect: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        public void FireEffect(IEntity target, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "FireEffect: [{0}] [{1}] [{2}] [{3}]", target.DebugName, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    FireEffect(itemInRoom, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of blindness
                if (!victim.CharacterFlags.HasFlag(CharacterFlags.Blind) && !victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Fire))
                {
                    victim.Send("Your eyes tear up from smoke...you can't see a thing!");
                    victim.Act(ActOptions.ToRoom, "{0} is blinded by smoke!", victim);
                    IAbility fireBreath = this["Fire Breath"];
                    int duration = RandomManager.Range(1, level / 10);
                    World.AddAura(victim, fireBreath, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterAttributeAffect { Operator = AffectOperators.Add, Modifier = -4, Location = CharacterAttributeAffectLocations.HitRoll },
                        new CharacterFlagsAffect { Operator = AffectOperators.Or, Modifier = CharacterFlags.Blind });
                }
                // getting thirsty
                (victim as IPlayableCharacter)?.GainCondition(Conditions.Thirst, damage / 20);
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    FireEffect(itemOnVictim, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.ItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.ItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only container, potion, scroll, staff, wand, food, pill
                if (!(item is IItemContainer || item is IItemFood || item is IItemPotion || item is IItemScroll || item is IItemStaff || item is IItemWand || item is IItemPill))
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
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
                        Wiznet.Wiznet($"FireEffect: default message for unexpected item type {item.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? item.EquippedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                viewer?.Act(ActOptions.ToAll, msg, item);
                // destroy container, dump the contents and apply fire effect on them
                if (item is IItemContainer itemContainer) // get rid of content and apply fire effect on it
                {
                    IRoom dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
                    if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                        dropItemTargetRoom = roomContainer;
                    else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                        dropItemTargetRoom = character.Room;
                    else if (item.EquippedBy?.Room != null) // if container is equipped, unequip and drop content to room
                    {
                        item.ChangeEquippedBy(null, true);
                        dropItemTargetRoom = item.EquippedBy.Room;
                    }
                    foreach (IItem itemInContainer in itemContainer.Content)
                    {
                        if (dropItemTargetRoom != null) // drop and apply acid effect
                        {
                            itemInContainer.ChangeContainer(dropItemTargetRoom);
                            FireEffect(itemInContainer, source, level / 2, damage / 2);
                        }
                        else // if item is nowhere, destroy it
                            World.RemoveItem(itemInContainer);
                    }
                    World.RemoveItem(item); // destroy item
                    dropItemTargetRoom?.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Wiznet.Wiznet($"FireEffect: invalid target type {target.GetType()}", WiznetFlags.Bugs, AdminLevels.Implementor);
        }

        public void PoisonEffect(IEntity target, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "PoisonEffect: [{0}] [{1}] [{2}] [{3}]", target.DebugName, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    PoisonEffect(itemInRoom, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of poisoning
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Poison))
                {
                    victim.Send("You feel poison coursing through your veins.");
                    victim.Act(ActOptions.ToRoom, "{0} looks very ill.", victim);
                    int duration = level / 2;
                    IAbility poisonAbility = this["Poison"];
                    IAura poisonAura = victim.GetAura(poisonAbility);
                    if (poisonAura != null)
                    {
                        poisonAura.Update(level, TimeSpan.FromMinutes(duration));
                        poisonAura.AddOrUpdateAffect(
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                        poisonAura.AddOrUpdateAffect(
                            x => x.Modifier == CharacterFlags.Poison,
                            () => new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                            null);
                    }
                    else
                        World.AddAura(victim, poisonAbility, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                }
                // equipment
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    PoisonEffect(itemOnVictim, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItemPoisonable poisonable) // do some poisoning
            {
                if (poisonable.ItemFlags.HasFlag(ItemFlags.BurnProof)
                    || poisonable.ItemFlags.HasFlag(ItemFlags.Bless)
                    || RandomManager.Chance(25))
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (poisonable.ItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= poisonable.Level * 2;
                chance = chance.Range(5, 95);
                if (!RandomManager.Chance(chance))
                    return;
                poisonable.Poison();
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "PoisonEffect: invalid target type {0}", target.GetType());
        }

        public void ShockEffect(IEntity target, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ShockEffect: [{0}] [{1}] [{2}] [{3}]", target.DebugName, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ShockEffect(itemInRoom, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // daze and confused?
                if (!victim.SavesSpell(level / 4 + damage / 20, SchoolTypes.Lightning))
                {
                    victim.Send("Your muscles stop responding.");
                    // TODO: set Daze to Math.Max(12, level/4 + damage/20)
                }
                // toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    ShockEffect(itemOnVictim, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.ItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // unequip and destroy item
                IEntity itemContainedInto;
                if (item.EquippedBy != null) // if item is equipped: unequip 
                {
                    item.ChangeEquippedBy(null, true);
                    itemContainedInto = item.EquippedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                itemContainedInto?.Recompute();
            }
            Log.Default.WriteLine(LogLevels.Error, "ShockEffect: invalid target type {0}", target.GetType());
        }
    }
}
