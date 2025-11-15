using Mud.Common;
using Mud.Server.Interfaces.Admin;
using Mud.Server.Interfaces.Character;
using Mud.Server.Interfaces.Entity;
using Mud.Server.Interfaces.GameAction;
using Mud.Server.Interfaces.Item;
using Mud.Server.Interfaces.Player;
using Mud.Server.Interfaces.Room;

namespace Mud.Server.Common;

public static class FindHelpers
{
    //// Search in room content, then in inventory, then in equipment
    //public static IItem FindItemHere(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
    //{
    //    return FindByName(character.Room.Content.Where(character.CanSee), parameter, perfectMatch)
    //           ?? FindByName(character.Content.Where(character.CanSee), parameter, perfectMatch)
    //           ?? (FindByName(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)), x => x.Item, parameter, perfectMatch) ?? EquippedItem.NullObject).Item;
    //}

    //// Concat room content, inventory and equipment, then search
    //public static IItem FindCharacterItemByNameInRoomAndInventoryAndEquipment(ICharacter character, CommandParameter parameter, bool perfectMatch = false) // should replace FindItemHere
    //{
    //    return FindByName(
    //        character.Room.Content.Where(character.CanSee)
    //            .Concat(character.Content.Where(character.CanSee))
    //            .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
    //        parameter, perfectMatch);
    //}

