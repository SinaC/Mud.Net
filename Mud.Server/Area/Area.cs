using System;
using Mud.Container;
using System.Collections.Generic;
using System.Linq;
using Mud.Server.Blueprints.Area;

namespace Mud.Server.Area
{
    public class Area : IArea
    {
        private readonly List<IRoom> _rooms;

        private IWiznet Wiznet => DependencyContainer.Current.GetInstance<IWiznet>();

        public Area(Guid id, AreaBlueprint blueprint)
        {
            Id = id;

            Blueprint = blueprint;
            DisplayName = blueprint.Name;
            Builders = blueprint.Builders;
            Credits = blueprint.Credits;

            _rooms = new List<IRoom>();
        }

        #region IArea

        public Guid Id { get; }

        public AreaBlueprint Blueprint { get; }

        public string DisplayName { get; }
        public string Builders { get; }
        public string Credits { get; }
        public IEnumerable<IRoom> Rooms => _rooms;
        public IEnumerable<IPlayer> Players => _rooms.SelectMany(x => x.People).OfType<IPlayableCharacter>().Select(x => x.ImpersonatedBy);
        public IEnumerable<ICharacter> Characters => _rooms.SelectMany(x => x.People);
        public IEnumerable<IPlayableCharacter> PlayableCharacters => _rooms.SelectMany(x => x.People).OfType<IPlayableCharacter>();

        public void ResetArea()
        {
            foreach (IRoom room in _rooms)
                room.ResetRoom();
            Wiznet.Wiznet($"{DisplayName} has just been reset.", Domain.WiznetFlags.Resets);
        }

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
