using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mud.Server
{
    public interface IAffect
    {
        void Append(StringBuilder sb);

        // TODO: join/merge/update method, allow to 'merge' 2 'identical' affects
    }

    public interface IRoomAffect : IAffect
    {
        // RoomFlags
        void Apply(IRoom room);
    }

    public interface IItemAffect<T> : IAffect
        where T : IItem
    {
        // ItemFlags
        // Armor: enchant affecting armor will be create as ICharacterAffect.Armor affect
        // Weapon: Flags
        void Apply(T item);
    }

    public interface ICharacterAffect : IAffect
    {
        // Attributes

        void Apply(ICharacter character);
    }
}
