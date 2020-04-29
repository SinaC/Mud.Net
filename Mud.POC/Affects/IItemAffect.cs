namespace Mud.POC.Affects
{
    public interface IItemAffect<T> : IAffect
        where T:IItem
    {
        // ItemFlags
        // Armor: enchant affecting armor will be create as ICharacterAffect.Armor affect
        // Weapon: Flags
        void Apply(T item);
    }
}
