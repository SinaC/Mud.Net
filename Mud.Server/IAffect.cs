using Mud.Domain;
using System.Text;

namespace Mud.Server
{
    public interface IAffect
    {
        void Append(StringBuilder sb);

        AffectDataBase MapAffectData();
    }

    public interface IRoomAffect : IAffect
    {
        // RoomFlags
        void Apply(IRoom room);
    }

    public interface IItemAffect<in T> : IAffect
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
