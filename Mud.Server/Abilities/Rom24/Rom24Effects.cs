using Mud.Domain;
using Mud.Logger;
using Mud.Server.Common;
using Mud.Server.Item;
using System;
using System.Linq;
using Mud.Server.Aura;

namespace Mud.Server.Abilities.Rom24
{
    public class Rom24Effects
    {
        private IWorld World { get; }
        private IAbilityManager AbilityManager { get; }
        private IRandomManager RandomManager { get; }

        private Rom24Common Rom24Common { get; }

        public Rom24Effects(IWorld world, IAbilityManager abilityManager, IRandomManager randomManager)
        {
            World = world;
            AbilityManager = abilityManager;
            RandomManager = randomManager;

            Rom24Common = new Rom24Common(randomManager);
        }

        public void AcidEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "AcidEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
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
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only corpse, container, armor, clothing, wand, staff and scroll
                if (!(item is IItemCorpse || item is IItemContainer || item is IItemArmor)) // TODO: wand, staff, scroll, clothing
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                // TODO: if staff/wand -> chance-=10
                // TODO: if scroll -> chance+=10
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
                    // TODO: staff, wand  "$p corrodes and breaks."
                    // TODO: scroll  "$p is burned into waste."
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "AcidEffect: default message for unexpected item type {0}", item.GetType());
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipable)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                if (viewer != null)
                    viewer.Act(ActOptions.ToAll, msg, item);
                if (item is IItemArmor) // etch it
                {
                    IAura existingAura = item.GetAura(ability);
                    if (existingAura != null)
                        existingAura.AddOrUpdate<CharacterAttributeAffect>(
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
                    else if (item is IEquipable equipable && equipable.EquipedBy.Room != null) // if container is in equipment, unequip and drop content to room
                    {
                        equipable.ChangeEquipedBy(null);
                        dropItemTargetRoom = equipable.EquipedBy.Room;
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
                    if (dropItemTargetRoom != null)
                        dropItemTargetRoom.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "AcidEffect: unknown EntityType {0}", target.GetType());
            return;
        }

        public void ColdEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ColdEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ColdEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // whack a character
            {
                // chill touch effect
                if (!Rom24Common.SavesSpell(level / 4 + damage / 20, victim, SchoolTypes.Cold))
                {
                    victim.Send("A chill sinks deep into your bones.");
                    victim.Act(ActOptions.ToRoom, "{0:N} turns blue and shivers.", victim);
                    IAbility chillTouchAbility = AbilityManager["Chill Touch"];
                    IAura chillTouchAura = victim.GetAura(chillTouchAbility);
                    if (chillTouchAura != null)
                        chillTouchAura.AddOrUpdate<CharacterAttributeAffect>( // TODO: update duration
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                    else
                        World.AddAura(victim, chillTouchAbility, source, level, TimeSpan.FromMinutes(6), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add });
                }
                // hunger! (warmth sucked out)
                // TODO: gain_condition(victim,COND_HUNGER,dam/20); if NPC
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    ColdEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only potion and drink container
                if (true) //if (!(item is IItemPotion || item is IDrinkContainer)) // TODO: potion and drink container
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                //if (item is IItemPotion)
                //    chance += 25;
                //if (item is IDrinkContainer)
                //    chance += 5;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // display msg
                string msg = "{0} freezes and shatters!";
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipable)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                if (viewer != null)
                    viewer.Act(ActOptions.ToAll, msg, item);
                // unequip and destroy item
                IEntity itemContainedInto = null;
                if (item is IEquipable equipable && equipable.EquipedBy != null) // if item equiped: unequip 
                {
                    equipable.ChangeEquipedBy(null);
                    itemContainedInto = equipable.EquipedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                if (itemContainedInto != null)
                    itemContainedInto.Recompute();
            }
            Log.Default.WriteLine(LogLevels.Error, "ColdEffect: unknown EntityType {0}", target.GetType());
            return;
        }

        public void FireEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "FireEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    FireEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of blindness
                if (!victim.CurrentCharacterFlags.HasFlag(CharacterFlags.Blind) && !Rom24Common.SavesSpell(level / 4 + damage / 20, victim, SchoolTypes.Fire))
                {
                    victim.Send("Your eyes tear up from smoke...you can't see a thing!");
                    victim.Act(ActOptions.ToRoom, "{0} is blinded by smoke!", victim);
                    IAbility fireBreath = AbilityManager["Fire Breath"];
                    int duration = RandomManager.Range(1, level / 10);
                    World.AddAura(victim, fireBreath, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                        new CharacterAttributeAffect {Operator = AffectOperators.Add, Modifier = -4, Location = CharacterAttributeAffectLocations.HitRoll},
                        new CharacterFlagsAffect {Operator = AffectOperators.Or, Modifier = CharacterFlags.Blind});
                }
                // getting thirsty
                // TODO: gain_condition(victim,COND_THIRST,dam/20); if NPC
                // let's toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    FireEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // toast an object
            {
                if (item.CurrentItemFlags.HasFlag(ItemFlags.BurnProof)
                    || item.CurrentItemFlags.HasFlag(ItemFlags.NoPurge)
                    || RandomManager.Range(0, 4) == 0)
                    return;
                // Affects only container, potion, scroll, staff, wand, food, pill
                if (!(item is IItemContainer)) // TODO: potion, scroll, staff, wand, food, pill
                    return;
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                //TODO: IItemPotion chance += 25
                //TODO: IItemScroll chance += 50
                //TODO: IItemStaff chance += 10
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
                    //TODO case IItemPotion _: msg = "{0} bubbles and boils!"
                    //TODO case IItemScroll _: msg = "{0} crackles and burns!"
                    //TODO case IItemStaff _: msg = "{0} smokes and chars!"
                    //TODO case IItemWand _: msg = "{0} sparks and sputters!"
                    //TODO case IItemFood _: msg = "{0} blackens and crisps!"
                    //TODO case IItemPill _: msg = "{0} melts and drips!"
                    default:
                        Log.Default.WriteLine(LogLevels.Error, "FireEffect: default message for unexpected item type {0}", item.GetType());
                        msg = "{0} burns.";
                        break;
                }
                ICharacter viewer = (item.ContainedInto as ICharacter) ?? (item as IEquipable)?.EquipedBy ?? (item.ContainedInto as IRoom)?.People.FirstOrDefault(); // viewer is holder or any person in the room
                if (viewer != null)
                    viewer.Act(ActOptions.ToAll, msg, item);
                // destroy container, dump the contents and apply fire effect on them
                if (item is IContainer container) // get rid of content and apply fire effect on it
                {
                    IRoom dropItemTargetRoom = null; // this will store the room where destroyed container's content has to go
                    if (item.ContainedInto is IRoom roomContainer) // if container is in a room, drop content to room
                        dropItemTargetRoom = roomContainer;
                    else if (item.ContainedInto is ICharacter character && character.Room != null) // if container is in an inventory, drop content to room
                        dropItemTargetRoom = character.Room;
                    else if (item is IEquipable equipable && equipable.EquipedBy.Room != null) // if container is equiped, unequip and drop content to room
                    {
                        equipable.ChangeEquipedBy(null);
                        dropItemTargetRoom = equipable.EquipedBy.Room;
                    }
                    foreach (IItem itemInContainer in container.Content)
                    {
                        if (dropItemTargetRoom != null) // drop and apply acid effect
                        {
                            itemInContainer.ChangeContainer(dropItemTargetRoom);
                            FireEffect(itemInContainer, ability, source, level / 2, damage / 2);
                        }
                        else // if item is nowhere, destroy it
                            World.RemoveItem(itemInContainer);
                    }
                    World.RemoveItem(item); // destroy item
                    if (dropItemTargetRoom != null)
                        dropItemTargetRoom.Recompute();
                    return;
                }
                //
                World.RemoveItem(item); // destroy item
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "FireEffect: unknown EntityType {0}", target.GetType());
            return;
        }

        public void PoisonEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "PoisonEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    PoisonEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // chance of poisoning
                if (!Rom24Common.SavesSpell(level / 4 + damage / 20, victim, SchoolTypes.Poison))
                {
                    victim.Send("You feel poison coursing through your veins.");
                    victim.Act(ActOptions.ToRoom, "{0} looks very ill.", victim);
                    int duration = level / 2;
                    IAbility poisonAbility = AbilityManager["Poison"];
                    IAura poisonAura = victim.GetAura(poisonAbility);
                    if (poisonAura != null) // TODO: update duration
                    {
                        poisonAura.AddOrUpdate<CharacterAttributeAffect>(
                            x => x.Location == CharacterAttributeAffectLocations.Strength,
                            () => new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            x => x.Modifier -= 1);
                        poisonAura.AddOrUpdate<CharacterFlagsAffect>(
                            x => x.Modifier == CharacterFlags.Poison,
                            () => new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or },
                            null);
                    }
                    else
                        World.AddAura(victim, ability, source, level, TimeSpan.FromMinutes(duration), AuraFlags.None, false,
                            new CharacterAttributeAffect { Location = CharacterAttributeAffectLocations.Strength, Modifier = -1, Operator = AffectOperators.Add },
                            new CharacterFlagsAffect { Modifier = CharacterFlags.Poison, Operator = AffectOperators.Or });
                }
                // equipment
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    PoisonEffect(itemOnVictim, ability, source, level, damage);
                victim.Recompute();
                return;
            }
            if (target is IItem item) // do some poisoning
            {
                int chance = level / 4 + damage / 10;
                if (chance > 25)
                    chance = (chance - 25) / 2 + 25;
                if (chance > 50)
                    chance = (chance - 50) / 2 + 50;
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // TODO: Food/DrinkContainer not implemented -> poison food or drink container
                return;
            }
            Log.Default.WriteLine(LogLevels.Error, "PoisonEffect: unknown EntityType {0}", target.GetType());
            return;
        }

        public void ShockEffect(IEntity target, IAbility ability, ICharacter source, int level, int damage) // recursive function
        {
            Log.Default.WriteLine(LogLevels.Debug, "ShockEffect: [{0}] [{1}] [{2}] [{3}] [{4}]", target.DebugName, ability.Name, source.DebugName, level, damage);
            if (!target.IsValid)
                return;
            if (target is IRoom room) // nail objects on the floor
            {
                foreach (IItem itemInRoom in room.Content)
                    ShockEffect(itemInRoom, ability, source, level, damage);
                room.Recompute();
                return;
            }
            if (target is ICharacter victim) // do the effect on a victim
            {
                // daze and confused?
                if (!Rom24Common.SavesSpell(level / 4 + damage / 20, victim, SchoolTypes.Lightning))
                {
                    victim.Send("Your muscles stop responding.");
                    // TODO: set Daze to Math.Max(12, level/4 + damage/20)
                }
                // toast some gear
                foreach (IItem itemOnVictim in victim.Inventory.Union(victim.Equipments.Where(x => x.Item != null).Select(x => x.Item)))
                    PoisonEffect(itemOnVictim, ability, source, level, damage);
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
                if (item.CurrentItemFlags.HasFlag(ItemFlags.Bless))
                    chance -= 5;
                chance -= item.Level * 2;
                chance = chance.Range(5, 95);
                if (RandomManager.Range(1, 100) > chance)
                    return;
                // unequip and destroy item
                IEntity itemContainedInto = null;
                if (item is IEquipable equipable && equipable.EquipedBy != null) // if item is equiped: unequip 
                {
                    equipable.ChangeEquipedBy(null);
                    itemContainedInto = equipable.EquipedBy;
                }
                else
                    itemContainedInto = item.ContainedInto;
                //
                World.RemoveItem(item); // destroy item
                if (itemContainedInto != null)
                    itemContainedInto.Recompute();
            }
            Log.Default.WriteLine(LogLevels.Error, "ShockEffect: unknown EntityType {0}", target.GetType());
            return;
        }
    }
}
