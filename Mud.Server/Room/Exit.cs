using System;
using System.Collections.Generic;
using System.Linq;
using Mud.Domain;
using Mud.Server.Blueprints.Room;

namespace Mud.Server.Room
{
    public class Exit : IExit
    {
        public Exit(ExitBlueprint blueprint, IRoom destination)
        {
            Name = blueprint.Keyword;
            Keywords = Name?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Enumerable.Empty<string>();
            Description = blueprint.Description;
            Destination = destination;
            Blueprint = blueprint;
            ExitFlags = Blueprint.Flags;
        }

        #region IExit


        #region ICloseable

        public int KeyId => Blueprint.Key;
        public bool IsCloseable => ExitFlags.HasFlag(ExitFlags.Door);
        public bool IsLockable => KeyId >= 0;
        public bool IsClosed => ExitFlags.HasFlag(ExitFlags.Closed);
        public bool IsLocked => ExitFlags.HasFlag(ExitFlags.Locked);
        public bool IsPickProof => ExitFlags.HasFlag(ExitFlags.PickProof);
        public bool IsEasy => ExitFlags.HasFlag(ExitFlags.Easy);
        public bool IsHard => ExitFlags.HasFlag(ExitFlags.Hard);

        public void Open()
        {
            RemoveFlags(ExitFlags.Closed);
        }

        public void Close()
        {
            AddFlags(ExitFlags.Closed);
        }

        public void Unlock()
        {
            RemoveFlags(ExitFlags.Locked);
        }

        public void Lock()
        {
            AddFlags(ExitFlags.Locked);
        }

        #endregion

        public ExitBlueprint Blueprint { get; private set; }

        public string Name { get; }
        public IEnumerable<string> Keywords { get; }
        public string Description { get; }
        public IRoom Destination { get; private set; }
        public ExitFlags ExitFlags { get; private set; }

        public bool IsDoor => ExitFlags.HasFlag(ExitFlags.Door);
        public bool IsHidden => ExitFlags.HasFlag(ExitFlags.Hidden);

        public void OnRemoved()
        {
            Destination = null;
            Blueprint = null;
        }

        #endregion

        private void AddFlags(ExitFlags flags)
        {
            ExitFlags |= flags;
        }

        private void RemoveFlags(ExitFlags flags)
        {
            ExitFlags &= ~flags;
        }
    }
}
