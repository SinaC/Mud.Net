using System.Collections.Generic;
using System.Linq;

namespace Mud.Server.Area
{
    public class Area : IArea
    {
        private readonly List<IRoom> _rooms;

        public Area(string displayName, int minLevel, int maxLevel, string builders, string credits)
        {
            DisplayName = displayName;
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            Builders = builders;
            Credits = credits;

            _rooms = new List<IRoom>();
        }

        #region IArea

        public string DisplayName { get; }
        public int MinLevel { get; }
        public int MaxLevel { get; }
        public string Builders { get; }
        public string Credits { get; }
        public IEnumerable<IRoom> Rooms => _rooms;
        public IEnumerable<IPlayer> Players => _rooms.SelectMany(x => x.People).Where(x => x.ImpersonatedBy != null).Select(x => x.ImpersonatedBy);

        public bool AddRoom(IRoom room)
        {
            //if (room.Area != null)
            //{
            //    Log.Default.WriteLine(LogLevels.Error, $"Area.AddRoom: Room {room.DebugName}");
            //    return false;
            //}
            // TODO: some checks ?
            _rooms.Add(room);
            return true;
        }

        public bool RemoveRoom(IRoom room)
        {
            // TODO: some checks ?
            _rooms.Remove(room);
            return true;
        }

        #endregion
    }
}
