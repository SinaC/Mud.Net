using Mud.POC.Abilities2.ExistingCode;
using Mud.Server.Input;
using System.Collections.Generic;
using System.Linq;
using Mud.Common;

namespace Mud.POC.Abilities2.Helpers
{
    public static class FindHelpers
    {
        public static TEntity FindByName<TEntity>(IEnumerable<TEntity> entities, CommandParameter parameter)
            where TEntity : IEntity
             => entities.FirstOrDefault(x => StringCompareHelpers.StringStartsWith(x.Name, parameter.Value));

        public static IEnumerable<T> FindAllByName<T>(IEnumerable<T> list, string parameter, bool perfectMatch = false)
            where T : IEntity
        {
            return perfectMatch
                ? list.Where(x => StringCompareHelpers.StringListEquals(x.Keywords, parameter))
                : list.Where(x => StringCompareHelpers.StringListStartsWith(x.Keywords, parameter));
        }

        public static IItem FindItemHere(ICharacter character, CommandParameter parameter)
            => FindByName(
                character.Room.Content.Where(character.CanSee)
                    .Concat(character.Inventory.Where(character.CanSee))
                    .Concat(character.Equipments.Where(x => x.Item != null && character.CanSee(x.Item)).Select(x => x.Item)),
                parameter);

        public static ICharacter FindChararacterInWorld(ICharacter asker, CommandParameter parameter) // equivalent to get_char_world in handler.C:3511
        {
            // In room
            ICharacter inRoom = FindByName(asker.Room.People.Where(asker.CanSee), parameter);
            if (inRoom != null)
                return inRoom;

            // In area
            //  players
            IPlayableCharacter inAreaPlayer = FindByName(asker.Room.Area.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            if (inAreaPlayer != null)
                return inAreaPlayer;
            //  characters
            INonPlayableCharacter inAreaCharacter = FindByName(asker.Room.Area.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            if (inAreaCharacter != null)
                return inAreaCharacter;

            //// In world
            ////  players
            //IPlayableCharacter inWorldPlayer = FindByName(World.Characters.OfType<IPlayableCharacter>().Where(x => x.ImpersonatedBy != null && asker.CanSee(x)), parameter);
            //if (inWorldPlayer != null)
            //    return inWorldPlayer;
            ////  characters
            //INonPlayableCharacter inWorldCharacter = FindByName(World.Characters.OfType<INonPlayableCharacter>().Where(asker.CanSee), parameter);
            //return inWorldCharacter;
            return null;
        }
    }
}
