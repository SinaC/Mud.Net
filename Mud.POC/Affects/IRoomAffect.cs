namespace Mud.POC.Affects
{
    public interface IRoomAffect : IAffect
    {
        // RoomFlags
        void Apply(IRoom room);
    }

}
