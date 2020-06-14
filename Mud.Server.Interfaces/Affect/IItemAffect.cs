using Mud.Server.Interfaces.Item;

namespace Mud.Server.Interfaces.Affect
{
    public interface IItemAffect<in T> : IAffect
            where T : IItem
    {
        // ItemFlags
        // Armor: enchant affecting armor will be create as ICharacterAffect.Armor affect
        // Weapon: Flags
        void Apply(T item);
    }
}
