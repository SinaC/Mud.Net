using System.Collections.Generic;

namespace Mud.POC.Affects
{
    public interface IRoom : IEntity
    {
        IEnumerable<ICharacter> People { get; }
        IEnumerable<IItem> Content { get; }

        RoomFlags BaseRoomFlags { get; }
        RoomFlags CurrentRoomFlags { get; }

        void ApplyAffect(RoomFlagsAffect affect);

        void AddItem(IItem item);
        void AddCharacter(ICharacter character);
    }
}
