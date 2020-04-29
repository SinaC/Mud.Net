using System.Text;

namespace Mud.POC.Affects
{
    // Rooms are affected by
    //  room itself
    //  people in room
    //  items in room

    // Items are affected by
    //  room containing item
    //  holder
    //  item itself

    // Characters are affected by
    //  room containing character
    //  equipments
    //  character itself

    public interface IAffect
    {
        void Append(StringBuilder sb);
    }

    public enum AffectOperators
    {
        Add     = 0,
        Or      = 1,
        Assign  = 2,
        Nor     = 3
    }
}