    // Concat room content, inventory and equipment, then search
    public static IItem? FindItemHere(ICharacter character, ICommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
    {
        return FindByName(
            character.Room.Content.Where(character.CanSee)
                .Concat(character.Inventory.Where(character.CanSee))
                .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
            parameter, perfectMatch);
    }
    public static T? FindItemHere<T>(ICharacter character, ICommandParameter parameter, bool perfectMatch = false) // equivalent to get_obj_here in handler.C:3680
        where T:IItem
    {
        return FindByName(
            character.Room.Content.Where(character.CanSee).OfType<T>()
                .Concat(character.Inventory.Where(character.CanSee).OfType<T>())
                .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item).OfType<T>()),
            parameter, perfectMatch);
    }

    // Players/Admin
    public static IPlayer? FindByName(IEnumerable<IPlayer> list, string name, bool perfectMatch = false)
    {
        return perfectMatch
            ? list.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name))
            : list.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
    }

    public static IPlayer? FindByName(IEnumerable<IPlayer> list, ICommandParameter parameter, bool perfectMatch = false)
    {
        return perfectMatch
            ? list.Where(x => StringCompareHelpers.StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
            : list.Where(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
    }

    public static IAdmin? FindByName(IEnumerable<IAdmin> list, string name, bool perfectMatch = false)
    {
        return perfectMatch
            ? list.FirstOrDefault(x => StringCompareHelpers.StringEquals(x.Name, name))
            : list.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, name));
    }

    public static IAdmin? FindByName(IEnumerable<IAdmin> list, ICommandParameter parameter, bool perfectMatch = false)
    {
        return perfectMatch
            ? list.Where(x => StringCompareHelpers.StringEquals(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1)
            : list.Where(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value)).ElementAtOrDefault(parameter.Count - 1);
    }

    // Entity
    public static T? FindByName<T>(IEnumerable<T> list, ICommandParameter parameter, bool perfectMatch = false)
        where T : IEntity?
    {
        return perfectMatch
            ? list.Where(x => StringCompareHelpers.StringListsEquals(x!.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
            : list.Where(x => StringCompareHelpers.StringListsStartsWith(x!.Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
    }

    public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, ICommandParameter parameter, bool perfectMatch = false)
        where T : IEntity
    {
        return perfectMatch
            ? list.Where(x => StringCompareHelpers.StringListsEquals(x.Keywords, parameter.Tokens))
            : list.Where(x => StringCompareHelpers.StringListsStartsWith(x.Keywords, parameter.Tokens));
    }

    public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, string parameter, bool perfectMatch = false)
        where T : IEntity
    {
        return perfectMatch
            ? list.Where(x => StringCompareHelpers.StringListEquals(x.Keywords, parameter))
            : list.Where(x => StringCompareHelpers.StringListStartsWith(x.Keywords, parameter));
    }

    //public static IEnumerable<IPlayer> FindAllByName(IEnumerable<IPlayer> list, CommandParameter parameter, bool perfectMatch = false)
    //{
    //    return perfectMatch
    //        ? list.Where(x => StringEquals(x.Name, parameter.Value))
    //        : list.Where(x => StringStartsWith(x.Name, parameter.Value));
    //}

    public static T? FindByName<T, TEntity>(IEnumerable<T> collection, Func<T, TEntity> getItemFunc, ICommandParameter parameter, bool perfectMatch = false)
        where T: notnull
        where TEntity : IEntity
    {
        return perfectMatch
            ? collection.Where(x => StringCompareHelpers.StringListsEquals(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1)
            : collection.Where(x => StringCompareHelpers.StringListsStartsWith(getItemFunc(x).Keywords, parameter.Tokens)).ElementAtOrDefault(parameter.Count - 1);
    }

    // FindLocation
    public static IRoom? FindLocation(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, ICommandParameter parameter)
    {
        if (parameter.IsNumber)
        {
            int id = parameter.AsNumber;
            return roomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
        }

        var victim = FindByName(characterManager.Characters, parameter);
        if (victim != null)
            return victim.Room;

        var item = FindByName(itemManager.Items, parameter);
        return item?.ContainedInto as IRoom;
    }

    public static IRoom? FindLocation(IRoomManager roomManager, ICharacterManager characterManager, IItemManager itemManager, ICharacter asker, ICommandParameter parameter)
    {
        if (parameter.IsNumber)
        {
            int id = parameter.AsNumber;
            return roomManager.Rooms.FirstOrDefault(x => x.Blueprint.Id == id);
        }

        var victim = FindChararacterInWorld(characterManager, asker, parameter);
        if (victim != null)
            return victim.Room;

        var item = FindItemInWorld(itemManager, asker, parameter);
        return item?.ContainedInto as IRoom;
    }

    // FindCharacter
    public static ICharacter? FindChararacterInWorld(ICharacterManager characterManager, ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
    {
        // In room
        var inRoom = FindByName(asker.Room.People.Where(asker.CanSee), parameter);
        if (inRoom != null)
            return inRoom;

        // In area
        //  players
        var inAreaPlayer = FindByName(asker.Room.Area.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
        if (inAreaPlayer != null)
            return inAreaPlayer;
        //  characters
        var inAreaCharacter = FindByName(asker.Room.Area.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
        if (inAreaCharacter != null)
            return inAreaCharacter;

        // In world
        //  players
        var inWorldPlayer = FindByName(characterManager.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
        if (inWorldPlayer != null)
            return inWorldPlayer;
        //  characters
        var inWorldCharacter = FindByName(characterManager.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
        return inWorldCharacter;
    }

    public static INonPlayableCharacter? FindNonPlayableChararacterInWorld(ICharacterManager characterManager, ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
    {
        // In room
        var inRoom = FindByName(asker.Room.NonPlayableCharacters.Where(asker.CanSee), parameter);
        if (inRoom != null)
            return inRoom;

        // In area
        var inAreaCharacter = FindByName(asker.Room.Area.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
        if (inAreaCharacter != null)
            return inAreaCharacter;

        // In world
        var inWorldCharacter = FindByName(characterManager.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
        return inWorldCharacter;
    }

    public static IPlayableCharacter? FindPlayableChararacterInWorld(ICharacterManager characterManager, ICharacter asker, ICommandParameter parameter) // equivalent to get_char_world in handler.C:3511
    {
        // In room
        var inRoom = FindByName(asker.Room.PlayableCharacters.Where(asker.CanSee), parameter);
        if (inRoom != null)
            return inRoom;

        // In area
        var inAreaPlayer = FindByName(asker.Room.Area.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
        if (inAreaPlayer != null)
            return inAreaPlayer;

        // In world
        var inWorldPlayer = FindByName(characterManager.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
        return inWorldPlayer;
    }

    // FindItem
    public static IItem? FindItemInWorld(IItemManager itemManager, ICharacter asker, ICommandParameter parameter) // equivalent to get_obj_world in handler.C:3702
    {
        var hereItem = FindItemHere(asker, parameter);
        if (hereItem != null)
            return hereItem;

        var inWorldItem = FindByName(itemManager.Items.Where(asker.CanSee), parameter);
        return inWorldItem;
    }
}
